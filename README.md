# 🛡️ [The Imagine Arc] - 2D Subculture Defense RPG Framework

## 🚀 Key Features & Architecture

### 1. 📊 Data-Driven Growth System (데이터 주도형 이원화 성장)

로비(아웃게임)에서의 영구 성장과 전투(인게임)에서의 임시 성장을 완벽하게 분리하여 데이터 오염을 원천 차단했습니다.

* **로비 성장 (Meta Progression):** `GrowthDataSO`를 활용하여 레벨업, 랭크업에 따른 스탯을 캐싱 처리(Dirty Flag 패턴)하여 메모리 할당(GC) 없이 UI에 즉각 반영합니다.
* **인게임 강화 (In-Game Upgrade):** `InGameUpgradeStrategySO` (전략 패턴)를 통해 전투 중 획득한 재화로 유닛을 강화하며, 전투 종료 시 객체 파괴와 함께 깔끔하게 초기화됩니다.

### 2. ⚔️ GAS-based Combat & Attribute System (GAS 기반 전투 스탯 시스템)

기존 데코레이터 패턴의 한계(버프 해제 및 관리의 어려움)를 극복하기 위해, 리스트 기반의 모디파이어(Modifier) 시스템을 자체 구축했습니다.

* **`IStatProvider` & `ConditionBuffeManager`:** 원본 베이스 스탯에 현재 걸려있는 버프/디버프(Condition) 리스트를 합산하여 최종 스탯을 동적으로 계산합니다.
* **Payload 기반 이펙트 적용:** 스킬 적중 시 `EffectPayload` 구조체에 데미지, 버프, 틱(Tick) 도트 뎀 등의 상태 이상 객체를 담아 타겟에게 던지는 방식으로 결합도를 크게 낮췄습니다.

### 3. 🧠 Stateless FSM AI (상태 비저장 유한 상태 머신)

개별 유닛이 무거운 FSM 객체를 생성하지 않도록 설계했습니다.

* `ScriptableObject` 기반의 Action 노드를 활용하여 상태(State) 로직 자체는 메모리에 싱글톤처럼 단 하나만 존재합니다.
* 각 캐릭터는 자신의 상태 데이터만 담긴 `Context`를 SO에 넘겨주어 로직을 실행하므로, 유닛이 수백 마리 스폰되어도 AI 연산 부하와 메모리 사용량이 극히 적습니다.

### 4. 🎯 Optimized Targeting & Line-of-Sight (최적화된 타겟팅 및 길막 시스템)

디펜스 장르의 핵심인 '언덕(고지대)'과 '평지(경로)' 판별 로직을 물리 엔진 필터를 통해 완벽히 분리했습니다.

* **스마트 캐스팅:** `ContactFilter2D`를 적용하여 `isTrigger`로 설정된 공격 사거리 콜라이더를 무시하고, 유닛의 '진짜 몸통'만 정확히 감지합니다.
* **경로 기반 Block 판별:** 근접 몬스터는 이동 경로(Path) 방향으로만 `CircleCast`를 쏘아 평지에 있는 탱커에게만 길막(Block)을 당하며, 언덕 위 원거리 딜러는 안전하게 보호됩니다.

---

## 🛠️ Tech Stack

* **Engine:** Unity 2022.x (C#)
* **Asynchronous Programming:** UniTask
* **Asset Management:** Addressables (에셋 동적 로드 및 메모리 관리)
* **Design Patterns Used:**
* Strategy Pattern (성장 공식 및 스킬 로직)
* Observer Pattern (HP 이벤트 및 사망 콜백)
* Factory / Provider Pattern (스탯 계산)
* Object Pooling (이펙트 및 투사체 최적화)



---

## 📂 Core Directory Structure (주요 코드 구조)

```text
📁 Scripts
 ┣ 📂 Character
 ┃ ┣ 📜 PlayerCharacterController.cs  # 유닛의 스폰, 수명주기, 입력 이벤트를 관리하는 허브
 ┃ ┣ 📜 PlayerCombatController.cs     # 타겟팅, 공격 실행, 이펙트 처리를 담당하는 전투기
 ┃ ┗ 📜 HPController.cs               # 체력 데이터의 단일 진실 공급원(SSOT)
 ┣ 📂 Data
 ┃ ┣ 📜 CharacterData.cs              # 캐릭터 기본 스탯 및 SO 참조 (원본 데이터)
 ┃ ┣ 📜 UserCharacterData.cs          # 로비 유저 데이터 (더티 플래그 캐싱 적용)
 ┃ ┗ 📜 InGameCharacterData.cs        # 인게임 스폰 시 생성되는 전투 전용 데이터 컨테이너
 ┣ 📂 FSM
 ┃ ┣ 📜 CharacterStateManager.cs      # FSM Context 관리 및 상태 전이 제어
 ┃ ┗ 📜 CharacterStateScriptableObjectBase.cs # 상태(Stateless) 로직 베이스
 ┗ 📂 StatSystem
   ┣ 📜 ConditionBuffeManager.cs      # 실시간 버프/디버프 및 스탯 연산 코어
   ┗ 📜 ConditionBuffeBase.cs         # 틱 데미지, 스탯 증감 모디파이어 베이스

```

---

## 💡 Developer's Note (개발자 코멘트)

단순히 기능이 '작동'하는 것을 넘어, "기획자가 프로그래머의 도움 없이 엑셀(CSV)과 SO만으로 새로운 직업과 스킬을 무한히 만들어낼 수 있는 환경"을 구축하는 것에 가장 큰 목표를 두었습니다. 또한 유니티 물리 엔진의 오버헤드를 줄이기 위해 무거운 `Rigidbody` 의존성을 덜어내고 논리적인 거리 계산과 가벼운 캐스팅만으로 전투 시스템을 구현했습니다.

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](https://www.google.com/search?q=LICENSE) file for details.
