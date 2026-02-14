#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 맵 에디터 환경에서 개별 타일의 편집 상호작용을 담당하는 클래스입니다.
/// 마우스 입력을 받아 타일의 스프라이트 변경, 타입 설정 및 경로 편집 기능을 수행합니다.
/// </summary>
public class TileEdtiorBase : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ====== Editor References & State ======

    /// <summary> 현재 에디터의 설정값(선택된 스프라이트, 모드 등)을 참조하기 위한 UI 객체입니다. </summary>
    public MapEditorUI mapEditorUI;

    /// <summary> 맵 데이터 상에서 이 타일이 위치한 좌표입니다. </summary>
    public Vector2Int currentPos;

    /// <summary> 타일이 클릭되었을 때 MapEditorManager에 알리기 위한 콜백 액션입니다. </summary>
    public UnityAction<Vector2Int> onclickEnter;

    /// <summary> 타일의 현재 상태(일반/삭제 등)를 시각적으로 나타내는 기본 색상입니다. </summary>
    private Color currentColor = Color.white;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 로드된 데이터를 기반으로 에디터 타일의 초기 비주얼 상태를 설정합니다.
    /// </summary>
    /// <param name="tileData">로드된 타일 정보</param>
    public void InitTileEdtiorBase(MapData.TileData tileData)
    {
        // 삭제 타입인 경우 검은색으로, 그 외에는 흰색으로 기본 색상을 지정합니다.
        if (tileData.type == MapData.MapObject.Delete)
            currentColor = Color.black;
        else
            currentColor = Color.white;

        SetTypeColor();
    }

    // ----------------------------------------------------------------------
    // ## Mouse Interaction (EventSystems)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일을 마우스로 클릭했을 때 호출됩니다.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
        {
            // 1. 경로 편집 모드(Path Mode)인 경우 데이터만 갱신하고 종료
            if (mapEditorUI.pathMode)
            {
                onclickEnter?.Invoke(currentPos);
                return;
            }

            // 2. 일반 타일 편집 모드: UI에서 선택된 스프라이트가 있는지 확인
            if (mapEditorUI.GetCurrentSprite() == null)
                return;

            // 3. 비주얼 업데이트: 타일의 이미지를 현재 브러시 스프라이트로 교체
            tileImage.sprite = mapEditorUI.GetCurrentSprite();

            // 4. 데이터 업데이트 요청 (Manager 호출)
            onclickEnter?.Invoke(currentPos);

            // 5. 타입에 따른 타일 색상 상태 갱신 (삭제 브러시 여부 판단)
            if (mapEditorUI.GetCurrentType() == MapData.MapObject.Delete)
                currentColor = Color.black;
            else
                currentColor = Color.white;

            SetTypeColor();
        }
    }

    /// <summary>
    /// 마우스 커서가 타일 영역 안으로 들어왔을 때 호출됩니다. (하이라이트 효과)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
            tileImage.color = Color.red; // 현재 편집 중인 타일을 빨간색으로 강조
    }

    /// <summary>
    /// 마우스 커서가 타일 영역 밖으로 나갔을 때 호출됩니다. (하이라이트 해제)
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
            SetTypeColor(); // 원래의 상태 색상으로 복구
    }

    /// <summary>
    /// 클릭 후 마우스 버튼을 뗐을 때 호출됩니다.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
        {
            return;
        }
    }

    // ----------------------------------------------------------------------
    // ## Helper Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일의 렌더러 색상을 현재 결정된 기본 색상(currentColor)으로 적용합니다.
    /// </summary>
    private void SetTypeColor()
    {
        tileImage.color = currentColor;
    }
}
#endif