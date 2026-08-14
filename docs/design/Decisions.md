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

## 2026-08-14 UI 2차 수정 (사용자 실측 피드백 반영)

- **9:16 고정 + 레터박스**: `UiFactory.CreateLetterboxedContentRoot()` 추가 — Canvas
  밑에 전체를 덮는 검은 배경 + `AspectRatioFitter`(FitInParent, 1080:1920)로 크기가
  고정된 `ContentRoot`를 만들고, 모든 화면은 이 밑에 짓는다. 화면비가 안 맞는 기기는
  위/아래(또는 좌/우)가 자동으로 검은 여백 처리됨.
- **타이틀 화면 좌표/폰트 크기는 사용자가 직접 실측한 값 그대로 반영**: Title
  posY -600 / 폰트 140, Hint posY -1100 / 폰트 50, StartButton posY -1200 / 400x120 /
  라벨 폰트 60 (전부 anchor top-center 기준).
  전체적으로 UI 요소 폰트 크기를 한 단계씩 키움(`UiTheme` 참고: HUD 값 72, HUD 라벨
  32, 타일 52, 콤보 40, 버튼 60, 힌트 50). 타일 크기도 138→152로 키움.
- **타이머 바가 시각적으로 안 줄어들던 버그의 원인**: `Image.Type.Filled`를
  스프라이트 없는 Image에 썼더니 fillAmount가 시각적으로 반영되지 않았음(색상
  변경은 정상 동작해서 SetTime() 호출 자체는 문제 없었음 — fillAmount 메커니즘만
  안 먹힘). **해결**: Image.fillAmount 대신 실제 폭(RectTransform.sizeDelta.x)을
  매 프레임 직접 계산해서 넣는 방식으로 교체(`HudView.SetTime`). 더 안정적이라
  앞으로도 진행바류는 이 방식을 기본으로 쓸 것.
- **HUD 여백**: 스코어/타이머가 화면 모서리에 너무 붙어 있던 문제 → `UiTheme.HudMargin`
  (56px) 도입 + 타이머 바는 여기에 60px을 더해 트랙 폭을 화면 폭보다 확실히 좁게.
- **게임오버 화면에 메인 메뉴 버튼 추가**: `GameOverOverlay`가 RETRY 외에
  MAIN MENU 버튼도 갖도록 시그니처 변경(`Create(parent, onRetry, onMainMenu)`).
- **안드로이드 뒤로가기 대응**: 새 Input System에서는 안드로이드 뒤로가기가
  `Keyboard.current.escapeKey`로 들어온다. 공용 `ConfirmDialog`(메시지 + BACK +
  확인 버튼, 뒤로가기 재입력 시 BACK과 동일 취급)를 만들어 타이틀 화면(뒤로가기 →
  "Exit the game?" → EXIT 시 `Application.Quit()`/에디터에서는 Play 종료)과 게임
  화면(뒤로가기 → "Return to title?" → TITLE 시 타이틀로) 양쪽에 재사용.
  `AppRoot`가 `ShowTitle`/`ShowGameplay`를 서로 콜백으로 연결해 화면 전환을 관리.

## 2026-08-14 UI 3차 수정 + 코어 규칙 변경 (기획서와 달라짐)

- **다이얼로그 버튼을 세로 2단 → 가로 1줄로 변경.** ConfirmDialog(BACK 왼쪽/확인
  오른쪽), GameOverOverlay(MAIN MENU 왼쪽/RETRY 오른쪽) 전부 `UiTheme.DialogButtonWidth
  /Height/RowOffsetX` 공용 크기로 나란히 배치.
- **⚠️ 오답(Mismatch) 시 선택 처리 — 기획서·프로토타입과 다르게 확정.**
  - 기획서 2장/12장 원문 및 HTML 프로토타입: "오답 타일이 바로 다음 선택으로
    이어짐(재탭 없이 바로 다음 시도 가능)" — 두 번째로 탭한 타일이 새 선택으로
    남는 체이닝 동작이었고, 처음 구현도 그대로 포팅했음.
  - **실제로 플레이해보니 의도와 다르게 동작함**: 9를 탭하고 2를 탭해 오답이 나면
    2가 선택된 채로 남아서, 사용자가 바로 다음 두 타일을 자유롭게 고르려 할 때
    방해가 됨. 사용자 피드백에 따라 **오답 시 선택을 완전히 해제**하는 것으로
    변경(`GameRound.Tap()`, `TapResult.CreateMismatched`, EditMode 테스트
    `TwoCellsNotSummingToTen_ClearsSelectionEntirely`까지 함께 수정).
  - 다음에 기획서를 다시 참고할 일이 있으면 **이 항목은 기획서 원문이 아니라 여기
    적힌 대로(체이닝 없음)가 맞다.**

