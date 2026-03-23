### 🎮 Unity Defense Game Base (수정본)

유니티를 이용한 **확장 가능하고 최적화된 디펜스 게임 프레임워크**입니다. 불필요한 복잡성을 줄이고, 실무에서 즉시 활용 가능한 핵심 시스템 구축에 집중했습니다.

## 🚀 핵심 설계 전략 (Core Architecture)

### 1. 직관적인 게임 라이프사이클 관리
* **Centralized Game Loop**: `InGameManager`를 통해 게임의 시작, 웨이브 진행, 승리/패배 조건을 중앙에서 명확하게 제어합니다. 복잡한 상태 머신 없이도 로직의 흐름을 한눈에 파악할 수 있는 직관적인 구조를 지향합니다.
* **이벤트 기반 시스템**: 각 매니저 간의 강한 결합을 피하기 위해 인터페이스와 이벤트를 활용하여, 유닛의 사망이나 웨이브 종료 등의 상황을 유연하게 전파합니다.
* **데이터 주도 설계 (Data-Driven)**: ScriptableObject와 Google Sheet 자동 변환 툴을 연동하여, 코드 수정 없이 에디터 환경에서 밸런싱과 데이터 확장이 가능합니다.

### 2. 성능 최적화 (Performance Optimization)
* **고급 객체 풀링 (Object Pooling)**: 빈번하게 생성/파괴되는 유닛, 투사체, UI 요소에 객체 풀링을 적용하여 **GC(Garbage Collection) 부하와 CPU 피크를 방지**했습니다.
* **비동기 리소스 관리 (Addressables + UniTask)**: `Addressables` 시스템으로 메모리 점유율을 효율적으로 관리하고, `UniTask`를 결합하여 로딩 및 연산 시 발생하는 프레임 드랍을 최소화했습니다.
* **타겟팅 로직 최적화**: `CircleCollider2D`와 LINQ를 조합하여 다수의 적 유닛 사이에서 효율적으로 타겟을 탐색하도록 구현했습니다.

### 3. 개발 생산성 도구 (Custom Tooling)
* **Custom Map Editor**: 전용 맵 에디터를 제작하여 레벨 디자인 프로세스를 자동화하고 개발 시간을 단축했습니다.
* **Data Converter**: 외부 스프레드시트 데이터를 게임 데이터셋으로 즉시 변환하는 자동화 파이프라인을 구축했습니다.

## 🛠 Tech Stack
* **Engine**: Unity 2022.3+ (LTS)
* **Language**: C#
* **Asynchronous**: UniTask
* **Resource Management**: Addressables
* **Optimization**: Advanced Object Pooling, UI Double Pooling

## 📂 Project Structure
```text
Assets
 ┣ 📂 Scripts
 ┃ ┣ 📂 Core          # 게임 시스템 및 상태 관리 (Manager Classes)
 ┃ ┣ 📂 Units         # 유닛/타워/적 시스템 (Inheritance Structure)
 ┃ ┣ 📂 UI            # UI 시스템 및 캔버스 최적화 로직
 ┃ ┣ 📂 Data          # ScriptableObject 및 데이터 로더
 ┃ ┗ 📂 Editor        # 커스텀 맵 에디터 및 변환 툴
 ┣ 📂 Addressables     # 메모리 최적화 에셋 관리
 ┗ 📂 Resources       # 기초 설정 및 게임 데이터
```
