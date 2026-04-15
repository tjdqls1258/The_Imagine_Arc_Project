#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static MapData;

/// <summary>
/// �� ������ UI ������ (MapEditorUI)
/// �� ������ ȭ���� ����� �������̽�(Ÿ�� �ȷ�Ʈ, Ÿ�� ���� ��)�� �����ϰ�
/// ������� �Է��� �޾� MapEditorManager�� �����ϴ� ������ �����մϴ�.
/// </summary>
public class MapEditorUI : MonoBehaviour
{
    // ====== Inspector References ======

    [Header("Manager Link")]
    [Tooltip("�ٽ� ���� �� ������ ������ ����ϴ� MapEditorManager�Դϴ�.")]
    [SerializeField] private MapEditorManager m_manager;

    [Header("UI Elements")]
    [Tooltip("Ÿ�� ��ư���� ��ġ�� UI Context (RectTransform)�Դϴ�. (Ÿ�� �ȷ�Ʈ)")]
    [SerializeField] private RectTransform m_context;

    [Tooltip("Ÿ�� �ȷ�Ʈ�� ����� ��ư �������Դϴ�. (TileItemButton ��ũ��Ʈ �ʿ�)")]
    [SerializeField] private TileItemButton m_tileItme;

    [Tooltip("���� ���õ� �� ������Ʈ Ÿ��(��: Wall, Floor)�� ǥ���ϴ� �ؽ�Ʈ�Դϴ�.")]
    [SerializeField] private Text m_currentTypeText;

    [Tooltip("Path ����϶��� Ȱ��ȭ �Ǵ� UI�Դϴ�.")]
    [SerializeField] private GameObject m_pathModeObject;

    // ====== Runtime State & Caches ======

    private SpriteAtlas m_atlas; // MapEditorManager�κ��� ���޹޴� SpriteAtlas ĳ��

    /// <summary> ���� ����ڰ� ������ Ÿ���� ��������Ʈ �̸��Դϴ�. </summary>
    private string m_currentSpriteName = string.Empty;

    /// <summary> ���� ����ڰ� ������ �� ������Ʈ�� Ÿ���Դϴ�. (�⺻��: Wall) </summary>
    private MapObject m_currentObjectType = MapObject.Wall;

    // ���� ���� ��� �߰� �ʵ�
    /// <summary> Ÿ�� �ȷ�Ʈ���� ������ ���õǾ� �����Ǿ��� ��ư �ν��Ͻ��� �����մϴ�. </summary>
    private TileItemButton m_selectedTileButton = null;

    /// <summary>
    /// Path ��� ���� Flag
    public bool pathMode 
    {
        get;
        private set;
    } = false;

    public int pathIndex
    {
        get;
        private set;
    } = 0;

