using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// 설정 화면(BGM/SFX On·Off + 볼륨)을 로컬에 저장. HighScoreStore와 동일하게
    /// PlayerPrefs만 쓴다(자체 서버 없음 원칙).
    /// </summary>
    public static class AudioSettingsStore
    {
        private const string BgmEnabledKey = "TenRush.Audio.BgmEnabled";
        private const string BgmVolumeKey = "TenRush.Audio.BgmVolume";
        private const string SfxEnabledKey = "TenRush.Audio.SfxEnabled";
        private const string SfxVolumeKey = "TenRush.Audio.SfxVolume";

        public static bool BgmEnabled
        {
            get => PlayerPrefs.GetInt(BgmEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(BgmEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static float BgmVolume
        {
            get => PlayerPrefs.GetFloat(BgmVolumeKey, 0.25f);
            set
            {
                PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        public static bool SfxEnabled
        {
            get => PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SfxEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, 0.45f);
            set
            {
                PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }
    }
}
