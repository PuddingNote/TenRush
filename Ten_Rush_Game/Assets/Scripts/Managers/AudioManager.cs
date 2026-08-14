using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// BGM/SFX 재생. 실제 클립은 <see cref="AudioLibrary"/> 에셋(인스펙터에서 끌어다
    /// 놓는 방식)에서 가져오고, On/Off·볼륨은 <see cref="AudioSettingsStore"/>(설정
    /// 화면에서 조절, 로컬 저장)를 따른다. 아직 클립을 안 넣었거나 에셋 자체가
    /// 없어도(1인 개발 초기 단계) 매치음만큼은 합성한 대체음으로 조용히 대신
    /// 재생해서, 오디오 파이프라인이 없다고 게임이 무음이 되거나 죽지 않게 한다.
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
        private static AudioClip _fallbackMatchClip;
        private static AudioClip _fallbackButtonClickClip;

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

        public static void PlayMatch(int comboCount)
        {
            EnsureInitialized();

            if (!AudioSettingsStore.SfxEnabled)
                return;

            var clip = (_library != null ? _library.MatchSfx : null) ?? _fallbackMatchClip;
            float baseVolume = _library != null ? _library.SfxVolume : 1f;
            float volume = baseVolume * AudioSettingsStore.SfxVolume;

            _sfxSource.pitch = Mathf.Min(MaxPitch, BasePitch + (comboCount - 1) * PitchStepPerCombo);
            _sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>버튼(타일 제외 — UiFactory.CreateButton으로 만든 모든 버튼)을 누를 때마다 자동 재생.</summary>
        public static void PlayButtonClick()
        {
            EnsureInitialized();

            if (!AudioSettingsStore.SfxEnabled)
                return;

            var clip = (_library != null ? _library.ButtonClickSfx : null) ?? _fallbackButtonClickClip;
            float baseVolume = _library != null ? _library.SfxVolume : 1f;
            float volume = baseVolume * AudioSettingsStore.SfxVolume;

            _sfxSource.pitch = BasePitch;
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

            _fallbackMatchClip = GenerateTone("TenRush.FallbackMatchTone", baseFrequency: 880f, duration: 0.18f, decay: 14f, withHarmonic: true);
            _fallbackButtonClickClip = GenerateTone("TenRush.FallbackButtonClick", baseFrequency: 1400f, duration: 0.05f, decay: 40f, withHarmonic: false);
            _library = Resources.Load<AudioLibrary>(LibraryResourcePath);
            // _library가 null이어도(AudioLibrary.asset을 아직 안 만들었어도) 정상 —
            // 위의 대체음으로 계속 동작한다.
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

        /// <summary>
        /// 실제 SFX 에셋이 없을 때 쓰는 대체음 합성기. 사인파 + (옵션) 배음을 지수
        /// 감쇠 엔벨로프로 감싼다. 매치음(길고 종소리 느낌)과 버튼 클릭음(짧고
        /// 딱딱한 느낌)을 파라미터만 다르게 줘서 같은 함수로 만든다.
        /// </summary>
        private static AudioClip GenerateTone(string name, float baseFrequency, float duration, float decay, bool withHarmonic)
        {
            const int sampleRate = 44100;

            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * decay);
                float wave = Mathf.Sin(2f * Mathf.PI * baseFrequency * t);
                if (withHarmonic)
                    wave = wave * 0.7f + Mathf.Sin(2f * Mathf.PI * baseFrequency * 2f * t) * 0.3f;
                samples[i] = wave * envelope;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