    public bool pathRemoveMode
    {
        get;
        private set;
    } = false;

    
    // ----------------------------------------------------------------------
    // ## Initialization & Lifecycle
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // Manager�� �� UI �ν��Ͻ��� ����Ͽ� Manager -> UI ����� �����ϵ��� ����
        m_manager.SetUI(this);
        // �ʱ� �� Ÿ���� ȭ�鿡 ǥ��
        m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
    }

    // ----------------------------------------------------------------------
    // ## Manager Actions (UI ��ư ����)
    // ----------------------------------------------------------------------

    /// <summary>
    /// �� �ʱ�ȭ ��ư�� ����Ǿ�, �� �����͸� �缳���մϴ�.
    /// </summary>
    public void Init()
    {
        m_manager.InitMap();
    }

    /// <summary>
    /// �� �ʱ�ȭ/���� ��ư�� ����Ǿ�, ���� ��� Ÿ���� �����մϴ�.
    /// </summary>
    public void Clear()
    {
        m_manager.DeleteAll();
    }

    /// <summary>
    /// �� ������ �ε� �� Ÿ�� �ȷ�Ʈ�� ����/������Ʈ�մϴ�.
    /// (Load ��ư�� ����)
    /// </summary>
    [ContextMenu("TestCreate")]
    public void Load()
    {
        // 1. MapManager�� ���� MapData�� �ε��ϰų� �����մϴ�.
        m_manager.LoadMapData();

        // 2. Manager�κ��� SpriteAtlas�� ������ ĳ���մϴ�.
        m_atlas = m_manager.m_atlas;
        if (m_atlas == null)
        {
            Debug.LogError("[MapEditorUI] SpriteAtlas is not assigned in MapEditorManager.");
            return;
        }

        // 3. ��Ʋ���� ��� ��������Ʈ�� �����ɴϴ�.
        Sprite[] spList = new Sprite[m_atlas.spriteCount];
        m_atlas.GetSprites(spList);

        // 4. Ÿ�� �ȷ�Ʈ UI�� �����ϰų� ������Ʈ�մϴ�.
        for (int i = 0; i < m_atlas.spriteCount; i++)
        {
            TileItemButton currentButton;

            // ���� ������Ʈ ���� (Pool-like)
            if (m_context.childCount > i)
            {
                currentButton = m_context.GetChild(i).GetComponent<TileItemButton>();
                // ���� ��ư�� ã�� �̹����� �׼��� ������Ʈ
                currentButton.SetImage(spList[i], OnClickAction);
                currentButton.gameObject.SetActive(true); // Ȥ�� ��Ȱ��ȭ�Ǿ� �־��ٸ� Ȱ��ȭ
            }
            // �� ������Ʈ �ν��Ͻ�ȭ
            else
            {
                currentButton = Instantiate(m_tileItme, m_context);
                currentButton.SetImage(spList[i], OnClickAction);
                currentButton.gameObject.SetActive(true);
            }
        }

        // 5. ��������Ʈ �������� ���� ���� ������Ʈ�� ��Ȱ��ȭ�Ͽ� ��Ȱ�� Ǯ�� �Ӵϴ�.
        for (int i = m_atlas.spriteCount; i < m_context.childCount; i++)
        {
            m_context.GetChild(i).gameObject.SetActive(false);
        }

        // �ε� ��, �⺻������ ù ��° Ÿ���� �����ϰų� ������ ����� currentSpriteName�� ������ �� �ֽ��ϴ�.
    }

    /// <summary>
    /// �� ������ ���� ��ư�� ����Ǿ�, ���� ���� ���¸� �����մϴ�.
    /// </summary>
    public void Save()
    {
        m_manager.SaveMapData();
    }

    // ----------------------------------------------------------------------
    // ## Getters & Setters
    // ----------------------------------------------------------------------

    /// <summary>
    /// ���� ���õ� Ÿ���� ��������Ʈ �ν��Ͻ��� ��ȯ�մϴ�.
    /// </summary>
    public Sprite GetCurrentSprite()
    {
        if (m_atlas == null) return null;
        return m_atlas.GetSprite(m_currentSpriteName);
    }

    /// <summary>
    /// ���� ���õ� Ÿ���� ��������Ʈ �̸��� MapEditorManager�� �����մϴ�.
    /// </summary>
    public string GetCurrentSpriteName() => m_currentSpriteName;

    /// <summary>
    /// ���� ���õ� �� ������Ʈ Ÿ��(MapObject)�� MapEditorManager�� �����մϴ�.
    /// </summary>
    public MapObject GetCurrentType() => m_currentObjectType;

    /// <summary>
    /// ��Ӵٿ� �Ǵ� ��ư�� ���� �� ������Ʈ Ÿ���� �����մϴ�.
    /// </summary>
    /// <param name="type">���õ� MapObject�� ���� �� (Dropdown.value ��)</param>
    public void SetCurrentType(int type)
    {
        m_currentObjectType = (MapObject)type;

        pathMode = false;
        m_manager.PathModeOff();
        // UI �ؽ�Ʈ ������Ʈ
        m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
    }

    /// <summary>
    /// ������ �̵� ��θ� ���� ��� On/Off ��ư�Դϴ�.
    /// </summary>
    public void SetPathButton()
    {
        pathMode = !pathMode;

        m_pathModeObject.SetActive(pathMode);
        if (pathMode)
        {
            m_currentTypeText.text = $"Current Type : PathMode PATH_{pathIndex}";
            m_manager.PathModeOn(pathIndex);
        }
        else
        {
            m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
            m_manager.PathModeOff();
        }
    }

    public void AddPathCount(int count)
    {
        pathIndex = Math.Max(pathIndex + count, 0);
        m_currentTypeText.text = $"Current Type : PathMode PATH_{pathIndex}";
        m_manager.PathModeOn(pathIndex);
    }

    public void RemovePath()
    {
        m_manager.RemovePathData(pathIndex);
    }

    public void PathRemoevMode()
    {
        pathRemoveMode = !pathRemoveMode;
    }

    // ----------------------------------------------------------------------
    // ## UI Callbacks
    // ----------------------------------------------------------------------

    /// <summary>
    /// Ÿ�� �ȷ�Ʈ�� ��ư Ŭ�� �� ȣ��Ǵ� �ݹ� �Լ��Դϴ�.
    /// </summary>
    /// <param name="spriteName">Ŭ���� Ÿ�� ��ư�� ���� ��������Ʈ�� �̸��Դϴ�.</param>
    private void OnClickAction(string spriteName, TileItemButton tileItemButton)
    {
        // 1. ���� ���õ� ��������Ʈ �̸� ������Ʈ
        m_currentSpriteName = spriteName;

        // 2. Ÿ�� ��ư ���� ����        
        // Note: ���� ����ȭ�� ���� context.GetChild(i).GetComponent<TileItemButton>()�� ĳ���� �� �ֽ��ϴ�.

        // ���� ���õ� ��ư ���� ����
        if (m_selectedTileButton != null)
        {
            m_selectedTileButton.SetHighlight(false); // ���� ����
        }

        // ���� ���õ� ��ư�� ã�� ���� ����
        tileItemButton.SetHighlight(true); // ���� ����
        m_selectedTileButton = tileItemButton; // ���� ���õ� ��ư ����
    }
}
#endif