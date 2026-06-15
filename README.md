# 🛡️ The Imagine Arc Project

> **2D 서브컬처 수집형 RPG 디펜스 게임 프레임워크**  
> Unity 기반의 고성능·확장 가능한 디펜스 RPG 코어 시스템

---

## 📌 프로젝트 개요

**The Imagine Arc**는 Unity(C#)로 개발 중인 2D 수집형 RPG 디펜스 게임입니다.  
기획자가 프로그래머의 도움 없이 **CSV(Google Sheet) + ScriptableObject** 만으로 새로운 직업, 스킬, 맵을 무한히 제작할 수 있는 **데이터 주도(Data-Driven) 아키텍처**를 핵심 목표로 설계되었습니다.

단순히 기능이 '작동'하는 수준을 넘어, **메모리 효율**·**연산 부하 최소화**·**낮은 결합도**를 동시에 달성하는 것에 중점을 두었습니다.

---

## 🚀 핵심 기능 및 아키텍처

### 1. 📊 데이터 주도형 이원화 성장 시스템 (Data-Driven Dual Growth)

로비(아웃게임)의 **영구 성장**과 전투(인게임)의 **임시 강화**를 완벽히 분리하여 데이터 오염을 원천 차단합니다.

| 구분 | 구현 방식 | 특징 |
|---|---|---|
| 로비 성장 (Meta Progression) | `GrowthDataSO` + Dirty Flag 패턴 | 레벨업/랭크업 스탯을 GC 없이 UI에 즉각 반영 |
| 인게임 강화 (In-Game Upgrade) | `InGameUpgradeStrategySO` (전략 패턴) | 전투 중 강화 → 전투 종료 시 객체 파괴와 함께 자동 초기화 |

---

### 2. ⚔️ GAS 기반 전투 스탯 시스템 (Modifier-based Stat System)

기존 데코레이터 패턴의 한계(버프 해제 및 관리 어려움)를 극복하기 위해 **리스트 기반 모디파이어(Modifier) 시스템**을 자체 구축했습니다.

- **`IStatProvider` & `ConditionBuffeManager`**  
  원본 베이스 스탯에 현재 활성화된 버프/디버프(Condition) 리스트를 합산하여 최종 스탯을 동적으로 계산합니다.

- **Payload 기반 이펙트 전달**  
  스킬 적중 시 `EffectPayload` 구조체에 데미지·버프·틱(Tick) 도트 데미지 등을 담아 타겟에게 전달하는 방식으로 결합도를 크게 낮췄습니다.

---

### 3. 🧠 Stateless FSM AI (상태 비저장 유한 상태 머신)

개별 유닛이 무거운 FSM 객체를 인스턴스화하지 않도록 설계했습니다.

- `ScriptableObject` 기반 Action 노드: 상태(State) 로직 자체는 메모리에 **싱글톤처럼 단 하나만** 존재
- 각 캐릭터는 자신의 상태 데이터만 담긴 `Context`를 SO에 넘겨 로직을 실행
- 수백 마리의 유닛이 동시에 스폰되어도 **AI 연산 부하와 메모리 사용량이 극히 낮음**

---

### 4. 🎯 최적화된 타겟팅 및 길막(Block) 시스템

디펜스 장르의 핵심인 **'언덕(고지대)'**과 **'평지(경로)'** 판별 로직을 물리 엔진 필터로 완벽히 분리했습니다.

- **스마트 캐스팅**: `ContactFilter2D`를 적용하여 공격 사거리 트리거 콜라이더를 무시하고 유닛의 '실제 몸통'만 정확히 감지
- **경로 기반 Block 판별**: 근접 몬스터는 이동 경로(Path) 방향으로만 `CircleCast`를 쏘아 평지의 탱커에게만 길막 적용 → 언덕 위 원거리 딜러 안전 보호

---

## 🛠️ 기술 스택

| 항목 | 내용 |
|---|---|
| **엔진** | Unity 6.3 (C#) |
| **비동기 처리** | UniTask |
| **에셋 관리** | Addressables (동적 로드 및 메모리 관리) |
| **데이터 파이프라인** | Google Sheet → CSV 자동 파싱 |
| **디자인 패턴** | Strategy, Observer, Factory/Provider, Object Pooling |

---

## 📂 프로젝트 구조

```
📁 The_Imagine_Arc_Project
 ┣ 📂 Buffe                    # 버프/디버프 Condition 모듈
 ┣ 📂 CharacterController       # 유닛 생명주기, 전투, HP 제어
 ┣ 📂 Data                     # ScriptableObject 및 캐릭터 데이터 정의
 ┣ 📂 Editor                   # Unity 커스텀 에디터 도구
 ┣ 📂 GoogleSheet               # CSV 파싱 및 데이터 자동화 파이프라인
 ┣ 📂 Indicator                 # 체력바, 상태 표시 UI 인디케이터
 ┣ 📂 Manager                  # GameManager 등 씬 전역 관리자
 ┣ 📂 MapEditor                 # 인게임 맵 편집 도구
 ┣ 📂 NetExcute                 # 네트워크 통신 처리
 ┣ 📂 Tile                     # 타일맵 및 경로 시스템
 ┣ 📂 TimeLineController        # 컷씬 및 타임라인 제어
 ┣ 📂 UI                       # 전체 UI 컴포넌트
 ┣ 📂 Util                     # 공용 유틸리티 및 확장 메서드
 ┣ 📜 AutoRelease.cs            # 오브젝트 자동 반환 (Object Pool)
 ┣ 📜 AwakeScene.cs             # 씬 초기화 진입점
 ┣ 📜 Config.cs                 # 전역 상수 및 설정값
 ┣ 📜 DisableTimerEffect.cs     # 이펙트 타이머 기반 비활성화
 ┣ 📜 DownloaderCheck.cs        # 에셋 다운로드 상태 확인
 ┗ 📜 MapData.cs                # 맵 데이터 구조 정의
```

### 핵심 스크립트

```
📁 CharacterController
 ┣ 📜 PlayerCharacterController.cs   # 유닛 스폰, 수명주기, 입력 이벤트 허브
 ┣ 📜 PlayerCombatController.cs      # 타겟팅, 공격 실행, 이펙트 처리 담당
 ┗ 📜 HPController.cs                # 체력 데이터의 단일 진실 공급원 (SSOT)

📁 Data
 ┣ 📜 CharacterData.cs               # 캐릭터 기본 스탯 및 SO 참조 (원본 데이터)
 ┣ 📜 UserCharacterData.cs           # 로비 유저 데이터 (Dirty Flag 캐싱 적용)
 ┗ 📜 InGameCharacterData.cs         # 인게임 스폰 시 생성되는 전투 전용 데이터 컨테이너

📁 Buffe (FSM + 스탯 시스템)
 ┣ 📜 CharacterStateManager.cs       # FSM Context 관리 및 상태 전이 제어
 ┣ 📜 CharacterStateSOBase.cs        # Stateless 상태 로직 베이스 (ScriptableObject)
 ┣ 📜 ConditionBuffeManager.cs       # 실시간 버프/디버프 및 스탯 연산 코어
 ┗ 📜 ConditionBuffeBase.cs          # 틱 데미지, 스탯 증감 모디파이어 베이스
```

---

## 🏗️ 설계 철학

**"기획이 코드를 기다리지 않는 구조"**

- **데이터와 로직의 완전한 분리**: ScriptableObject와 CSV 데이터만으로 신규 콘텐츠 추가 가능
- **물리 엔진 의존성 최소화**: 무거운 `Rigidbody` 대신 논리적 거리 계산과 경량 Physics Cast만 사용
- **메모리 효율 최우선**: Object Pooling, Dirty Flag, Stateless FSM을 통해 GC 압박 최소화
- **낮은 결합도**: Payload 패턴과 이벤트 기반 구조로 모듈 간 독립성 확보

---

## 📝 라이선스

This project is licensed under the **MIT License**.  
See the [LICENSE](./LICENSE) file for details.

---

<div align="center">
  <sub>Built with Unity 6.3 · C# · UniTask · Addressables</sub>
</div>
