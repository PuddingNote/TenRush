using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// 전면 광고 노출 빈도 정책(재사용 시스템 모음 2장 그대로): 첫 판은 절대 노출
    /// 안 함, 게임오버 N회당 1번, 최소 간격 확보. 리워드 광고를 봤을 때도 같은
    /// "마지막 전체화면 광고 시각"을 공유해서, 리워드 보고 나가자마자 전면이 또
    /// 뜨는 최악의 경험을 막는다.
    /// </summary>
    public static class AdFrequencyStore
    {
        public const int RoundsPerInterstitial = 3;
        public const float MinSecondsBetweenFullScreenAds = 90f;

        private const string TotalRoundsKey = "TenRush.Ads.TotalRoundsPlayed";
        private const string RoundsSinceInterstitialKey = "TenRush.Ads.RoundsSinceInterstitial";
        private const string LastFullScreenAdRealtimeKey = "TenRush.Ads.LastFullScreenAdRealtime";

        // 리소스가 시작된 이후 흐른 초(Time.realtimeSinceStartup) 기준으로 최소 간격을
        // 재는 세션 로컬 타이머. 기기 시계 조작 방어는 필요 없는 범위(광고 빈도일 뿐,
        // 재화/보상이 아님)라 PlayerPrefs에 그대로 저장해도 충분하다.
        public static void RecordRoundPlayed()
        {
            PlayerPrefs.SetInt(TotalRoundsKey, PlayerPrefs.GetInt(TotalRoundsKey, 0) + 1);
            PlayerPrefs.SetInt(RoundsSinceInterstitialKey, PlayerPrefs.GetInt(RoundsSinceInterstitialKey, 0) + 1);
            PlayerPrefs.Save();
        }

        public static bool ShouldShowInterstitial()
        {
            if (PlayerPrefs.GetInt(TotalRoundsKey, 0) <= 1)
                return false; // 첫 판은 절대 노출 안 함

            if (PlayerPrefs.GetInt(RoundsSinceInterstitialKey, 0) < RoundsPerInterstitial)
                return false;

            float lastShown = PlayerPrefs.GetFloat(LastFullScreenAdRealtimeKey, float.NegativeInfinity);
            if (Time.realtimeSinceStartup - lastShown < MinSecondsBetweenFullScreenAds)
                return false;

            return true;
        }

        /// <summary>전면이든 리워드든 "전체 화면 광고"를 실제로 보여줬을 때 호출.</summary>
        public static void RecordFullScreenAdShown()
        {
            PlayerPrefs.SetInt(RoundsSinceInterstitialKey, 0);
            PlayerPrefs.SetFloat(LastFullScreenAdRealtimeKey, Time.realtimeSinceStartup);
            PlayerPrefs.Save();
        }
    }
}
