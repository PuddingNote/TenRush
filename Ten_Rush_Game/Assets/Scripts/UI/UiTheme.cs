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

        public const float TileSize = 152f;
        public const float TileSpacing = 12f;
        public const float TileCornerBoost = 1.08f; // 선택 시 살짝 커지는 배율

        public const float HudValueFontSize = 72f;
        public const float HudLabelFontSize = 32f;
        public const float TileFontSize = 52f;
        public const float ComboFontSize = 40f;
        public const float TitleFontSize = 140f;
        public const float ButtonFontSize = 60f;
        public const float HintFontSize = 50f;
        public const float DialogMessageFontSize = 54f;
        public const float DialogButtonFontSize = 55f; // ConfirmDialog/GameOverOverlay 버튼 전용 — "MAIN MENU" 등 긴 라벨이 넘치지 않게 ButtonFontSize보다 살짝 작게

        public const float HudMargin = 56f; // 화면 모서리로부터 스코어/타이머가 떨어지는 여백

        // 확인창/게임오버의 "왼쪽·오른쪽 버튼 한 줄" 배치에 쓰는 공용 크기.
        public const float DialogButtonWidth = 340f;
        public const float DialogButtonHeight = 110f;
        public const float DialogButtonRowOffsetX = 190f; // 카드 중앙에서 좌우로 떨어지는 거리(버튼 사이 간격 40px)

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
