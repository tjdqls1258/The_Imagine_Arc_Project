# 🛡️ The Imagine Arc Project

<div align="center">

**2D 서브컬처 수집형 RPG 디펜스 게임**

Unity 6.3 · C# · VContainer · UniTask · UniRx · Addressables · DOTween

[![Unity](https://img.shields.io/badge/Engine-Unity%206.3-black?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://docs.microsoft.com/ko-kr/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![Status](https://img.shields.io/badge/Status-진행%20중-orange)](https://github.com/tjdqls1258/The_Imagine_Arc_Project)

[📺 시연 영상](https://youtu.be/DkD73mF1m1o) · [📄 Notion 포트폴리오](https://www.notion.so/Unity-1613587ebe7e80afbf91da50f70b74e3)

</div>

---

## 📌 프로젝트 개요

**The Imagine Arc**는 Unity 6.3(C#)으로 1인 개발 중인 2D 수집형 RPG 디펜스 게임입니다.

> **핵심 목표:** 기획자가 프로그래머의 도움 없이 **CSV(Google Sheet) + ScriptableObject** 만으로 새로운 직업, 스킬, 맵을 무한히 추가할 수 있는 **데이터 주도(Data-Driven) 아키텍처** 구현

단순히 기능이 '작동'하는 수준을 넘어, **메모리 효율 · 연산 부하 최소화 · 낮은 결합도**를 동시에 달성하는 설계에 중점을 두었습니다.

| 개발 규모 | 엔진 | 핵심 모듈 | 주요 시스템 | 상태 |
|:-:|:-:|:-:|:-:|:-:|
| 1인 전담 | Unity 6.3 | 6개 | 8개 | 진행 중 |

---

## 🚀 핵심 기능 및 아키텍처

### 1. 🏗️ VContainer 레이어드 아키텍처

Unreal GAS의 설계 철학을 Unity에 이식한 DI(의존성 주입) 기반 계층형 아키텍처입니다.

```
Presentation Layer   │  UI / View / Screen / MVP Presenter
Application Layer    │  UseCase / ViewModel / Command
Domain Layer         │  Entity / Repository Interface / Service
Infrastructure Layer │  Repository Impl / Addressables / API
```

**VContainer LifetimeScope 계층 분리**

| Scope | 역할 |
|---|---|
| `GameScope` | 게임 전역 싱글톤 서비스 등록 |
| `BattleScope` | 전투 전용 일시적 인스턴스 등록 |
| `UIScope` | UI 전용 수명주기 분리 |

생성자 주입(`[Inject]` 어트리뷰트) + Interface 기반 설계로 테스트 가능하고 모듈 교체 비용이 최소화된 구조를 확보했습니다.

---

### 2. 📊 데이터 주도형 이원화 성장 시스템

로비(아웃게임)의 **영구 성장**과 전투(인게임)의 **임시 강화**를 완벽히 분리하여 데이터 오염을 원천 차단합니다.

| 구분 | 구현 방식 | 특징 |
|---|---|---|
| **로비 성장 (Meta Progression)** | `GrowthDataSO` + Dirty Flag 패턴 | 레벨업/랭크업 스탯을 GC 없이 UI에 즉각 반영 |
| **인게임 강화 (In-Game Upgrade)** | `InGameUpgradeStrategySO` (전략 패턴) | 전투 종료 시 객체 파괴와 함께 자동 초기화 |

---

### 3. ⚔️ GAS 기반 전투 스탯 시스템

Unreal Engine의 GAS(Gameplay Ability System) 설계 철학을 Unity에 자체 이식하였습니다.  
기존 데코레이터 패턴의 한계(버프 해제 및 관리 어려움)를 극복하기 위해 **리스트 기반 모디파이어(Modifier) 시스템**을 구축했습니다.

```
IStatProvider                     ConditionBuffeManager                EffectPayload
──────────────────                ─────────────────────────────        ──────────────────────
· 원본 베이스 스탯 보유           · 버프/디버프 추가·제거·갱신 관리   · 스킬 적중 시 구조체로 전달
· 활성 버프/디버프 리스트 합산    · 스택 레벨 & 지속시간 Refresh      · 데미지·버프·틱 도트 캡슐화
· 최종 스탯 동적 계산             · 틱 간격 OnTick 자동 호출          · 타겟에게 전달만 — 스킬 코드 수정 無
· Flat / Percent 수정자 분리      · 버프 만료 시 자동 정리 (GC 절감)  · 다중 이펙트 모듈 리스트 지원
· 복수 버프 중첩 스택 지원        · ITargetable 인터페이스로 결합도↓  · Payload 기반 결합도 최소화
```

---

### 4. 🎯 스킬 모듈 설계 (SkillBase SO 3단 분리)

`SkillBase ScriptableObject`를 **메타 데이터 / 타겟팅 / 이펙트** 세 레이어로 분리하여, 타겟팅 모듈 교체만으로 스킬 동작을 변경할 수 있는 전략 패턴 유사 구조를 구현했습니다.

```
SkillBase (ScriptableObject)
 ├── Base Information   : Skill ID · Name · Desc · Icon · Cooldown · Proc Chance · Type
 ├── Targeting System   : Max Cast Range / Range · Shape Indicator · Target Layer · Search Radius
 └── Effect Modules     : Vfx Prefab · Damage Multiply · [+] 리스트 확장 구조 (다중 이펙트 스택)
```

- SO 기반이므로 코드 없이 인스펙터에서 스킬 편집 가능
- `ConditionBuffeSO` 파생 클래스 생성만으로 독·화상·기절 등 신규 상태이상 무한 확장

---

### 5. 🧠 Stateless FSM AI (상태 비저장 유한 상태 머신)

개별 유닛이 무거운 FSM 객체를 인스턴스화하지 않도록 설계했습니다.

- `ScriptableObject` 기반 Action 노드: 상태(State) 로직 자체는 메모리에 **싱글톤처럼 단 하나만** 존재
- 각 캐릭터는 자신의 상태 데이터만 담긴 `Context`를 SO에 넘겨 로직을 실행
- 수백 마리의 유닛이 동시에 스폰되어도 **AI 연산 부하와 메모리 사용량이 극히 낮음**

---

### 6. 🎯 최적화된 타겟팅 및 길막(Block) 시스템

디펜스 장르의 핵심인 **'언덕(고지대)'** 과 **'평지(경로)'** 판별 로직을 물리 엔진 필터로 완벽히 분리했습니다.

- **스마트 캐스팅:** `ContactFilter2D`를 적용하여 공격 사거리 `isTrigger` 콜라이더를 무시하고 유닛의 '실제 몸통'만 정확히 감지
- **경로 기반 Block 판별:** 근접 몬스터는 이동 경로(Path) 방향으로만 `CircleCast`를 쏘아 평지 탱커에게만 길막 적용 → 언덕 위 원거리 딜러 보호

---

### 7. 🎬 스킬 연출 파이프라인 (Queue 기반 순차 재생)

```
SkillFiredEvent → Queue 적재 → TimeScale 0 동결 → Addressables 로드 → PlayableDirector 동적 바인딩 후 재생
```

| 해결한 문제 | 해결 방법 |
|---|---|
| 연속 스킬 발동 시 연출 중첩 | Queue에 적재 → 순차 소비 |
| TimeScale 복원 누락 | `finally` 블록으로 항상 Release 보장 |
| 스킬마다 에셋 중복 로드 | `Addressables LoadAndCache`로 1회 로드 후 재사용 |
| 캐릭터별 타임라인 중복 제작 | 스프라이트만 교체, 타임라인 에셋 1개 공유 |

---

### 8. ⏱️ 우선순위 기반 TimeScale 관리

스킬 슬로우·컷씬·옵션 등 여러 시스템이 `Time.timeScale`을 동시에 조작할 때 발생하는 충돌을 방지합니다.

```
PRIORITY_TIME (열거형)
  SetBaseTime  = 0   ─ 기본 게임 속도 (항상 존재)
  SkillDrag    = 1   ─ 스킬 조준 슬로우모션
  SkillCutScene= 2   ─ 스킬 컷씬 (완전 정지)
  Option       = 3   ─ 옵션창 (최우선 정지)
```

- 요청을 리스트로 관리 → `OrderByDescending(Priority).First()`로 최우선 요청만 실제 적용
- `UniRx MessageBroker` 이벤트 버스로 발신자가 Manager를 직접 참조하지 않아도 되는 구조

---

## 🛠️ 기술 스택

| 항목 | 내용 |
|---|---|
| **엔진** | Unity 6.3 (C#) |
| **DI 프레임워크** | VContainer |
| **비동기 처리** | UniTask + CancellationToken |
| **반응형 프로그래밍** | UniRx (MessageBroker 이벤트 버스) |
| **에셋 관리** | Addressables (동적 로드 및 메모리 관리) |
| **연출** | DOTween · Unity PlayableDirector(Timeline) |
| **데이터 파이프라인** | Google Sheet → CSV 자동 파싱 |
| **디자인 패턴** | Strategy · Observer · Factory/Provider · Object Pooling · Dirty Flag |

---

## 📂 프로젝트 구조

```
📁 The_Imagine_Arc_Project
 ┣ 📂 Buffe                    # 버프/디버프 Condition 모듈 (FSM + 스탯 시스템)
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

📁 UI
 ┗ 📜 SkillCutscenePresenter.cs      # 스킬 컷씬 Queue 기반 순차 재생 처리
```

---

## 🐛 트러블슈팅

### 메모리 누수 — Addressables 미해제
- **문제:** 씬 전환 후 캐릭터 스프라이트가 메모리에 잔류 → GC 스파이크 발생
- **해결:** 씬 Unload 이벤트에서 `AssetLoader.ReleaseAll()` 호출, UniTask CancellationToken으로 로딩 중 해제도 안전하게 처리
- **결과:** 메모리 사용량 **약 44% 절감**

### UniRx Subscribe 누적 — DisposeBag 미정리
- **문제:** UI 창 반복 열고 닫을 때 Subscribe 중복 등록 → 이벤트 N배 발화
- **해결:** `CompositeDisposable`을 `OnDestroy`에서 Dispose, `AddTo(this)` 패턴으로 수명 자동 연동
- **결과:** 이벤트 중복 발화 완전 해소

### Texture2D GPU 데이터 전달 — Shader Uniform 한계
- **문제:** Shader Uniform 배열 크기 고정 → 런타임 유동 파동 데이터 전달 불가
- **해결:** `Texture2D`를 데이터 버퍼로 활용, 파동 데이터 Vector4 → RGBA 채널 인코딩 후 GPU 전달
- **결과:** 수백 개의 동적 파동 실시간 렌더링 성공

---

## 🏗️ 설계 철학

> **"기획이 코드를 기다리지 않는 구조"**

| 원칙 | 구현 |
|---|---|
| **데이터와 로직의 완전한 분리** | ScriptableObject + CSV만으로 신규 콘텐츠 추가 가능 |
| **물리 엔진 의존성 최소화** | Rigidbody 대신 논리적 거리 계산과 경량 Physics Cast 사용 |
| **메모리 효율 최우선** | Object Pooling · Dirty Flag · Stateless FSM으로 GC 압박 최소화 |
| **낮은 결합도** | Payload 패턴 + 이벤트 버스(UniRx)로 모듈 간 독립성 확보 |
| **확장성 우선** | ConditionBuffeSO 파생만으로 신규 상태이상 추가, 전투 코드 수정 無 |

---

## 🗺️ 향후 로드맵

| 단계 | 계획 |
|---|---|
| 단기 | 스킬 파티클 이펙트 완성 · DOTween 스킬 연출 고도화 |
| 중기 | 서버 연동 (REST API + UniTask) · PVP 매칭 시스템 설계 |
| 장기 | Cinemachine 전투 카메라 연출 · 멀티언어 글로벌 빌드 대응 |

---

## 📝 라이선스

This project is licensed under the **MIT License** — see the [LICENSE](./LICENSE) file for details.

---

<div align="center">

Built with ❤️ using **Unity 6.3 · VContainer · UniTask · UniRx · Addressables · DOTween**

</div>
