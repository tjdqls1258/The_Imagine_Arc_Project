using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 버튼을 드래그하여 유닛(캐릭터)을 맵 타일에 배치하는 기능을 담당합니다.
/// IBeginDrag, IDrag, IEndDrag 인터페이스를 사용하여 유닛 배치 과정을 제어합니다.
/// </summary>
public class UnitButton : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerDownHandler, IBeginDragHandler
{
    // ====== Editor References (Unity Inspector) ======
    [Header("UI References")]
    [SerializeField] private Image m_characterImage;
    [SerializeField] private Image m_blockImage; // 쿨타임 또는 사용 불가 상태를 표시하는 오버레이 이미지

    [SerializeField] private TextMeshProUGUI m_costText;

    [Header("Character Data")]
    [SerializeField] private InGameCharacterData m_characterData; // 버튼에 할당된 캐릭터 데이터

    private InGameUIManager m_inGameUIManager;

    // ====== Cached Components and Data ======
    // m_tileMask는 Tile 레이어(Layer 8)에 대해서만 레이캐스트를 수행합니다.
    private readonly LayerMask m_tileMask = 1 << 8;
    private PlayerData PlayerDataInstance => PlayerData.Instance; // PlayerData 싱글톤 인스턴스
    private Camera MainCamera => GameUtil.mainCamera; // 메인 카메라 캐시

    // ====== Runtime State ======
    private PlayerCharacterContrroller m_previewCharacter; // 드래그 시 표시되는 유닛의 프리뷰 인스턴스
    private Vector3 m_pointerWorldPosition; // 현재 포인터의 월드 좌표 위치
    private RaycastHit2D m_hit2D; // Physics2D.Raycast의 마지막 결과
    private bool m_startDrag = false;

    private bool m_isUnitSpawned = false; // 유닛이 현재 맵에 배치되었는지 (또는 쿨타임 중인지) 여부

    private void Awake()
    {
        ActiveBlockButton(true);
    }

    /// <summary>
    /// 버튼에 캐릭터 데이터를 할당하고, 해당 캐릭터의 프리뷰 인스턴스를 비동기로 생성합니다.
    /// </summary>
    public void SetCharater(InGameCharacterData characterData, InGameUIManager ingameManager = null)
    {
        m_characterData = characterData;
        m_characterData.characterData.LoadSprite().Forget();
        CreateCharacterPreviewAsync().Forget(); // Addressables 로딩을 비동기로 시작하고 결과를 기다리지 않음

        m_inGameUIManager = ingameManager;

        m_costText.text = m_characterData.characterData.cost.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Todo: 설치 가능한 타일 하이라이트 기능 구현 예정
    }

    // ====== Drag Handlers ======

    /// <summary>
    /// 드래그 시작 시: 유닛 배치 가능 상태인지 확인 후, 프리뷰 캐릭터를 활성화하고 드래그 위치로 이동시킵니다.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsUnitSpawned() || m_previewCharacter == null) return;

        m_startDrag = true;
        // 1. 프리뷰 캐릭터 활성화 및 위치 초기화
        m_previewCharacter.gameObject.SetActive(true);
        SetPointerWorldPosition(eventData.position);
        m_previewCharacter.transform.position = m_pointerWorldPosition;

