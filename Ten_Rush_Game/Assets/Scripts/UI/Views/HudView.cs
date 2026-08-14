using System.Collections;
using TenRush.Core;
using TenRush.Core.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>점수, 타이머 바, 콤보 팝업. 게임 규칙은 모르고 넘겨받은 값만 표시한다.</summary>
    public sealed class HudView : MonoBehaviour
    {
        private TextMeshProUGUI _scoreValue;
        private TextMeshProUGUI _timeValue;
        private Image _timerFill;
        private TextMeshProUGUI _comboText;
        private CanvasGroup _comboGroup;
        private Coroutine _comboHideRoutine;

        public static HudView Create(Transform parent)
        {
            var rootRect = UiFactory.CreatePanel(parent, "Hud", Color.clear);
            var hud = rootRect.gameObject.AddComponent<HudView>();

            // 점수(좌) / 시간(우)
            var scoreLabel = UiFactory.CreateText(rootRect, "ScoreLabel", "SCORE", UiTheme.HudLabelFontSize, UiTheme.SubInk, TextAlignmentOptions.TopLeft);
            UiFactory.SetAnchor(scoreLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            UiFactory.SetAnchoredPosition(scoreLabel.rectTransform, 8f, -8f);
            UiFactory.SetSize(scoreLabel.rectTransform, 200f, 32f);

            hud._scoreValue = UiFactory.CreateText(rootRect, "ScoreValue", "0", UiTheme.HudValueFontSize, UiTheme.Ink, TextAlignmentOptions.TopLeft);
            UiFactory.SetAnchor(hud._scoreValue.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            UiFactory.SetAnchoredPosition(hud._scoreValue.rectTransform, 8f, -40f);
            UiFactory.SetSize(hud._scoreValue.rectTransform, 260f, 64f);

            var timeLabel = UiFactory.CreateText(rootRect, "TimeLabel", "TIME", UiTheme.HudLabelFontSize, UiTheme.SubInk, TextAlignmentOptions.TopRight);
            UiFactory.SetAnchor(timeLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            UiFactory.SetAnchoredPosition(timeLabel.rectTransform, -8f, -8f);
            UiFactory.SetSize(timeLabel.rectTransform, 200f, 32f);

            hud._timeValue = UiFactory.CreateText(rootRect, "TimeValue", "60", UiTheme.HudValueFontSize, UiTheme.Ink, TextAlignmentOptions.TopRight);
            UiFactory.SetAnchor(hud._timeValue.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            UiFactory.SetAnchoredPosition(hud._timeValue.rectTransform, -8f, -40f);
            UiFactory.SetSize(hud._timeValue.rectTransform, 200f, 64f);

            // 타이머 바
            var barBg = UiFactory.CreatePanel(rootRect, "TimeBarBg", new Color(1f, 1f, 1f, 0.08f));
            UiFactory.SetAnchor(barBg, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(barBg, 0f, -112f);
            UiFactory.SetSize(barBg, -16f, 14f); // 좌우 8px 여백을 위해 음수 오프셋 트릭 대신 SetSize에서 폭 보정

            var barFillRect = UiFactory.CreatePanel(barBg, "TimeBarFill", UiTheme.Accent);
            UiFactory.Stretch(barFillRect);
            hud._timerFill = barFillRect.GetComponent<Image>();
            hud._timerFill.type = Image.Type.Filled;
            hud._timerFill.fillMethod = Image.FillMethod.Horizontal;
            hud._timerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            hud._timerFill.fillAmount = 1f;

            // 콤보 팝업
            hud._comboText = UiFactory.CreateText(rootRect, "Combo", string.Empty, UiTheme.ComboFontSize, UiTheme.Select);
            UiFactory.SetAnchor(hud._comboText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(hud._comboText.rectTransform, 0f, -148f);
            UiFactory.SetSize(hud._comboText.rectTransform, 400f, 40f);
            hud._comboGroup = hud._comboText.gameObject.AddComponent<CanvasGroup>();
            hud._comboGroup.alpha = 0f;

            return hud;
        }

        public void SetScore(int score) => _scoreValue.text = score.ToString();

        public void SetTime(float timeRemainingSeconds)
        {
            int displaySeconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemainingSeconds));
            _timeValue.text = displaySeconds.ToString();

            float pct = Mathf.Clamp01(timeRemainingSeconds / GridConstants.RoundTimeSeconds);
            _timerFill.fillAmount = pct;
            _timerFill.color = timeRemainingSeconds <= GridConstants.TimerWarningThresholdSeconds ? UiTheme.Danger : UiTheme.Accent;
        }

        public void ShowComboIfRelevant(TapResult result)
        {
            if (result.Outcome != TapOutcome.Matched || result.ComboCount < 2)
                return;

            _comboText.text = $"COMBO x{result.ComboCount}  +{result.ScoreGained}";

            if (_comboHideRoutine != null)
                StopCoroutine(_comboHideRoutine);
            _comboGroup.alpha = 1f;
            _comboHideRoutine = StartCoroutine(HideComboAfterDelay());
        }

        private IEnumerator HideComboAfterDelay()
        {
            yield return new WaitForSeconds(0.7f);
            yield return UiTween.FadeCanvasGroup(_comboGroup, 1f, 0f, 0.15f);
        }
    }
}
