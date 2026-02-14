🚀 Unity UI & InGame Framework
유니티 최신 기술 스택을 활용한 데이터 중심(Data-Driven) 및 비동기(Asynchronous) 기반의 UI & 인게임 유닛 배치 프레임워크입니다. 효율적인 리소스 관리와 확장성 있는 UI 생명주기 제어를 목적으로 설계되었습니다.

🛠 Tech Stack
Engine: Unity 6000.2.7f2 (LTS)

Async Library: UniTask

Resource Management: Addressables Asset System

Tweening: DOTween

Serialization: Newtonsoft.Json

✨ Key Features
1. Advanced UI Lifecycle Management
CachObject & Auto-Binding: Enum 기반의 자동 컴포넌트 바인딩 시스템을 통해 GetComponent 호출을 최소화하고 타입 안정성을 확보했습니다.

UI/Popup Stack: 팝업의 중첩 관리 및 씬 전환 간의 UI 시퀀스를 중앙 집중식 매니저(UIManager, PopupManager)로 제어합니다.

Async Workflow: 모든 UI 오픈/클로즈 로직에 UniTask를 적용하여 리소스 로딩 시 발생하는 메인 스레드 병목 현상을 제거했습니다.

2. Data-Driven System
JSON/CSV Serialization: 스테이지 구성, 캐릭터 스탯 및 UI 레이아웃 정보를 외부 데이터 파일로 관리하여 코드 수정 없이 콘텐츠를 확장할 수 있습니다.

ScriptableObject Integration: 게임 데이터를 유니티 에셋과 결합하여 직관적인 데이터 워크플로우를 제공합니다.

3. Editor Tooling (UIMakerTool)
UI Layout Tool: 현재 하이어라키의 UI 배치 정보를 실시간으로 추출하여 JSON 데이터로 변환하거나, 데이터로부터 UI를 자동 복구하는 에디터 툴을 포함합니다.

Custom Inspector: 커스텀 에디터를 통해 비개발 직군도 UI 레이아웃을 손쉽게 수정하고 저장할 수 있습니다.

4. InGame Interaction (Drag & Drop)
Unit Placement System: UI 버튼 드래그를 통해 월드 맵 타일에 유닛을 배치하는 시스템입니다.

Smart Snapping: Raycast2D를 이용한 타일 감지 및 그리드 스냅 기능을 제공합니다.

Cooldown System: 비동기 루프를 활용한 유닛 재배치 쿨타임 UI 연출이 포함되어 있습니다.

🏗 System Architecture
UI Inheritance Hierarchy
CachObject: 컴포넌트 캐싱 및 바인딩 엔진.

UIBase: UI의 기본 생명주기(Init, Show, Close) 및 애니메이션 제어.

PopupBase: 팝업 전용 콜백 및 스택 관리 로직 추가.

UIBaseFormMaker: 에디터 툴 연동을 위한 마커 클래스.

💻 Quick Start
Opening a Popup
C#
// 간단한 확인 팝업 호출 예시
PopupManager.Instance.ShowPopup(PopupManager.PopupType.Message, new object[] { 
    "알림", 
    "데이터 로드가 완료되었습니다." 
}).Forget();
Unit Deployment
C#
// 캐릭터 데이터를 버튼에 할당
unitButton.SetCharater(characterData, ingameUIManager);
🔍 Core Logic Preview: Async Component Binding
C#
// CachObject.cs 일부
protected void Bind<T>(Type type) where T : UnityEngine.Object
{
    string[] names = Enum.GetNames(type);
    for (int i = 0; i < names.Length; i++)
    {
        // GameUtil을 통해 하위 객체를 검색하고 캐싱
        T component = GameUtil.FindChild<T>(gameObject, names[i], true);
        _objects[typeof(T)][i] = component;
    }
}
📄 License
This project is licensed under the MIT License.

💡 Portfolio Note
이 프레임워크는 유니티 개발 환경에서 발생할 수 있는 반복적인 UI 작업의 자동화와 런타임 퍼포먼스 최적화를 해결하기 위해 제작되었습니다. 특히 비동기 프로그래밍 기술을 적극 활용하여 대규모 프로젝트에서도 안정적으로 동작하는 구조를 지향합니다.
