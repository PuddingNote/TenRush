using UnityEngine;

namespace TenRush.UI
{
    /// <summary>
    /// <see cref="UiSpriteLibrary"/> 에셋을 지연 로드해서 스프라이트를 꺼내 쓰는
    /// 헬퍼. 에셋이 아직 없거나(해당 스프라이트 슬롯이 비어 있으면) 전부 null을
    /// 반환하고, UiFactory는 null 스프라이트를 그냥 무시하고 기존처럼 단색
    /// Image로 그린다 — 스프라이트를 나중에 채워도 게임이 깨지지 않는다.
    /// </summary>
    public static class UiSprites
    {
        private const string ResourcePath = "UI/UiSpriteLibrary";

        private static UiSpriteLibrary _library;
        private static bool _attemptedLoad;

        private static UiSpriteLibrary Library
        {
            get
            {
                if (!_attemptedLoad)
                {
                    _attemptedLoad = true;
                    _library = Resources.Load<UiSpriteLibrary>(ResourcePath);
                }
                return _library;
            }
        }

        public static Sprite Tile => Library != null ? Library.TileSprite : null;
        public static Sprite TileShadow => Library != null ? Library.TileShadowSprite : null;
        public static Sprite TileShadingOverlay => Library != null ? Library.TileShadingOverlaySprite : null;
        public static Sprite SelectionBorder => Library != null ? Library.SelectionBorderSprite : null;
        public static Sprite Button => Library != null ? Library.ButtonSprite : null;
        public static Sprite Dialog => Library != null ? Library.DialogSprite : null;
        public static Sprite Background => Library != null ? Library.BackgroundSprite : null;
        public static Sprite SliderHandle => Library != null ? Library.SliderHandleSprite : null;
    }
}