## 2026-08-14 최고 점수 저장 + 사운드 (코어 루프 완성도)

- **최고 점수**: `TenRush.Managers.HighScoreStore`가 `PlayerPrefs`로 로컬에만 저장.
  자체 서버 없음 원칙 그대로 유지(개인정보처리방침 가이드 5장과 일치 — 서버가
  없으면 방침이 짧아짐). 게임오버 오버레이에 SCORE 밑에 BEST도 같이 표시.
- **사운드는 오디오 에셋 없이 코드로 합성.** `TenRush.Managers.AudioManager`가
  `AudioClip.Create`로 짧은 "딩" 톤(사인파 + 배음, 지수 감쇠 엔벨로프)을 직접
  만들어 쓰고, 콤보 단계에 따라 `AudioSource.pitch`만 올려서 "콤보 단계별
  피치업"(기획서 8장)을 구현했다. **실제 SFX 에셋을 나중에 붙이고 싶으면
  `AudioManager.GenerateMatchClip()`만 다른 에셋 로드로 바꾸면 되고, 호출부
  (`BoardView`의 `AudioManager.PlayMatch(comboCount)`)는 그대로 둬도 된다.**
  씬에 카메라/AudioListener가 없어도 소리가 나도록 `AudioManager`가 필요하면
  리스너를 직접 만들어 둔다(어떤 씬에 붙여도 동작하게 하려는 `AppRoot`의
  설계 원칙과 동일).

## 2026-08-14 스프라이트 적용 + Settings 화면 + 선택 테두리 방식 변경

- **스프라이트도 AudioLibrary와 동일한 패턴(ScriptableObject + Resources.Load)으로
  인스펙터 노출.** `UiSpriteLibrary`(`TenRush.UI`)에 Tile/SelectionBorder/Button/
  Dialog/Background 슬롯. `Assets/Sprites/`의 흰색 PNG 3종
  (`white_square_128`, `white_square_rounded_128`, `line_box_white_square_128`)은
  스프라이트 모드가 Multiple이라(9-slice border 40px 이미 세팅돼 있음)
  `Resources.Load<Sprite>()`로 직접 못 읽어서(서브 스프라이트 이름 문제)
  ScriptableObject 필드에 직접 끌어다 놓는 방식을 그대로 씀. **Claude Code가
  이 에셋 자체를 만들 수는 없음(에디터 자동화 불가) — 사용자가
  `Create → TenRush → UI Sprite Library`로 만들고 `Assets/Resources/UI/UiSpriteLibrary.asset`
  경로에 저장 후 5개 슬롯에 지정된 파일을 끌어다 놓아야 함.** 에셋이 없거나
  슬롯이 비어 있으면 `UiFactory`가 스프라이트 없이 기존 단색으로 그림(안 깨짐).
  `UiFactory.CreatePanel`이 스프라이트를 받으면 `Image.Type.Sliced`로 그려서
  버튼마다 크기가 달라도 모서리가 안 뭉개지게 함.
- **타일 선택 표시 방식 변경**: 타일을 확대하는 방식(`TileCornerBoost`, Unity
  기본 `Outline` 컴포넌트) → 타일보다 `UiTheme.SelectionBorderPadding`(18px)만큼
  큰 테두리 스프라이트(`line_box_white_square_128`)를 덧씌워 켜고 끄는 방식으로
  교체. 색은 `UiTheme.Select`(기존 선택/콤보 색과 동일 톤)를 그대로 재사용해서
  타일 색상(HSL 무지개 전체)과 안 섞이면서 일관성 유지.
