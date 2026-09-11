using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// AdMob 리워드(타임업 연장) + 전면(게임오버 전환 시점) 광고. GoogleMobileAds
    /// 타입이 이 클래스 밖으로 새지 않게 감싼다(재사용 노트 6-1) — UI는 이 클래스의
    /// public 메서드만 알면 된다.
    ///
    /// 실제 AdMob 콘솔에서 발급받은 유닛 ID를 쓴다(2026-08-15 등록 완료).
    /// 에디터에서 Play 할 때는 Google Mobile Ads SDK가 실제 기기 SDK를 안 타고
    /// 자체 스텁으로 항상 샘플 광고를 보여주므로, 에디터 안에서는 이 ID로 실제
    /// 광고가 나가거나 계정에 영향이 가지 않는다 — 실제 기기 빌드로 반복 클릭할
    /// 때만 주의(무효 트래픽 위험, AdMob 콘솔에 테스트 기기로 등록해서 방지).
    ///
    /// 전 세계 배포로 정하면서(2026-09-11) SDK 초기화/광고 요청 전에 반드시
    /// <see cref="ConsentManager"/>의 UMP 동의 수집을 먼저 거치도록 바뀌었다.
    /// </summary>
    public static class AdManager
    {
        private const string RewardedAdUnitId = "ca-app-pub-6387288948977074/4816764246";
        private const string InterstitialAdUnitId = "ca-app-pub-6387288948977074/6747320837";

        private static bool _initialized;
        private static RewardedAd _rewardedAd;
        private static InterstitialAd _interstitialAd;

        public static bool IsRewardedReady => _rewardedAd != null && _rewardedAd.CanShowAd();

        /// <summary>
        /// AppRoot 부팅 시 한 번 호출. UMP 동의 수집이 끝난 뒤에만(그리고 동의
        /// 상 광고 요청이 가능할 때만) 실제로 SDK를 초기화하고 광고를 미리
        /// 로드해 둔다 — EEA/영국 사용자에게 동의 전 광고를 요청하면 안 되기
        /// 때문(<see cref="ConsentManager"/> 참고).
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            ConsentManager.GatherConsent(() =>
            {
                if (!ConsentManager.CanRequestAds)
                {
                    Debug.Log("[TenRush] Ads not initialized — consent not granted for ad requests.");
                    return;
                }

                MobileAds.Initialize(_ =>
                {
                    LoadRewardedAd();
                    LoadInterstitialAd();
                });
            });
        }

        /// <summary>
        /// 리워드 광고를 보여준다. 끝까지 봐야만 onRewardEarned가 불리고, 로드가
        /// 안 됐거나/중간에 닫았거나/실패하면 onDeclinedOrUnavailable만 불린다
        /// (기획서: 보상 지급은 반드시 "끝까지 봤다" 콜백에서만).
        /// </summary>
        public static void ShowRewarded(Action onRewardEarned, Action onDeclinedOrUnavailable)
        {
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
            {
                onDeclinedOrUnavailable?.Invoke();
                return;
            }

            var ad = _rewardedAd;
            bool rewardEarned = false;

            void HandleClosed()
            {
                ad.OnAdFullScreenContentClosed -= HandleClosed;
                LoadRewardedAd();

                if (rewardEarned)
                {
                    AdFrequencyStore.RecordFullScreenAdShown();
                    onRewardEarned?.Invoke();
                }
                else
                {
                    onDeclinedOrUnavailable?.Invoke();
                }
            }

            void HandleFailed(AdError error)
            {
                Debug.LogWarning($"[TenRush] Rewarded ad failed to show: {error}");
                ad.OnAdFullScreenContentClosed -= HandleClosed;
                ad.OnAdFullScreenContentFailed -= HandleFailed;
                LoadRewardedAd();
                onDeclinedOrUnavailable?.Invoke();
            }

            ad.OnAdFullScreenContentClosed += HandleClosed;
            ad.OnAdFullScreenContentFailed += HandleFailed;
            ad.Show(_ => rewardEarned = true);
        }

        /// <summary>
        /// 빈도 정책(<see cref="AdFrequencyStore"/>)을 만족하고 로드도 돼 있으면
        /// 전면 광고를 보여준다. 어느 쪽이든 onCompleted는 반드시 불린다 — 광고를
        /// 안 띄웠으면 즉시, 띄웠으면 닫힌 뒤에.
        /// </summary>
        public static void TryShowInterstitial(Action onCompleted)
        {
            bool policyAllows = AdFrequencyStore.ShouldShowInterstitial();
            bool adReady = _interstitialAd != null && _interstitialAd.CanShowAd();

            if (!policyAllows || !adReady)
            {
                // 왜 안 뜨는지 원인을 바로 구분할 수 있게 남겨 둔다(정책 때문인지,
                // 광고 로드가 안 끝나서인지).
                Debug.Log($"[TenRush] Interstitial skipped — policyAllows={policyAllows}, adReady={adReady} (loaded={_interstitialAd != null})");
                onCompleted?.Invoke();
                return;
            }

            var ad = _interstitialAd;

            void HandleClosed()
            {
                ad.OnAdFullScreenContentClosed -= HandleClosed;
                LoadInterstitialAd();
                onCompleted?.Invoke();
            }

            void HandleFailed(AdError error)
            {
                Debug.LogWarning($"[TenRush] Interstitial ad failed to show: {error}");
                ad.OnAdFullScreenContentClosed -= HandleClosed;
                ad.OnAdFullScreenContentFailed -= HandleFailed;
                LoadInterstitialAd();
                onCompleted?.Invoke();
            }

            ad.OnAdFullScreenContentClosed += HandleClosed;
            ad.OnAdFullScreenContentFailed += HandleFailed;
            AdFrequencyStore.RecordFullScreenAdShown();
            ad.Show();
        }

        private static void LoadRewardedAd()
        {
            RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[TenRush] Rewarded ad failed to load: {error}");
                    return;
                }
                _rewardedAd = ad;
            });
        }

        private static void LoadInterstitialAd()
        {
            InterstitialAd.Load(InterstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[TenRush] Interstitial ad failed to load: {error}");
                    return;
                }
                Debug.Log("[TenRush] Interstitial ad loaded and ready.");
                _interstitialAd = ad;
            });
        }
    }
}
