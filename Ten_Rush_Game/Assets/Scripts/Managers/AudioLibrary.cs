using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// BGM/SFX 클립을 담아 두는 ScriptableObject. 씬에 아무 것도 안 놓고(이 프로젝트
    /// UI 전체가 코드로만 지어지는 것과 같은 이유) 에셋 하나로 인스펙터에서 바로
    /// 클립을 끌어다 놓을 수 있게 하기 위함.
    ///
    /// 사용법: 프로젝트 창에서 우클릭 → Create → TenRush → Audio Library, 반드시
    /// <c>Assets/Resources/Audio/AudioLibrary.asset</c> 경로에 저장할 것
    /// (AudioManager가 Resources.Load로 이 경로를 찾는다). 클립을 아직 안 넣었으면
    /// 해당 사운드는 그냥 무음으로 재생되므로, 지금 당장 슬롯이 비어 있어도
    /// 게임은 정상 동작한다.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "TenRush/Audio Library")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;

        [Header("SFX")]
        [SerializeField] private AudioClip buttonClickSfx; // 버튼 터치음
        [SerializeField] private AudioClip tileSelectSfx; // 숫자 타일 터치음
        [SerializeField] private AudioClip matchSuccessSfx; // 합 10 맞을 때
        [SerializeField] private AudioClip matchFailSfx; // 합 10 아닐 때(미스매치)
        [SerializeField] private AudioClip roundEndSfx; // 라운드 종료
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        public AudioClip BgmClip => bgmClip;
        public float BgmVolume => bgmVolume;
        public AudioClip ButtonClickSfx => buttonClickSfx;
        public AudioClip TileSelectSfx => tileSelectSfx;
        public AudioClip MatchSuccessSfx => matchSuccessSfx;
        public AudioClip MatchFailSfx => matchFailSfx;
        public AudioClip RoundEndSfx => roundEndSfx;
        public float SfxVolume => sfxVolume;
    }
}
