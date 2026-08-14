using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// BGM/SFX 재생. 실제 클립은 <see cref="AudioLibrary"/> 에셋(인스펙터에서 끌어다
    /// 놓는 방식)에서 가져온다. 아직 클립을 안 넣었거나 에셋 자체가 없어도(1인 개발
    /// 초기 단계) 매치음만큼은 합성한 대체음으로 조용히 대신 재생해서, 오디오
    /// 파이프라인이 없다고 게임이 무음이 되거나 죽지 않게 한다.
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

        /// <summary>AppRoot 부팅 시 한 번 호출. 라이브러리에 BGM 클립이 있을 때만 재생 시작(없으면 조용히 스킵).</summary>
        public static void PlayBgmIfConfigured()
        {
            EnsureInitialized();

            var clip = _library != null ? _library.BgmClip : null;
            if (clip == null)
                return;

            _bgmSource.clip = clip;
            _bgmSource.volume = _library.BgmVolume;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public static void PlayMatch(int comboCount)
        {
            EnsureInitialized();

            var clip = (_library != null ? _library.MatchSfx : null) ?? _fallbackMatchClip;
            float volume = _library != null ? _library.SfxVolume : 1f;

            _sfxSource.pitch = Mathf.Min(MaxPitch, BasePitch + (comboCount - 1) * PitchStepPerCombo);
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

            _fallbackMatchClip = GenerateFallbackMatchClip();
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

        private static AudioClip GenerateFallbackMatchClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            const float baseFrequency = 880f; // A5 — 짧고 경쾌한 '딩' 느낌

            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 14f); // 빠르게 감쇠
                float wave =
                    Mathf.Sin(2f * Mathf.PI * baseFrequency * t) * 0.7f +
                    Mathf.Sin(2f * Mathf.PI * baseFrequency * 2f * t) * 0.3f; // 배음 살짝 섞어 종소리 느낌
                samples[i] = wave * envelope;
            }

            var clip = AudioClip.Create("TenRush.FallbackMatchTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
