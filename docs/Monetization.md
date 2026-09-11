# 수익화 — 광고 (2026-08-15)

> 왜 이렇게 정했는지 위주로 적는다. 무엇을 구현했는지는 코드가 말해준다.

## 결정: 리워드는 "타임업 +15초 연장" 하나만

기획서 7장 원안은 리워드 광고를 2곳(시간 연장, 셔플권)에 넣는 것이었다. 실제
논의에서 셔플권을 뺐다 — 이 게임은 매치 후 유효 페어가 없으면 **자동으로
전체 리셔플**되는 막힘 방지 로직이 이미 있어서, "막혔다"는 느낌 자체가 잘 안
생긴다. 즉 셔플권이 풀어줄 실제 페인포인트가 없다.

**"+15초 하나로 상품성이 있나?"**라는 질문이 나왔고, 검토 결과 아래처럼
정리했다:

- 대안으로 "게임오버 시 2배 점수" 리워드를 제안했으나(노출 빈도가 훨씬
  높아짐 — 매판마다 기회가 생기므로), **사용자가 시간 연장만으로 가기로
  결정**했다. 상품 개수보다 단순함을 우선한 판단.
- 대신 UX를 다듬어서 이 하나의 리워드가 최대한 잘 작동하게 만든다:
  타임업 순간 결과 화면으로 바로 넘어가지 않고, **"광고 보고 +15초?" 오퍼를
  먼저 5초간 보여주고(자동 사양 카운트다운), 리워드 광고가 로드돼 있을 때만
  오퍼 자체를 띄운다.**

## 확정된 광고 구성

| 광고 | 조건 | 판당 제한 |
|---|---|---|
| 리워드(+15초) | 타임업 순간, 광고가 로드돼 있을 때만 오퍼 표시 | 1회 |
| 전면 | 결과 화면에서 RETRY/MAIN MENU로 **벗어나는** 시점 | 게임 3회당 1회, 첫 판 제외, 최소 90초 간격 |

전면과 리워드는 `AdFrequencyStore`의 같은 "마지막 전체화면 광고 시각"을
공유한다 — 리워드 광고를 보고 나가자마자 전면이 또 뜨는 최악의 경험을
막기 위함(재사용 시스템 모음 2장 원칙).

## SDK

**Google Mobile Ads Unity Plugin** (`com.google.ads.mobile`), OpenUPM 스코프드
레지스트리로 설치:
```json
"scopedRegistries": [
  { "name": "OpenUPM", "url": "https://package.openupm.com", "scopes": ["com.google"] }
],
"dependencies": { "com.google.ads.mobile": "11.3.0" }
```
버전은 2026-08-15 기준 최신([GitHub Releases](https://github.com/googleads/googleads-mobile-unity/releases)).

**광고 유닛 ID는 실제 AdMob 계정 ID로 교체 완료**(2026-08-15, `AdManager.cs` 상단
상수):
- App ID: `ca-app-pub-6387288948977074~2472888119` — `Assets → Google Mobile Ads → Settings`
  메뉴에서 직접 입력 필요(Claude Code가 그 설정 에셋을 대신 만들 수 없음)
- 전면: `ca-app-pub-6387288948977074/6747320837`
- 리워드: `ca-app-pub-6387288948977074/4816764246`

Unity 에디터에서 Play 할 때는 SDK가 실제 기기 광고 대신 자체 샘플 광고를
보여주므로, 에디터 테스트 중에는 이 ID로도 실제 계정에 영향이 없다. **실제
안드로이드 기기/에뮬레이터 빌드로 반복 클릭 테스트할 때만 주의** — 무효
트래픽으로 계정이 제재될 수 있으니, 그럴 땐 AdMob 콘솔에 해당 기기를
테스트 기기로 등록해 둘 것.

## UMP(EEA/영국 동의) — 2026-09-11 구현 완료

전 세계 배포로 정하면서 미뤄뒀던 UMP를 붙였다. `ConsentManager`
(`TenRush.Managers`)가 GoogleMobileAds.Ump.Api를 감싼다.

- **순서**: `AdManager.Initialize()`가 예전엔 바로 `MobileAds.Initialize()`를
  불렀는데, 이제 `ConsentManager.GatherConsent(...)`로 동의 정보를 먼저
  조회 → 필요하면(EEA/영국) 동의 폼을 보여줌 → 그 다음에야 `CanRequestAds()`가
  true일 때만 실제 SDK 초기화 + 광고 로드. EEA/영국이 아닌 사용자는 SDK가
  알아서 폼 없이 바로 통과시킨다.
- **Fail-open**: 동의 정보 조회 자체가 실패해도(오프라인 등) 콜백은 항상
  불린다 — 강제 업데이트와 같은 원칙. 그 경우 `CanRequestAds()`가 false면
  그냥 광고를 안 띄울 뿐, 게임 진행에는 영향 없음.
- **Privacy Options 진입점**: Google 정책상 동의 폼을 보여준 사용자에게는
  나중에 선택을 바꿀 수 있는 진입점을 계속 제공해야 함. `SettingsDialog`에
  `ConsentInformation.PrivacyOptionsRequirementStatus == Required`일 때만
  "PRIVACY OPTIONS" 버튼이 나타나게 했다(해당 지역 아니면 버튼 자체가 안 생김).
- **테스트 방법**: `ConsentManager.GatherConsent()`에 `DebugGeography.EEA`
  디버그 설정을 넣어 뒀는데, `#if UNITY_EDITOR || DEVELOPMENT_BUILD`로
  감싸서 **에디터/개발 빌드에서만 컴파일되고 실제 Release 빌드(스토어 제출용)
  에는 이 코드 자체가 통째로 빠진다** — "나중에 지워야 하는 위험한 코드"가
  아니라 구조적으로 안전하다. 실제로 동의창을 보려면: Build Settings에서
  "Development Build" 체크 → 실기기에 설치(에디터는 네이티브 동의창 UI가
  안 뜰 가능성이 높음) → 첫 실행. 그래도 안 뜨면 기기 로그(Logcat)에 찍히는
  "테스트 기기로 등록하라"는 안내의 해시 ID를 `TestDeviceHashedIds`에 추가.
- 개인정보처리방침(`docs/privacy-policy.html`)에도 UMP·Privacy Options
  안내 문구 추가함(2026-09-11).

## 미룬 것

- **배너 광고**: 재사용 시스템 모음 2장 원칙대로 처음부터 제외(그리드+HUD가
  화면을 이미 꽉 채워서 배너 넣으면 오조작 유발).

## ⚠️ 결정: IAP는 아예 안 함 — 기획서와 다르게 확정 (2026-08-15)

기획서 7장 원안에는 IAP 2종(노애드, 테마팩)이 있었지만, **사용자가 IAP 자체를
빼고 광고 수익만으로 가기로 확정**했다. 이유(대화 기준): 결제 심사·세금 설정
등으로 일정이 늘어나는 것보다 "사람이 계속 하는지"를 광고 수익만으로 먼저
확인하는 쪽을 택함(Dice Battle 재사용 노트 2장의 "첫 출시는 광고만, 결제는
나중" 원칙과도 일치).

**다음에 기획서를 다시 참고할 일이 있으면 7장의 IAP 항목(노애드, 테마팩)은
없는 것으로 본다.** 나중에 마음이 바뀌면 그때 다시 논의 — Unity IAP 패키지
설치부터 새로 시작해야 함(지금 프로젝트엔 전혀 없음).
