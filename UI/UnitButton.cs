using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 버튼을 드래그하여 유닛을 배치하고, 배치 후에는 스킬 버튼으로 전환되는 클래스입니다.
/// </summary>
public class UnitButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    // ==========================================
    // [1] Inspector References
    // ==========================================
    [Header("UI References")]
    [SerializeField] private Image m_characterImage;
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private GameObject m_blockImage;

    [Header("Skill References")]
    [SerializeField] private Button m_skillButton;
    [SerializeField] private TextMeshProUGUI m_coolTimeText; // m_lateTimeText -> m_coolTimeText 직관적으로 변경

    [Header("Character Data")]
    [SerializeField] private InGameCharacterData m_characterData;

    // ==========================================
    // [2] Cached & State Variables
    // ==========================================
    private InGameUIManager m_inGameUIManager;
    private PlayerCharacterContrroller m_previewCharacter;

    private readonly LayerMask m_tileMask = 1 << 8;
    private Camera MainCamera => GameUtil.mainCamera;

    // 상태 플래그
    private bool m_isDragging = false;
    private bool m_isSpawned = false; // 맵에 배치되어 스킬 모드로 전환되었는지 여부
    private bool m_hasEnoughCost = false;

    // 스킬 및 비동기 제어
    private CancellationTokenSource m_cancelTokenSource;
    private float m_skillReadyTime; // 스킬을 다시 사용할 수 있는 시간

    // ==========================================
    // [3] Initialization & Data Setup
    // ==========================================
    private void Awake()
    {
        SetBlockState(true);
        m_skillButton.onClick.AddListener(UseSkill);
    }

    public void SetCharacter(InGameCharacterData characterData, InGameUIManager ingameManager = null)
    {
        m_characterData = characterData;
        m_inGameUIManager = ingameManager;
        m_costText.text = m_characterData.characterData.cost.ToString();

        // 비동기 작업 취소 토큰 초기화
        ResetCancellationToken();

        // 리소스 로딩
        m_characterData.characterData.LoadSprite().Forget();
        CreateCharacterPreviewAsync().Forget();
    }

    private void ResetCancellationToken()
    {
        m_cancelTokenSource?.Cancel();
        m_cancelTokenSource?.Dispose();
        m_cancelTokenSource = new CancellationTokenSource();
    }

    public void DeleteData()
    {
        m_cancelTokenSource?.Cancel();

        m_characterData?.characterData.UnloadAtlas();
        if (m_previewCharacter != null)
        {
            Destroy(m_previewCharacter.gameObject);
            m_previewCharacter = null;
        }

        m_isSpawned = false;
        ToggleSkillMode(false);
        SetBlockState(true);
    }

    // ==========================================
    // [4] Drag & Drop Logic (유닛 배치)
    // ==========================================
    public void OnPointerDown(PointerEventData eventData)
    {
        // Todo: 설치 가능한 타일 하이라이트 기능 구현 예정
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_isSpawned || !m_hasEnoughCost || m_previewCharacter == null) return;

        m_isDragging = true;
        m_previewCharacter.gameObject.SetActive(true);

        UpdatePreviewPosition(eventData.position);
        m_previewCharacter.SetSpawn(false);
        m_previewCharacter.AtkAreaActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_isDragging || m_previewCharacter == null) return;

        UpdatePreviewPosition(eventData.position);

        TrySnapToTile(
            onHit: (hitPos) => m_previewCharacter.transform.position = hitPos,
            onFail: () => m_previewCharacter.transform.position = GetWorldPosition(eventData.position)
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!m_isDragging || m_previewCharacter == null) return;

        m_isDragging = false;
        m_previewCharacter.AtkAreaActive(false);

        TrySnapToTile(
            onHit: (hitPos) => TrySpawnCharacter(),
            onFail: () => m_previewCharacter.gameObject.SetActive(false)
        );
    }

    private void UpdatePreviewPosition(Vector2 screenPosition)
    {
        m_previewCharacter.transform.position = GetWorldPosition(screenPosition);
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = MainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;
        return worldPos;
    }

    private void TrySnapToTile(Action<Vector3> onHit, Action onFail)
    {
        Vector3 currentPos = m_previewCharacter.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(currentPos, Vector3.forward, float.MaxValue, m_tileMask);

        if (hit.collider != null)
        {
            var spawnTile = hit.collider.GetComponent<SpawnPlayerCharacterTile>();
            if (spawnTile != null && !spawnTile.CheckSpawn())
            {
                onHit?.Invoke(hit.transform.position);
                return;
            }
        }

        onFail?.Invoke();
    }

    // ==========================================
    // [5] Spawn Logic
    // ==========================================
    private async UniTask CreateCharacterPreviewAsync()
    {
        if (m_previewCharacter != null)
        {
            Destroy(m_previewCharacter.gameObject);
        }

        string path = string.Format(Util.CHARACTER_MODLED_PATH, m_characterData.characterData.modelObjectName);
        var obj = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(path);

        m_previewCharacter = obj.GetComponent<PlayerCharacterContrroller>();
        m_previewCharacter.SetCharacter(m_characterData);
        m_previewCharacter.AddDieAction(HandleCharacterDie);
        m_previewCharacter.gameObject.SetActive(false);

        m_skillButton.image.sprite = m_characterData.activeSkill.SkillIcon;
        ToggleSkillMode(false);
    }

    private void TrySpawnCharacter()
    {
        Vector3 currentPos = m_previewCharacter.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(currentPos, Vector3.forward, float.MaxValue, m_tileMask);

        if (hit.collider == null) return;

        var spawnTile = hit.collider.GetComponent<SpawnPlayerCharacterTile>();

        if (spawnTile == null || spawnTile.CheckSpawn() || !spawnTile.CheckSpawnPoint(false))
        {
            m_previewCharacter.gameObject.SetActive(false);
            return;
        }

        if (!m_inGameUIManager.m_inGameManager.UseCost(m_characterData.characterData.cost))
        {
            m_previewCharacter.gameObject.SetActive(false);
            return;
        }

        // 배치 완료
        spawnTile.SpawnUnit(m_previewCharacter);
        m_previewCharacter.enabled = true;
        m_previewCharacter.SetSpawn(true);

        ToggleSkillMode(true);
    }

    private void HandleCharacterDie()
    {
        // 유닛 사망 시 재배치 대기 시간(쿨타임) 적용
        StartRespawnCooldownAsync(5f).Forget();
    }

    private async UniTask StartRespawnCooldownAsync(float delay)
    {
        m_isSpawned = true; // 쿨타임 중 드래그 방지
        ToggleSkillMode(false); // 사망했으므로 스킬 UI는 끔

        // 빈 while문 대신 UniTask.Delay 사용 (훨씬 최적화됨)
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: m_cancelTokenSource.Token);

        m_isSpawned = false; // 쿨타임 종료, 다시 배치 가능
    }

    // ==========================================
    // [6] Cost & UI State Logic
    // ==========================================
    public void UpdateCostAction(int currentCost)
    {
        if (m_previewCharacter == null || m_previewCharacter.CheckSpawn()) return;

        m_hasEnoughCost = currentCost >= m_characterData.characterData.cost;

        // 배치되지 않은 상태일 때만 블록 UI 업데이트
        if (!m_isSpawned)
        {
            SetBlockState(!m_hasEnoughCost);
        }
    }

    private void SetBlockState(bool isBlocked)
    {
        m_hasEnoughCost = !isBlocked;
        m_blockImage.SetActive(isBlocked);
    }

    // ==========================================
    // [7] Skill Logic
    // ==========================================
    private void ToggleSkillMode(bool isActive)
    {
        m_isSpawned = isActive;

        m_skillButton.gameObject.SetActive(isActive);
        m_coolTimeText.gameObject.SetActive(isActive);
        m_costText.gameObject.SetActive(!isActive);

        if (isActive)
        {
            UpdateSkillCooldownTask().Forget();
        }
    }

    private void UseSkill()
    {
        if (m_previewCharacter.Skill() == false) return;
        m_skillReadyTime = m_previewCharacter.GetLastSkillTime() + m_previewCharacter.GetSkillCoolTime();

        SetSkillUIState(isOnCooldown: true);
        UpdateSkillCoolTimeText();
    }

    private async UniTask UpdateSkillCooldownTask()
    {
        while (!m_cancelTokenSource.IsCancellationRequested)
        {
            await UniTask.WaitForEndOfFrame(this.GetCancellationTokenOnDestroy());

            if (!m_isSpawned) break; // 맵에서 사라지면 루프 종료

            if (m_skillReadyTime <= Time.time)
            {
                SetSkillUIState(isOnCooldown: false);
            }
            else
            {
                UpdateSkillCoolTimeText();
            }
        }
    }

    private void UpdateSkillCoolTimeText()
    {
        m_coolTimeText.text = (m_skillReadyTime - Time.time).ToString("N1");
    }

    private void SetSkillUIState(bool isOnCooldown)
    {
        m_coolTimeText.gameObject.SetActive(isOnCooldown);
        m_blockImage.SetActive(isOnCooldown);
        m_skillButton.interactable = !isOnCooldown;
    }
}