using UnityEngine;
using TenRush.Core;

namespace TenRush.UI
{
    /// <summary>
    /// 기획서 11장 확정 공식: hue = (숫자값-1) * 40, HSL(hue, 70%, 60%).
    /// 색상 에셋 없이 코드로만 타일 색을 만든다.
    /// </summary>
    public static class TileColor
    {
        private const float Saturation = 0.70f;
        private const float Lightness = 0.60f;

        public static Color For(int tileValue)
        {
            float hueDegrees = (tileValue - GridConstants.MinTileValue) * 40f;
            float hue01 = (hueDegrees % 360f) / 360f;
            return HslToRgb(hue01, Saturation, Lightness);
        }

        private static Color HslToRgb(float h, float s, float l)
        {
            if (s <= 0f)
                return new Color(l, l, l);

            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;

            float r = HueToChannel(p, q, h + 1f / 3f);
            float g = HueToChannel(p, q, h);
            float b = HueToChannel(p, q, h - 1f / 3f);
            return new Color(r, g, b);
        }

        private static float HueToChannel(float p, float q, float t)
        {
            if (t < 0f) t += 1f;
            if (t > 1f) t -= 1f;

            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }
    }
}
