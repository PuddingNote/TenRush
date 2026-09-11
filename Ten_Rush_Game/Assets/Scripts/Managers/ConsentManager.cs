using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// UMP(User Messaging Platform) 동의 수집. 전 세계 배포라 EEA/영국 사용자에게는
    /// 광고를 요청하기 전에 반드시 이 절차를 먼저 거쳐야 한다(GDPR 요건).
    /// EEA/영국이 아닌 사용자에게는 SDK가 알아서 폼을 안 띄우고 바로 통과시킨다.
    ///
    /// **실패해도 게임 진행을 막지 않는다(fail-open)** — 동의 정보 조회 자체가
    /// 실패해도(오프라인 등) onReadyToRequestAds는 항상 호출된다. 그 다음
    /// <see cref="CanRequestAds"/>가 false면 AdManager가 광고 요청 자체를
    /// 스킵할 뿐, 게임 진행에는 아무 영향이 없다(강제 업데이트 fail-open 원칙과
    /// 동일 — 재사용 시스템 모음 1장).
    /// </summary>
    public static class ConsentManager
    {
        /// <summary>동의 정보 조회/폼 표시가 끝나면(성공이든 실패든) 항상 호출된다.</summary>
        public static void GatherConsent(Action onReadyToRequestAds)
        {
            var request = new ConsentRequestParameters();

#if DEVELOPMENT_BUILD
            // 테스트 전용 — 이 블록은 "Development Build" 체크 후 만든 실기기
            // 빌드에서만 컴파일된다(에디터는 제외 — 아래 참고). 실제 스토어에
            // 올리는 Release 빌드에는 이 코드 자체가 통째로 빠지므로, 지워야
            // 한다는 걸 기억할 필요 없이 안전하다. 한국은 EEA/영국이 아니라
            // 평소엔 동의창이 절대 안 뜨는데, 이걸로 기기를 EEA인 것처럼 속여서
            // 동의창이 실제로 어떻게 뜨는지 확인할 수 있다.
            request.ConsentDebugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                // 처음 실행하면 기기 로그(Logcat/콘솔)에 "이 기기를 테스트 기기로
                // 쓰려면 이 ID를 추가하라"는 안내가 찍힌다 — 그 ID를 여기 채워
                // 넣으면 이후 실행부터 확실히 EEA로 인식된다(비워 둬도 대부분
                // 동작하지만, 안 뜨면 이 방법을 쓴다).
                TestDeviceHashedIds = new List<string>()
            };
#endif

            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning($"[TenRush] Consent info update failed: {updateError.Message}");
                    onReadyToRequestAds?.Invoke();
                    return;
                }

#if UNITY_EDITOR
                // 유니티 에디터의 UMP는 "플레이스홀더"라서, 폼을 보여주는 척하며
                // Time.timeScale을 0으로 고정해 버린다 — 근데 실제로 뜨는 화면이
                // 없어서 닫을 방법이 없고, 그대로 게임이 영원히 멈춘다(2026-09-11
                // 실제로 겪은 버그). 그래서 에디터에서는 폼 표시 자체를 건너뛴다 —
                // 동의창 미리보기는 실기기 Development Build로 확인한다.
                onReadyToRequestAds?.Invoke();
#else
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null)
                        Debug.LogWarning($"[TenRush] Consent form failed: {formError.Message}");

                    onReadyToRequestAds?.Invoke();
                });
#endif
            });
        }

        /// <summary>GatherConsent()를 먼저 호출하기 전까지는 항상 false.</summary>
        public static bool CanRequestAds => ConsentInformation.CanRequestAds();

        /// <summary>
        /// 설정 화면에 "Privacy Options" 같은 진입점을 계속 보여줘야 하는지.
        /// EEA/영국 사용자에게 동의 폼을 보여준 경우에만 true — 그 외 지역
        /// 사용자에게는 이 버튼 자체를 안 보여주는 게 맞다.
        /// </summary>
        public static bool IsPrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        /// <summary>사용자가 설정 화면에서 동의 선택을 나중에 바꿀 수 있게 하는 진입점.</summary>
        public static void ShowPrivacyOptionsForm(Action onClosed = null)
        {
            ConsentForm.ShowPrivacyOptionsForm(error =>
            {
                if (error != null)
                    Debug.LogWarning($"[TenRush] Privacy options form failed: {error.Message}");
                onClosed?.Invoke();
            });
        }
    }
}
