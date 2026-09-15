using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// 진동 On/Off 설정을 로컬에 저장. AudioSettingsStore와 동일한 패턴(PlayerPrefs만
    /// 사용, 자체 서버 없음). 볼륨 개념이 없어서 On/Off 하나뿐이다.
    /// </summary>
    public static class HapticSettingsStore
    {
        private const string EnabledKey = "TenRush.Haptic.Enabled";

        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
