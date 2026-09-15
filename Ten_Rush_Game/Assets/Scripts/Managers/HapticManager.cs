using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// 진동(햅틱) 재생. 아무 때나 울리면 거슬리므로 딱 두 군데에서만 부른다 —
    /// 미스매치(합이 10이 아닐 때)와 라운드 종료. <see cref="HapticSettingsStore"/>가
    /// 꺼져 있으면 조용히 아무 것도 안 한다.
    ///
    /// Unity의 Handheld.Vibrate()는 길이·패턴을 커스텀할 수 없는 OS 기본 진동
    /// 한 번뿐이다(안드로이드 VIBRATE 권한 필요 — Assets/Plugins/Android/AndroidManifest.xml
    /// 참고). 나중에 이벤트별로 다른 느낌을 주고 싶다면 네이티브 Android 플러그인이
    /// 필요하다.
    /// </summary>
    public static class HapticManager
    {
        /// <summary>숫자 타일 2개를 선택했는데 합이 10이 아닐 때.</summary>
        public static void PlayMismatch() => Vibrate();

        /// <summary>라운드가 종료될 때.</summary>
        public static void PlayRoundEnd() => Vibrate();

        private static void Vibrate()
        {
            if (!HapticSettingsStore.Enabled)
                return;

            Handheld.Vibrate();
        }
    }
}
