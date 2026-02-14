using System;
using UnityEngine;

/// <summary>
/// 게임 내 메인 카메라의 이동과 줌 기능을 제어하는 클래스입니다.
/// 마우스 드래그를 통한 위치 이동과 휠 스크롤을 통한 배율 조정을 지원합니다.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ====== Camera Reference ======
    /// <summary> 유틸리티 클래스를 통해 캐싱된 메인 카메라를 참조합니다. </summary>
    Camera mainCam => GameUtil.mainCamera;

    // ====== Runtime State ======
    /// <summary> 카메라 이동 방향 벡터입니다. </summary>
    private Vector2 movedic;
    /// <summary> 마우스 드래그 계산을 위한 이전 프레임의 마우스 위치입니다. </summary>
    private Vector2 currentPosition = Vector2.zero;

    // ====== Inspector Settings ======
    [Header("Speed Settings")]
    [SerializeField] float zoomSpeed = 1f; // 줌 속도
    [SerializeField] float moveSpeed = 1f; // 이동 속도

    [Header("Zoom Constraints")]
    [SerializeField] float zoomMin = 5f;   // 최소 확대 크기 (Orthographic Size)
    [SerializeField] float zoomMax = 20f;  // 최대 축소 크기 (Orthographic Size)

    /// <summary> 카메라 조작 활성화 여부 플래그입니다. </summary>
    private bool cameraMoveModeOn = false;

    // ----------------------------------------------------------------------
    // ## Public Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부 UI나 버튼을 통해 카메라 조작 모드를 토글(Toggle)합니다.
    /// </summary>
    public void SetMoveMode()
    {
        cameraMoveModeOn = !cameraMoveModeOn;
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle & Input Logic
    // ----------------------------------------------------------------------

    public void Update()
    {
        // 이동 모드가 꺼져 있다면 조작을 처리하지 않습니다.
        if (cameraMoveModeOn)
            return;

        // 1. 카메라 이동 (드래그 조작)
        if (Input.GetMouseButton(0))
        {
            // 현재 마우스 위치와 이전 위치의 차이를 계산하여 방향 추출
            movedic = (currentPosition - ConvartVector(Input.mousePosition)).normalized;
            Vector3 vec = movedic;

            // 이동 방향에 속도를 곱하여 카메라 위치 갱신
            mainCam.transform.position += vec * moveSpeed;

            // 현재 위치를 기록하여 다음 프레임 계산에 사용
            currentPosition = ConvartVector(Input.mousePosition);
        }

        // 2. 줌 처리 (스크롤 조작)
        Zoom();
    }

    // ----------------------------------------------------------------------
    // ## Helper & Internal Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// Vector3 좌표를 Vector2로 변환하는 단순 헬퍼 메서드입니다. (Z값 무시)
    /// </summary>
    private Vector2 ConvartVector(Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    /// <summary>
    /// 마우스 휠 입력을 감지하여 카메라의 Orthographic Size를 조절합니다.
    /// </summary>
    private void Zoom()
    {
        // 마우스 휠 스크롤 값 가져오기 (위: +, 아래: -)
        float zoomValue = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // orthographicSize를 조절하고 설정된 최소/최대 범위 내로 클램핑(제한)합니다.
        // Size가 커질수록 화면이 멀어지고(축소), 작아질수록 가까워집니다(확대).
        mainCam.orthographicSize = Math.Clamp(mainCam.orthographicSize - zoomValue, zoomMin, zoomMax);
    }
}