        // 2. 프리뷰 상태이므로 공격/로직은 비활성화
        m_previewCharacter.SetSpawn(false);
        m_previewCharacter.AtkAreaActive(true);
    }

    /// <summary>
    /// 드래그 중: 프리뷰 캐릭터를 포인터 위치로 이동시키고, 유효한 타일 위에 있을 경우 타일 중앙으로 스냅합니다.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (IsUnitSpawned() || m_previewCharacter == null || m_startDrag == false) return;

        SetPointerWorldPosition(eventData.position);
        m_previewCharacter.transform.position = m_pointerWorldPosition; // 임시로 포인터 위치로 이동

        TrySnapToTile(eventData.position,
            // 히트 성공 및 타일 유효 시
            () =>
            {
                // 캐릭터 위치를 히트한 타일의 중앙으로 스냅
                m_previewCharacter.transform.position = m_hit2D.transform.position;
            },
            // 히트 실패 또는 타일 유효성 검사 실패 시
            () =>
            {
                // 포인터 위치를 유지 (스냅 해제)
                m_previewCharacter.transform.position = m_pointerWorldPosition;
            });
    }

    /// <summary>
    /// 드래그 종료 시: 유닛 배치 가능 상태인지 확인하고, 유효한 타일 위에 있을 경우 유닛을 배치합니다.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsUnitSpawned() || m_previewCharacter == null || m_startDrag == false) return;

        m_startDrag = false;
        SetPointerWorldPosition(eventData.position);

        TrySnapToTile(eventData.position,
          CharacterSpawn, // 히트 성공 및 유효성 통과 시: 유닛 배치 실행
                () => m_previewCharacter.gameObject.SetActive(false)); // 실패 시: 프리뷰 비활성화

        m_previewCharacter.AtkAreaActive(false);
    }

    // ====== Utility Methods ======

    /// <summary>
    /// 포인터 월드 위치에서 Raycast를 쏘아 타일 히트를 확인하고, 결과를 기반으로 액션을 실행합니다.
    /// </summary>
    private void TrySnapToTile(Vector2 pointerScreenPos, Action hitAction, Action notHitAction = null)
    {
        // 월드 포지션에서 전방(forward)으로 레이캐스트를 쏴서 m_tileMask 레이어의 객체를 찾습니다.
        m_hit2D = Physics2D.Raycast(m_pointerWorldPosition, Vector3.forward, float.MaxValue, m_tileMask);

        if (m_hit2D.transform != null)
        {
            var spawnTile = m_hit2D.collider.gameObject.GetComponent<SpawnPlayerCharacterTile>();

            // 타일이 유효성 검사를 통과하는지 확인
            if (IsValidSpawnTile(spawnTile))
            {
                hitAction?.Invoke();
            }
            else
            {
                notHitAction?.Invoke(); // 유효하지 않은 타일
            }
        }
        else
        {
            notHitAction?.Invoke(); // 히트 실패
        }
    }

    /// <summary>
    /// 특정 타일 컴포넌트가 유닛 스폰을 위한 유효한 타일인지 확인합니다.
    /// </summary>
    private bool IsValidSpawnTile(SpawnPlayerCharacterTile spawnTile)
    {
        // 타일 객체가 존재하고, 아직 유닛이 스폰되지 않은 상태여야 합니다.
        return spawnTile != null && spawnTile.CheckSpawn() == false;
    }

    /// <summary>
    /// Addressables를 사용하여 유닛의 프리뷰 인스턴스를 비동기로 생성하고 설정합니다.
    /// </summary>
    private async UniTask CreateCharacterPreviewAsync()
    {
        if (m_previewCharacter != null)
        {
            DestroyImmediate(m_previewCharacter);
            m_previewCharacter = null;
        }
        // AddressableManager를 통해 객체를 비동기로 인스턴스화
        var obj = await AddressableManager.Instance.InstantiateObjectAsync(string.Format(Util.CHARACTER_MODLED_PATH, m_characterData.characterData.modelObjectName));

        m_previewCharacter = obj.GetComponent<PlayerCharacterContrroller>();
        m_previewCharacter.SetCharacter(m_characterData);
        m_previewCharacter.AddDieAction(HandleCharacterDie); // 유닛 사망 시 쿨타임 처리를 위한 액션 등록
        m_previewCharacter.gameObject.SetActive(false); // 생성 직후에는 비활성화 상태
    }

    /// <summary>
    /// 배치된 유닛이 사망했을 때 호출되는 콜백입니다. 쿨타임 처리를 시작합니다.
    /// </summary>
    private void HandleCharacterDie()
    {
        StartCoolDownAsync(5f).Forget(); // 5초 쿨타임을 비동기로 시작
    }

    /// <summary>
    /// 지정된 지연 시간 동안 UI 이미지 fillAmount를 업데이트하여 쿨타임을 시각적으로 표시합니다.
    /// </summary>
    private async UniTask StartCoolDownAsync(float delay)
    {
        m_isUnitSpawned = true; // 쿨타임 동안 유닛 배치 불가 상태로 설정

        float current = delay;
        while (current >= 0)
        {
            await UniTask.WaitForEndOfFrame(); // 매 프레임 업데이트
            current -= Time.deltaTime;

            // 쿨타임 진행률을 오버레이 이미지에 표시 (1에서 0으로 감소)
            m_blockImage.fillAmount = current / delay;
        }

        m_isUnitSpawned = false; // 쿨타임 종료, 스폰 가능 상태로 복귀
        m_blockImage.fillAmount = 0;
    }

    /// <summary>
    /// PointerEventData의 스크린 좌표를 월드 좌표로 변환하여 저장합니다. (Z축 0 고정)
    /// </summary>
    private void SetPointerWorldPosition(Vector2 screenPosition)
    {
        m_pointerWorldPosition = MainCamera.ScreenToWorldPoint(screenPosition);
        m_pointerWorldPosition.z = 0;
    }

    /// <summary>
    /// 유닛 버튼 사용 불가 상태(스폰됨 또는 쿨타임 중)인지 확인합니다.
    /// </summary>
    private bool IsUnitSpawned() => m_isUnitSpawned;

    /// <summary>
    /// 프리뷰 유닛을 유효한 타일 위에 정식으로 배치하고 활성화합니다.
    /// </summary>
    private void CharacterSpawn()
    {
        // 최종 유효성 검사 (타일이 유효하고, 스폰 지점이 사용 가능한지)
        var spawnTile = m_hit2D.collider.gameObject.GetComponent<SpawnPlayerCharacterTile>();
        if (!IsValidSpawnTile(spawnTile) || spawnTile.CheckSpawnPoint(false) == false || CheckCost() == false)
        {
            m_previewCharacter.gameObject.SetActive(false);
            return;
        }

        // 1. 유닛 배치
        spawnTile.SpawnUnit(m_previewCharacter);
        m_previewCharacter.enabled = true; // 유닛의 로직 활성화
        m_previewCharacter.SetSpawn(true); // 유닛의 내부 스폰 로직 실행

        ActiveBlockButton(true);
    }

    public void DeleteData()
    {
        m_characterData.characterData.UnloadAtlas();
        Destroy(m_previewCharacter);
        m_previewCharacter = null;
        m_isUnitSpawned = false; // 쿨타임 종료, 스폰 가능 상태로 복귀
        m_blockImage.fillAmount = 0;
        ActiveBlockButton(true);
    }

    private bool CheckCost()
    {
        if (m_inGameUIManager.m_inGameManager.UseCost(m_characterData.characterData.cost) == false)
        {
            Logger.Log("코스트 부족!");
            return false;
        }

        return true;
    }

    public void UpdateCostAction(int cost)
    {
        if (m_previewCharacter == null || m_previewCharacter.CheckSpawn())
            return;

        if (m_characterData.characterData.cost > cost && m_isUnitSpawned == false)
        {
            ActiveBlockButton(true);
        }
        else if (m_characterData.characterData.cost <= cost)
        {
            ActiveBlockButton(false);
        }
    }

    private void ActiveBlockButton(bool Active)
    {
        m_isUnitSpawned = Active; // 유닛 배치 상태로 전환 (쿨타임 시작까지 유지)
        m_blockImage.fillAmount = Active ? 1 : 0; // 쿨타임 UI를 즉시 꽉 채움 (DieAction에서 감소 시작)
    }
}
