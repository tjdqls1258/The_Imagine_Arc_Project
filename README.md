🎮 Unity Defense Game Base
유니티를 이용한 확장 가능하고 최적화된 디펜스 게임 프레임워크입니다. 단순한 기능 구현을 넘어, 유지보수가 용이한 모듈형 아키텍처와 성능 최적화에 중점을 두어 설계되었습니다.

🚀 Key Features
1. Architecture & Design Patterns
Manager Pattern & Centralized Control: InGameManager를 중심으로 웨이브, 상태 관리, 유닛 생성을 중앙 집중식으로 제어하여 게임 흐름의 가독성을 높였습니다.

Decoupled Design (Interface-driven): TileClickEvent 등의 인터페이스를 활용하여 시스템 간 결합도를 낮추고, 특정 모듈의 수정이 전체 시스템에 미치는 영향을 최소화했습니다.

Data-Driven Design: ScriptableObject와 Google Sheet 연동 툴을 통해 기획 데이터를 코드와 분리하여 관리합니다. 수치 밸런싱과 콘텐츠 확장이 용이합니다.

2. Performance Optimization
Advanced Object Pooling: 유닛, 투사체, UI 요소 및 파티클 시스템에 객체 풀링을 적용하여 빈번한 Instantiate/Destroy로 인한 GC(Garbage Collection) 부하와 CPU 피크를 방지했습니다.

Asynchronous Resource Management: Addressables 시스템과 UniTask를 결합하여 에셋 로딩 시 메인 스레드 병목을 제거하고 메모리 점유율을 최적화했습니다.

Efficient Targeting: CircleCollider2D와 LINQ를 조합하여 다수의 적 유닛 사이에서 효율적인 타겟팅 로직을 구현했습니다.

3. Productivity Tools (Custom Editor)
Custom Map Editor: 개발 생산성을 높이기 위해 전용 맵 에디터 툴을 내장하여 레벨 디자인 프로세스를 자동화했습니다.

Data Converter: Google Spreadsheet 데이터를 게임 내 데이터셋으로 즉시 변환하는 자동화 툴을 포함하고 있습니다.

🛠 Tech Stack
Engine: Unity 2022.3+ (LTS)

Language: C#

Libraries:

UniTask: 고성능 비동기 처리

Addressables: 효율적인 에셋 관리 및 메모리 최적화

LINQ: 데이터 필터링 및 타겟팅 로직 최적화

📂 Project Structure
Plaintext
Assets
 ┣ 📂 Scripts
 ┃ ┣ 📂 Core          # 게임 엔진 및 매니저 클래스
 ┃ ┣ 📂 Units         # 유닛, 타워, 적 베이스 및 상속 클래스
 ┃ ┣ 📂 UI            # UI 매니저 및 캔버스 최적화 로직
 ┃ ┣ 📂 Data          # ScriptableObject 및 데이터 로더
 ┃ ┗ 📂 Editor        # 커스텀 맵 에디터 및 툴
 ┣ 📂 AddressableAssets # 최적화된 리소스 폴더
 ┗ 📂 Resources       # 기초 데이터 및 설정 파일
💡 Implementation Details
확장성: UnitBase 추상화를 통해 새로운 공격 패턴을 가진 유닛을 코드 수정 없이 추가할 수 있는 OCP(개방-폐쇄 원칙)를 준수했습니다.

최적화: 캔버스 부하를 줄이기 위한 이중 객체 풀링 기법을 적용하여 다량의 UI 텍스트 출력 시에도 안정적인 프레임을 유지합니다.
