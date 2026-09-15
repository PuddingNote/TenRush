using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// BGM/SFX 재생. 실제 클립은 <see cref="AudioLibrary"/> 에셋(인스펙터에서 끌어다
    /// 놓는 방식)에서 가져오고, On/Off·볼륨은 <see cref="AudioSettingsStore"/>(설정
    /// 화면에서 조절, 로컬 저장)를 따른다. **클립을 안 넣어 둔 슬롯은 그냥 무음이다**
    /// (2026-09-11 변경 — 이전엔 합성 대체음을 냈는데, 실제로는 아무 소리도
    /// 안 나길 원한다는 피드백에 따라 뺐다).
    /// </summary>
    public static class AudioManager
    {
        private const string LibraryResourcePath = "Audio/AudioLibrary";

        private const float BasePitch = 1f;
        private const float PitchStepPerCombo = 0.06f;
        private const float MaxPitch = 1.8f;

        private static bool _initialized;
        private static AudioLibrary _library;
        private static AudioSource _sfxSource;
        private static AudioSource _bgmSource;

        /// <summary>AppRoot 부팅 시 한 번 호출. 라이브러리에 BGM 클립이 있을 때만 재생 시작(없으면 조용히 스킵).</summary>
        public static void PlayBgmIfConfigured()
        {
            EnsureInitialized();

            var clip = _library != null ? _library.BgmClip : null;
            if (clip == null)
                return;

            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            ApplyBgmVolume();
            _bgmSource.Play();
        }

        /// <summary>설정 화면에서 BGM On/Off·볼륨을 바꿀 때마다 호출해서 재생 중인 소리에 즉시 반영한다.</summary>
        public static void ApplyBgmVolume()
        {
            EnsureInitialized();

            float baseVolume = _library != null ? _library.BgmVolume : 0.6f;
            _bgmSource.volume = AudioSettingsStore.BgmEnabled ? baseVolume * AudioSettingsStore.BgmVolume : 0f;
        }

        /// <summary>숫자 타일을 선택/해제할 때(아직 매치 결과가 나오기 전) — 기본 터치 피드백음.</summary>
        public static void PlayTileSelect()
        {
            PlaySfx(_library != null ? _library.TileSelectSfx : null, BasePitch);
        }

        /// <summary>합이 10이 맞아서 두 타일이 매치됐을 때.</summary>
        /// <remarks>
        /// 콤보가 이어질수록 피치를 올리는 기능이 있었는데(2026-09-15 임시 비활성화)
        /// — 실제 사운드로 들어보니 콤보 피치업 없이 들어보고 싶다는 요청으로 꺼둠.
        /// 되살리려면 아래 주석 처리된 줄로 pitch를 계산하면 됨.
        /// </remarks>
        public static void PlayMatch(int comboCount)
        {
            // float pitch = Mathf.Min(MaxPitch, BasePitch + (comboCount - 1) * PitchStepPerCombo);
            PlaySfx(_library != null ? _library.MatchSuccessSfx : null, BasePitch);
        }

        /// <summary>합이 10이 아니라서 매치가 실패했을 때.</summary>
        public static void PlayMismatch()
        {
            PlaySfx(_library != null ? _library.MatchFailSfx : null, BasePitch);
        }

        /// <summary>버튼(타일 제외 — UiFactory.CreateButton으로 만든 모든 버튼)을 누를 때마다 자동 재생.</summary>
        public static void PlayButtonClick()
        {
            PlaySfx(_library != null ? _library.ButtonClickSfx : null, BasePitch);
        }

        /// <summary>라운드가 최종 종료되어(연장 없이) 결과 화면으로 넘어가는 순간.</summary>
        public static void PlayRoundEnd()
        {
            PlaySfx(_library != null ? _library.RoundEndSfx : null, BasePitch);
        }

        private static void PlaySfx(AudioClip clip, float pitch)
        {
            EnsureInitialized();

            if (!AudioSettingsStore.SfxEnabled)
                return;

            if (clip == null)
                return; // 클립을 안 넣어 뒀으면 그냥 무음.

            float baseVolume = _library != null ? _library.SfxVolume : 1f;
            float volume = baseVolume * AudioSettingsStore.SfxVolume;
            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, volume);
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;
            _initialized = true;

            EnsureAudioListenerExists();

            var go = new GameObject("TenRush.AudioManager");
            Object.DontDestroyOnLoad(go);

            _sfxSource = go.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;

            _bgmSource = go.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.spatialBlend = 0f;

            _library = Resources.Load<AudioLibrary>(LibraryResourcePath);
            // _library가 null이어도(AudioLibrary.asset을 아직 안 만들었어도) 정상 —
            // 위 각 Play 메서드가 클립 없음을 감지해서 조용히 아무 것도 안 한다.
        }

        /// <summary>이 스크립트를 어떤 씬에 붙여도(카메라/리스너가 없어도) 소리가 들리게 보장한다.</summary>
        private static void EnsureAudioListenerExists()
        {
            if (Object.FindFirstObjectByType<AudioListener>() != null)
                return;

            var listenerGo = new GameObject("TenRush.AudioListener");
            Object.DontDestroyOnLoad(listenerGo);
            listenerGo.AddComponent<AudioListener>();
        }
    }
}