- **메인 화면에 SETTINGS 버튼 추가**(START 왼쪽, 한 줄 배치). "SETTINGS" 라벨이
  길어서 START도 같이 `DialogButtonFontSize`(55)로 맞춤(안 그러면 넘칠 위험).
- **SettingsDialog 추가**: BGM/SFX 각각 라벨 + On/Off 토글 버튼 + 볼륨 슬라이더,
  하단에 CLOSE 버튼만(참고 이미지에 있던 "게임 설명서"/"크레딧" 버튼은 요청에
  없어서 제외). `UiFactory.CreateSlider`를 새로 추가(Unity 기본 Slider 프리팹과
  동일한 Background/FillArea+Fill/HandleSlideArea+Handle 구조를 코드로 구성).
- **오디오 설정 저장**: `AudioSettingsStore`(PlayerPrefs)로 BGM/SFX On·Off와
  볼륨을 저장. `AudioManager.ApplyBgmVolume()`을 슬라이더/토글 변경 시마다
  호출해 재생 중인 BGM에도 즉시 반영. `AudioLibrary`의 볼륨 필드(디자인
  기준 믹스 레벨)와 곱해서 최종 볼륨을 낸다.
- **실측 조정**: 선택 테두리 여백 18px → 5px. 슬라이더 Handle 이미지를
  `UiSpriteLibrary`에 전용 슬롯(`SliderHandleSprite`)으로 분리(버튼 스프라이트
  재사용 안 함, 나중에 별도 이미지로 추가 예정).
  **Handle Top/Bottom 인셋의 진짜 원인**: `Slider.UpdateVisuals()`가 세로
  앵커를 항상 (0,1) 풀스트레치로 강제 덮어쓴다(가로만 값에 따라 점으로 바꿈) —
  그래서 세로를 점(point)으로 미리 고정해도 Slider가 즉시 되돌려버린다. 세로가
  스트레치 상태에서는 `sizeDelta.y`가 "부모보다 얼마나 더/덜 튀어나오는가"가
  되므로, `UiFactory.CreateSlider`의 `handleVerticalOverhang`(현재 5px)를
  2배 해서 sizeDelta.y로 넣는 방식으로 Top/Bottom을 원하는 값에 정확히 맞춘다.

## 2026-08-14 버튼 선택음(SFX) 칸 추가

- `AudioLibrary`에 `ButtonClickSfx` 슬롯 추가(매치음과 별도). `UiFactory.CreateButton`
  으로 만든 모든 버튼(타일 제외 — TileButton은 CreateButton을 안 씀)에 자동으로
  물려서, 각 화면에서 따로 챙기지 않아도 클릭할 때마다 재생된다. 클립을 아직
  안 넣었으면 매치음과 마찬가지로 합성 대체음(더 짧고 딱딱한 클릭 느낌)으로
  자동 대체. 매치음 합성 코드를 `GenerateTone(...)` 공용 함수로 뽑아서 버튼
  클릭음과 파라미터만 다르게 재사용.

## 2026-08-14 오디오 인스펙터 노출 (BGM/SFX 나중에 직접 추가 예정)

- **UI 전체가 코드로만 지어져서 클립을 끌어다 놓을 씬 오브젝트가 없다.** 그래서
  `AudioLibrary`(ScriptableObject, `TenRush.Managers`)를 만들어 인스펙터에서
  직접 클립을 넣을 수 있게 함. 사용법: 프로젝트 창에서 우클릭 →
  `Create → TenRush → Audio Library`, 반드시 `Assets/Resources/Audio/AudioLibrary.asset`
  경로에 저장(`AudioManager`가 `Resources.Load`로 이 경로를 찾음).
- `AudioManager`가 이 에셋을 `Resources.Load`로 읽어 BGM(루프 재생, `AppRoot`
  부팅 시 1회 시작)과 매치 SFX(콤보에 따라 피치업)를 재생한다. **에셋이 아직
  없거나 클립이 비어 있어도 게임은 정상 동작** — BGM은 조용히 스킵, 매치 SFX는
  이전에 만든 합성 대체음(`GenerateFallbackMatchClip`)으로 자동 대체된다.
  나중에 실제 클립을 넣으면 코드 수정 없이 그 즉시 반영됨.
