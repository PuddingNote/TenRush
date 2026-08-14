using TMPro;
using UnityEngine;

namespace TenRush.UI
{
    /// <summary>
    /// 색상·폰트·크기 등 모든 매직 넘버를 여기 한 곳에 모아 둔다. 디자인을 바꿀 땐
    /// 이 파일만 고치면 된다(Dice Battle의 UiTheme 패턴 재사용). 값은 프로토타입
    /// (docs/design/ten_match_prototype.html)의 CSS 변수를 그대로 옮긴 것.
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color Background = HexColor("#14172B");
        public static readonly Color BoardBackground = HexColor("#20244A");
        public static readonly Color BoardBorder = HexColor("#323874");
        public static readonly Color Ink = HexColor("#EEF0FF");
        public static readonly Color SubInk = HexColor("#8B90C4");
        public static readonly Color Danger = HexColor("#FF5D7A");
        public static readonly Color Accent = HexColor("#6EE7B7");
        public static readonly Color Select = HexColor("#FDE68A");

        public const float TileSize = 138f;
        public const float TileSpacing = 10f;
        public const float TileCornerBoost = 1.08f; // 선택 시 살짝 커지는 배율

        public const float HudValueFontSize = 56f;
        public const float HudLabelFontSize = 24f;
        public const float TileFontSize = 44f;
        public const float ComboFontSize = 32f;
        public const float TitleFontSize = 96f;
        public const float ButtonFontSize = 40f;
        public const float HintFontSize = 26f;

        public const float ClearAnimSeconds = 0.18f; // 프로토타입 setTimeout(180ms)과 동일
        public const float DropInAnimSeconds = 0.16f;
        public const float MismatchShakeSeconds = 0.28f;

        private const string FontResourcePath = "Fonts/ONE Mobile POP SDF";
        private static TMP_FontAsset _font;

        /// <summary>이 게임의 유일한 폰트. Resources/Fonts에서 로드한다(씬 직렬화 참조 없이 코드로만 UI를 짓기 위함).</summary>
        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.Load<TMP_FontAsset>(FontResourcePath);
                    if (_font == null)
                        Debug.LogError($"[TenRush] Font not found at Resources/{FontResourcePath}");
                }
                return _font;
            }
        }

        private static Color HexColor(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
        }
    }
}
