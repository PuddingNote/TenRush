using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>시간 종료 시 뜨는 반투명 오버레이. 최종 점수 + MAIN MENU(왼쪽)/RETRY(오른쪽) 버튼.</summary>
    public sealed class GameOverOverlay : MonoBehaviour
    {
        private TextMeshProUGUI _finalScoreValue;
        private TextMeshProUGUI _bestScoreValue;

        public static GameOverOverlay Create(Transform parent, Action onRetry, Action onMainMenu)
        {
            var rootRect = UiFactory.CreatePanel(parent, "GameOverOverlay", new Color(0.04f, 0.04f, 0.1f, 0.82f));
            UiFactory.Stretch(rootRect);
            var overlay = rootRect.gameObject.AddComponent<GameOverOverlay>();

            var cardRect = UiFactory.CreatePanel(rootRect, "Card", UiTheme.BoardBackground, UiSprites.Dialog);
            UiFactory.SetAnchor(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetSize(cardRect, 840f, 560f);

            var title = UiFactory.CreateText(cardRect, "Title", "TIME UP", 72f, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, -90f);
            UiFactory.SetSize(title.rectTransform, 700f, 100f);

            overlay._finalScoreValue = UiFactory.CreateText(cardRect, "FinalScore", "SCORE 0", UiTheme.HintFontSize, UiTheme.SubInk);
            UiFactory.SetAnchor(overlay._finalScoreValue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(overlay._finalScoreValue.rectTransform, 0f, -210f);
            UiFactory.SetSize(overlay._finalScoreValue.rectTransform, 700f, 70f);

            overlay._bestScoreValue = UiFactory.CreateText(cardRect, "BestScore", "BEST 0", UiTheme.HintFontSize, UiTheme.Select);
            UiFactory.SetAnchor(overlay._bestScoreValue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(overlay._bestScoreValue.rectTransform, 0f, -285f);
            UiFactory.SetSize(overlay._bestScoreValue.rectTransform, 700f, 60f);

            // MAIN MENU(왼쪽) / RETRY(오른쪽) 한 줄 배치.
            var mainMenuButton = UiFactory.CreateButton(cardRect, "MainMenuButton", "MAIN MENU", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.SubInk, Color.black, UiTheme.DialogButtonFontSize);
            var mainMenuRect = (RectTransform)mainMenuButton.transform;
            UiFactory.SetAnchor(mainMenuRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(mainMenuRect, -UiTheme.DialogButtonRowOffsetX, -160f);
            mainMenuButton.onClick.AddListener(() => onMainMenu?.Invoke());

            var retryButton = UiFactory.CreateButton(cardRect, "RetryButton", "RETRY", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.Accent, Color.black, UiTheme.DialogButtonFontSize);
            var retryRect = (RectTransform)retryButton.transform;
            UiFactory.SetAnchor(retryRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(retryRect, UiTheme.DialogButtonRowOffsetX, -160f);
            retryButton.onClick.AddListener(() => onRetry?.Invoke());

            overlay.gameObject.SetActive(false);
            return overlay;
        }

        public void Show(int finalScore, int bestScore)
        {
            _finalScoreValue.text = $"SCORE {finalScore}";
            _bestScoreValue.text = $"BEST {bestScore}";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
