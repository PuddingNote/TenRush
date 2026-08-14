# 확정 사항 (기획서 vs 실제 개발 범위)

> 기획서 원문과 실제로 확정한 개발 범위가 다를 때, 그 차이와 이유를 기록해 둔다.
> (다음 세션이 기획서만 보고 되돌리는 실수를 막기 위한 문서)

## 기준 문서

`텐러쉬_기획서_Claude Code 전달용.md`를 기준으로 개발한다. `텐러쉬_기획서.md`(원본)는
프로토타입 검증 과정의 이전 버전이며, 아래 항목에서 전달용 버전과 다르다.

## 2026-08-14 확정

- **라이트 배틀패스(시즌 보상) 제외.** 원본 기획서 7장에는 있었으나, 시즌/보상 관리를
  위한 별도 상태 관리가 필요해 범위가 커진다. Claude Code 전달용 버전 기준으로
  1차 개발 범위에서 뺀다. 코어 루프가 안정화된 뒤 별도로 재검토.
- **수익화 범위**: 광고(리워드: 시간 연장/셔플, 전면: 게임오버 3~4회당 1번) + IAP(노애드,
  테마팩) — 전달용 버전 7장 그대로.
- **타겟 플랫폼**: Android만. iOS는 고려하지 않음(Player Settings·SDK 세팅도 Android
  기준으로만 진행).
- **패키지 식별자**: `com.onepixel.tenrush` (companyName: `OnePixel`, productName: `Ten Rush`).
  `ProjectSettings/ProjectSettings.asset`에 반영 완료.
- **백엔드 없음**: 기획서에 랭킹/멀티플레이 언급이 없으므로 자체 서버·계정 시스템 없이
  로컬 저장(최고 점수 등)만 사용하는 것을 기본값으로 한다. 필요해지면(예: 리더보드) 그
  시점에 다시 논의.
- **개발 순서**: 코어 매치 로직(그리드/매치/콤보/타이머, 광고 SDK 없이) 먼저 구현하고
  EditMode 테스트로 검증 → 이후 광고/IAP 붙임. 기획서 13장 제안과 동일한 순서.

## 2026-08-14 추가 확정

- **Git push는 사용자가 직접 한다.** Claude Code는 커밋까지만 하고 절대 push하지 않는다.
- **타이틀 화면을 만든다.** 기획서 10장의 "타이틀 없이 바로 플레이 진입" 제안은 채택하지
  않음 — 게임 제목 + 시작 버튼 정도의 최소 타이틀 화면을 둔다.
- **텍스트는 영어만 사용한다.** 이 게임은 텍스트가 거의 없는 게임이라 한국어/영어
  분기 없이 영어 텍스트만 쓴다(개인정보처리방침처럼 별도로 다국어가 필요한 문서는
  예외).
- **폰트는 `ONE Mobile POP SDF`(TextMeshPro SDF) 고정.** 코드에서 `Resources.Load`로
  불러오기 위해 `Assets/Fonts/` → `Assets/Resources/Fonts/`로 이동함(GUID는 유지되므로
  기존 참조는 없음 — 문제 없음). 이 게임의 모든 텍스트는 이 폰트만 쓴다.

## 2026-08-14 UI 1차 구현 관련 메모

- **UI는 프리팹/씬 편집 없이 전부 코드로 구성.** `AppRoot`가
  `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`로 게임 시작 시 자동 실행되어
  Canvas·EventSystem·타이틀 화면까지 스스로 만든다 — **씬 파일(.unity)에는 아무 것도
  추가하지 않았다.** 어떤 씬이 열려 있든(SampleScene 포함) Play를 누르면 바로 뜬다.
- **DOTween 미사용.** 기획서 8장은 DOTween을 제안했지만 이 프로젝트에 아직
  임포트돼 있지 않아서(에셋스토어 임포트가 필요해 코드만으로 처리 불가), 1차
  구현은 `UiTween`(코루틴 기반 Scale/Fade/Shake)으로 대체했다. 호출부
  (`TileButton.PlayClear` 등)는 그대로 두고 내부 구현만 나중에 DOTween으로
  바꿔 끼울 수 있게 분리해 둠. 사용자가 DOTween을 임포트하고 싶다면 알려주면
  교체 가능.
- Input System이 "Input System Package (New)" 전용으로 설정돼 있어서(레거시
  Input Manager 아님), `InputSystemUIInputModule`을 코드로 만들고
  `AssignDefaultActions()`를 호출해 UI 클릭이 동작하게 함.
