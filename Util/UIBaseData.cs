using System;
using UnityEngine;

/// <summary>
/// UI 요소의 배치 정보(RectTransform)를 직렬화하여 관리하기 위한 데이터 클래스입니다.
/// 앵커 위치, 크기, 피벗 등 UI 레이아웃 설정값을 저장하고 불러오는 기능을 제공합니다.
/// </summary>
[Serializable]
public class UIBaseData
{
    /// <summary> UI 패널이나 요소의 분류를 정의하는 타입입니다. </summary>
    public enum UIType
    {
        Command,  // 명령어/조작 관련 UI
        MainUI,   // 메인 로비나 메뉴 UI
        InGameUI, // 인게임 플레이 화면 UI
    }

    // ====== 데이터 식별 및 속성 ======
    public string dataName;    // 해당 UI 데이터의 고유 이름
    public UIType uiType;      // UI 분류 타입

    // ====== RectTransform 관련 상세 좌표 값 ======
    // (직렬화 호환성을 위해 Vector2 대신 개별 float 필드로 관리)

    public float anchorPosX;   // 앵커 기준 상대적 X 위치 (anchoredPosition.x)
    public float anchorPosY;   // 앵커 기준 상대적 Y 위치 (anchoredPosition.y)

    public float sizeDetailX;  // UI 요소의 너비 (sizeDelta.x)
    public float sizeDetailY;  // UI 요소의 높이 (sizeDelta.y)

    // 하단 필드들은 앵커 프리셋 및 피벗 설정을 위한 값들입니다.
    public float anchorminX;
    public float anchorminY;

    public float anchorsMinX;  // Anchor Min X (0~1 범위)
    public float anchorsMinY;  // Anchor Min Y (0~1 범위)
    public float anchorsMaxX;  // Anchor Max X (0~1 범위)
    public float anchorsMaxY;  // Anchor Max Y (0~1 범위)

    public float pivotX;       // UI 기준점 Pivot X (0~1 범위)
    public float pivotY;       // UI 기준점 Pivot Y (0~1 범위)

    // ----------------------------------------------------------------------
    // ## Helper Methods (Getter & Setter)
    // ----------------------------------------------------------------------

    /// <summary> 앵커 기준 위치(anchoredPosition)를 설정합니다. </summary>
    public void SettingAnchorPos(Vector2 anchor)
    {
        anchorPosX = anchor.x;
        anchorPosY = anchor.y;
    }

    /// <summary> 저장된 앵커 위치를 Vector2 형태로 반환합니다. </summary>
    public Vector2 GetAnchorPos()
    {
        return new Vector2(anchorPosX, anchorPosY);
    }

    /// <summary> UI 요소의 크기(sizeDelta)를 설정합니다. </summary>
    public void SettingSizeDetail(Vector2 sizeDetail)
    {
        sizeDetailX = sizeDetail.x;
        sizeDetailY = sizeDetail.y;
    }

    /// <summary> 저장된 UI 크기를 Vector2 형태로 반환합니다. </summary>
    public Vector2 GetSizeDetail()
    {
        return new Vector2(sizeDetailX, sizeDetailY);
    }

    /// <summary> 앵커의 최소/최대 가동 범위(Anchor Min/Max)를 설정합니다. </summary>
    public void SettingAnchorMinMax(Vector2 anchorsMin, Vector2 anchorsMax)
    {
        anchorsMinX = anchorsMin.x;
        anchorsMinY = anchorsMin.y;

        anchorsMaxX = anchorsMax.x;
        anchorsMaxY = anchorsMax.y;
    }

    /// <summary> 저장된 앵커 최소/최대 범위를 튜플(Tuple) 형태로 반환합니다. </summary>
    public (Vector2 min, Vector2 max) GetAchorMinMax()
    {
        return (new Vector2(anchorsMinX, anchorsMinY), new Vector2(anchorsMaxX, anchorsMaxY));
    }

    /// <summary> UI 기준점(Pivot) 위치를 설정합니다. </summary>
    public void SettingPivot(Vector2 pivot)
    {
        pivotX = pivot.x;
        pivotY = pivot.y;
    }

    /// <summary> 저장된 피벗 위치를 Vector2 형태로 반환합니다. </summary>
    public Vector2 GetPivot()
    {
        return new Vector2(pivotX, pivotY);
    }
}