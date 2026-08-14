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
        private RectTransform _timerTrack; // 100% 폭의 기준이 되는 바탕 바
        private RectTransform _timerFillRect;
        private Image _timerFillImage;
        private TextMeshProUGUI _comboText;
        private CanvasGroup _comboGroup;
        private Coroutine _comboHideRoutine;

        public static HudView Create(Transform parent)
        {
            var rootRect = UiFactory.CreatePanel(parent, "Hud", Color.clear);
            UiFactory.Stretch(rootRect); // 이게 빠지면 기본 100x100 크기로 화면 중앙에 뭉쳐버린다.
            var hud = rootRect.gameObject.AddComponent<HudView>();

            const float margin = UiTheme.HudMargin;

            // 점수(좌) / 시간(우)
            var scoreLabel = UiFactory.CreateText(rootRect, "ScoreLabel", "SCORE", UiTheme.HudLabelFontSize, UiTheme.SubInk, TextAlignmentOptions.TopLeft);
            UiFactory.SetAnchor(scoreLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            UiFactory.SetAnchoredPosition(scoreLabel.rectTransform, margin, -margin);
            UiFactory.SetSize(scoreLabel.rectTransform, 300f, 40f);

            hud._scoreValue = UiFactory.CreateText(rootRect, "ScoreValue", "0", UiTheme.HudValueFontSize, UiTheme.Ink, TextAlignmentOptions.TopLeft);
            UiFactory.SetAnchor(hud._scoreValue.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            UiFactory.SetAnchoredPosition(hud._scoreValue.rectTransform, margin, -margin - 46f);
            UiFactory.SetSize(hud._scoreValue.rectTransform, 340f, 90f);

            var timeLabel = UiFactory.CreateText(rootRect, "TimeLabel", "TIME", UiTheme.HudLabelFontSize, UiTheme.SubInk, TextAlignmentOptions.TopRight);
            UiFactory.SetAnchor(timeLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            UiFactory.SetAnchoredPosition(timeLabel.rectTransform, -margin, -margin);
            UiFactory.SetSize(timeLabel.rectTransform, 300f, 40f);

            hud._timeValue = UiFactory.CreateText(rootRect, "TimeValue", "60", UiTheme.HudValueFontSize, UiTheme.Ink, TextAlignmentOptions.TopRight);
            UiFactory.SetAnchor(hud._timeValue.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            UiFactory.SetAnchoredPosition(hud._timeValue.rectTransform, -margin, -margin - 46f);
            UiFactory.SetSize(hud._timeValue.rectTransform, 300f, 90f);

            // 타이머 바 — 트랙(바탕)은 화면 폭보다 좁게 여백을 더 줘서 시각적으로 가늘게.
            const float barSideMargin = margin + 60f;
            var barBg = UiFactory.CreatePanel(rootRect, "TimeBarBg", new Color(1f, 1f, 1f, 0.08f));
            UiFactory.SetAnchor(barBg, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(barBg, 0f, -220f);
            UiFactory.SetSize(barBg, -barSideMargin * 2f, 20f);
            hud._timerTrack = barBg;

            // Image.fillAmount(Type.Filled) 대신 실제 폭을 직접 계산해서 넣는다 — 스프라이트가
            // 없는 Image에서 Filled 타입이 시각적으로 줄지 않는 문제를 우회하기 위함.
            var barFillRect = UiFactory.CreatePanel(barBg, "TimeBarFill", UiTheme.Accent);
            UiFactory.SetAnchor(barFillRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
            UiFactory.SetAnchoredPosition(barFillRect, 0f, 0f);
            hud._timerFillRect = barFillRect;
            hud._timerFillImage = barFillRect.GetComponent<Image>();

            // 콤보 팝업
            hud._comboText = UiFactory.CreateText(rootRect, "Combo", string.Empty, UiTheme.ComboFontSize, UiTheme.Select);
            UiFactory.SetAnchor(hud._comboText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(hud._comboText.rectTransform, 0f, -270f);
            UiFactory.SetSize(hud._comboText.rectTransform, 500f, 60f);
            hud._comboGroup = hud._comboText.gameObject.AddComponent<CanvasGroup>();
            hud._comboGroup.alpha = 0f;

            hud.SetTime(GridConstants.RoundTimeSeconds);

            return hud;
        }

        public void SetScore(int score) => _scoreValue.text = score.ToString();

        public void SetTime(float timeRemainingSeconds)
        {
            int displaySeconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemainingSeconds));
            _timeValue.text = displaySeconds.ToString();

            float pct = Mathf.Clamp01(timeRemainingSeconds / GridConstants.RoundTimeSeconds);
            float trackWidth = _timerTrack.rect.width;
            _timerFillRect.sizeDelta = new Vector2(trackWidth * pct, 0f);
            _timerFillImage.color = timeRemainingSeconds <= GridConstants.TimerWarningThresholdSeconds ? UiTheme.Danger : UiTheme.Accent;
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
