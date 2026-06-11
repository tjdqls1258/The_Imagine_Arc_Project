using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 유닛 배치 및 스킬 사용을 관리하는 UI 버튼입니다. 
/// 모든 실제 로직은 State(상태 패턴) 클래스들에게 위임합니다.
/// </summary>
public class UnitButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private Image m_characterImage;
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private GameObject m_blockImage;

    [Header("Skill References")]
    [SerializeField] private Button m_skillButton;
    [SerializeField] private TextMeshProUGUI m_coolTimeText;

    public Camera MainCamera => GameUtil.mainCamera;
    public LayerMask TileMask => 1 << 8;

    public InGameCharacterData CharacterData { get; private set; }
    public InGameUIManager InGameUIManager { get; private set; }
    public PlayerCharacterController PreviewCharacter { get; private set; }
    public bool HasEnoughCost { get; private set; }
    public float SkillReadyTime { get; set; }

    private CancellationTokenSource m_cancelTokenSource;

    private IButtonState m_currentState;


    public SpawnReadyState SpawnReady { get; private set; }
    public SpawnDragState SpawnDrag { get; private set; }
    public SkillIdleState SkillIdle { get; private set; }
    public SkillDragState SkillDrag { get; private set; }
    public SpawnCoolTimeState SpawnCoolTimeState { get; private set; }

    private void Awake()
    {
        SpawnReady = new SpawnReadyState(this);
        SpawnDrag = new SpawnDragState(this);
        SkillIdle = new SkillIdleState(this);
        SkillDrag = new SkillDragState(this);
        SpawnCoolTimeState = new SpawnCoolTimeState(this);

        SetBlockState(true);
    }

    public void SetCharacter(InGameCharacterData characterData, InGameUIManager ingameManager = null)
    {
        CharacterData = characterData;
        InGameUIManager = ingameManager;
        m_costText.text = CharacterData.characterData.cost.ToString();

        ResetCancellationToken();

        CharacterData.characterData.LoadSprite().Forget();
        CreateCharacterPreviewAsync().Forget();
    }

    private async UniTask LoadSpriteSetButtonImage()
    {
        await CharacterData.characterData.GetCharacterSpriteFace(targetImage: m_characterImage);
    }

    public void ChangeState(IButtonState newState)
    {
        m_currentState?.Exit();
        m_currentState = newState;
        m_currentState?.Enter();
    }

    private void Update() => m_currentState?.Update();
    public void OnPointerDown(PointerEventData e) => m_currentState?.OnPointerDown(e);
    public void OnPointerUp(PointerEventData e) => m_currentState?.OnPointerUp(e);
    public void OnBeginDrag(PointerEventData e) => m_currentState?.OnBeginDrag(e);
    public void OnDrag(PointerEventData e) => m_currentState?.OnDrag(e);
    public void OnEndDrag(PointerEventData e) => m_currentState?.OnEndDrag(e);

    public bool IsSkillReady => SkillReadyTime <= Time.time;
    public void UpdateSkillCoolTimeText() => m_coolTimeText.text = (SkillReadyTime - Time.time).ToString("N1");

    public void UpdateCoolTimeText(float time) => m_coolTimeText.text = time.ToString("N1");

    private async UniTask CreateCharacterPreviewAsync()
    {
        if (PreviewCharacter != null) Destroy(PreviewCharacter.gameObject);

        string path = string.Format(Util.CHARACTER_MODLED_PATH, CharacterData.characterData.modelObjectName);
        var obj = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(path);
        await LoadSpriteSetButtonImage();

        PreviewCharacter = obj.GetComponent<PlayerCharacterController>();
        PreviewCharacter.SetCharacter(CharacterData);

        PreviewCharacter.AddDieAction(() =>
        { 
            ChangeState(SpawnCoolTimeState);
            PreviewCharacter.gameObject.SetActive(false);
        });
        PreviewCharacter.gameObject.SetActive(false);

        m_skillButton.image.sprite = CharacterData.activeSkill.SkillIcon;

        ChangeState(SpawnReady);
    }

    public void UpdateCostAction(int currentCost)
    {
        if (PreviewCharacter == null || PreviewCharacter.IsSpawn()) return;

        HasEnoughCost = currentCost >= CharacterData.characterData.cost;

        if (m_currentState == SpawnReady)
        {
            SetBlockState(!HasEnoughCost);
        }
    }

    public void SetBlockState(bool isBlocked)
    {
        HasEnoughCost = !isBlocked;
        m_blockImage.SetActive(isBlocked);
    }

    public void ToggleSkillUI(bool isSkillMode)
    {
        m_skillButton.gameObject.SetActive(isSkillMode);
        m_coolTimeText.gameObject.SetActive(isSkillMode);
        m_costText.gameObject.SetActive(!isSkillMode);
    }

    private void ResetCancellationToken()
    {
        m_cancelTokenSource?.Cancel();
        m_cancelTokenSource?.Dispose();
        m_cancelTokenSource = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        m_cancelTokenSource?.Cancel();
        m_cancelTokenSource?.Dispose();
    }

    public void DeleteData()
    {
        m_cancelTokenSource?.Cancel();

        CharacterData?.characterData.UnloadAtlas();

        if (PreviewCharacter != null)
        {
            Destroy(PreviewCharacter.gameObject);

            PreviewCharacter = null;
        }

        ToggleSkillUI(false);
        SetBlockState(true);
    }

    public void UpdateSkillContextPosition(SkillContext context, Vector2 screenPosition)
    {
        if (context == null) return;

        Vector3 worldPos = MainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;

        context.TargetPosition = worldPos;
    }
}