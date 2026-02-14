
# 🚀 Unity 6 Professional UI & InGame Framework

Unity 6(LTS) 기반의 **데이터 중심(Data-Driven)** 및 **비동기(Asynchronous)** 아키텍처를 지향하는 UI & 인게임 유닛 배치 시스템 프레임워크입니다. 리소스 최적화, 확장성 있는 UI 생명주기 관리, 그리고 에디터 툴을 통한 생산성 향상을 목표로 설계되었습니다.

---

## 🛠 Tech Stack

* **Engine:** **Unity 6 (LTS)**
* **Async Library:** [UniTask](https://github.com/Cysharp/UniTask)
* **Resource Management:** Addressables Asset System
* **Tweening:** DOTween
* **Serialization:** Newtonsoft.Json

---

## ✨ Key Features

### 1. Advanced UI Lifecycle Management

* **CachObject & Auto-Binding:** `Enum` 기반의 자동 컴포넌트 바인딩 시스템을 구축하여 하이어라키 탐색 비용을 최소화하고 타입 안전성을 확보했습니다.
* **Async UI Flow:** `UniTask`와 `Addressables`를 결합하여 리소스 로딩 시 메인 스레드 병목 현상을 제거하고 부드러운 화면 전환을 구현했습니다.
* **UI/Popup Stack:** 중앙 집중형 `UIManager`와 `PopupManager`를 통해 팝업의 중첩 관리 및 씬 전환 간의 상태 동기화를 제어합니다.

### 2. Editor Tooling (UIMakerTool)

* **UI Layout to JSON:** 하이어라키의 `RectTransform` 데이터를 실시간으로 추출하여 JSON으로 직렬화하거나, 데이터로부터 UI 레이아웃을 자동 복구하는 에디터 기능을 제공합니다.
* **Visual Debugging:** 커스텀 인스펙터를 통해 비개발 직군도 UI 레이아웃을 데이터화하고 관리할 수 있는 워크플로우를 구축했습니다.

### 3. InGame Interaction System

* **Drag & Drop Deployment:** UI 버튼 드래그를 통해 월드 맵 타일에 유닛을 배치하는 직관적인 시스템입니다.
* **Smart Snapping:** `Raycast2D`와 타일 레이어 마스크를 이용한 그리드 스냅 기능을 제공합니다.
* **Async Cooldowns:** 유닛 사망 후 재배치 대기 시간을 `UniTask` 루프로 처리하여 시각적인 쿨타임 게이지 연출을 구현했습니다.

---

## 🏗 System Architecture

### UI Inheritance Hierarchy

1. **CachObject:** 리플렉션과 제네릭을 이용한 컴포넌트 캐싱 엔진.
2. **UIBase:** UI의 기본 생명주기(`Init`, `Show`, `Close`) 및 오픈 애니메이션 정의.
3. **UIBaseFormMaker:** 에디터 자동 생성 툴 연동을 위한 마커 클래스.
4. **UILobbyUpdate:** 로비 전용 동적 데이터 업데이트(캐릭터 대사 루프 등) 지원.

---

## 💻 Core Code Preview

### Asynchronous UI Instantiation

`Addressables`를 통해 프리팹을 로드하고 데이터 수치를 즉시 적용하는 병렬 비동기 로직입니다.

```csharp
public async UniTask InstantiateObjectSetting(UIBaseData data, Transform parent)
{
    // 리소스 비동기 인스턴스화
    var obj = await AddressableManager.Instance.InstantiateObjectAsync(data.dataName, parent);
    if (obj == null) return;

    RectTransform rect = (RectTransform)obj.transform;
    
    // 데이터 기반 레이아웃 복원
    rect.anchorMin = data.GetAchorMinMax().min;
    rect.anchorMax = data.GetAchorMinMax().max;
    rect.anchoredPosition = data.GetAnchorPos();
    rect.sizeDelta = data.GetSizeDetail();
}

```

### Unit Deployment Snapping

드래그 시 타일을 감지하여 유닛을 타일 중앙으로 스냅하는 물리 레이캐스트 로직입니다.

```csharp
private void TrySnapToTile(Action hitAction, Action notHitAction)
{
    m_hit2D = Physics2D.Raycast(m_pointerWorldPosition, Vector3.forward, float.MaxValue, m_tileMask);

    if (m_hit2D.transform != null)
    {
        var spawnTile = m_hit2D.collider.gameObject.GetComponent<SpawnPlayerCharacterTile>();
        if (IsValidSpawnTile(spawnTile))
        {
            hitAction?.Invoke();
            return;
        }
    }
    notHitAction?.Invoke();
}

```

---

## 📄 Portfolio Perspective

이 프레임워크는 유니티 개발 환경에서 발생할 수 있는 **반복적인 UI 작업의 자동화**와 **런타임 퍼포먼스 최적화**를 해결하기 위해 제작되었습니다. 특히 **Unity 6**의 최신 환경에 맞춰 비동기 프로그래밍 기술을 적극 활용하여, 유지보수가 용이하고 확장성 있는 구조를 지향합니다.

---

## 📝 License

This project is licensed under the MIT License.

---

**다음 단계로 무엇을 도와드릴까요?**

* 프로젝트의 각 클래스를 설명하는 **상세 API 문서(Doxygen 스타일)**가 필요하신가요?
* 면접을 위한 **기술 면접 예상 질문 및 모범 답안 리스트**를 만들어 드릴까요?
