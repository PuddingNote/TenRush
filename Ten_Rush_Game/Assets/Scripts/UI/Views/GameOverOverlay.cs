using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>시간 종료 시 뜨는 반투명 오버레이. 최종 점수 + 다시하기 버튼.</summary>
    public sealed class GameOverOverlay : MonoBehaviour
    {
        private TextMeshProUGUI _finalScoreValue;

        public static GameOverOverlay Create(Transform parent, Action onRetry)
        {
            var rootRect = UiFactory.CreatePanel(parent, "GameOverOverlay", new Color(0.04f, 0.04f, 0.1f, 0.82f));
            UiFactory.Stretch(rootRect);
            var overlay = rootRect.gameObject.AddComponent<GameOverOverlay>();

            var cardRect = UiFactory.CreatePanel(rootRect, "Card", UiTheme.BoardBackground);
            UiFactory.SetAnchor(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetSize(cardRect, 640f, 420f);

            var title = UiFactory.CreateText(cardRect, "Title", "TIME UP", 56f, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, -60f);
            UiFactory.SetSize(title.rectTransform, 560f, 80f);

            overlay._finalScoreValue = UiFactory.CreateText(cardRect, "FinalScore", "SCORE 0", UiTheme.HintFontSize, UiTheme.SubInk);
            UiFactory.SetAnchor(overlay._finalScoreValue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(overlay._finalScoreValue.rectTransform, 0f, -140f);
            UiFactory.SetSize(overlay._finalScoreValue.rectTransform, 560f, 50f);

            var retryButton = UiFactory.CreateButton(cardRect, "RetryButton", "RETRY", 320f, 100f, UiTheme.Accent, Color.black, UiTheme.ButtonFontSize);
            var retryRect = (RectTransform)retryButton.transform;
            UiFactory.SetAnchor(retryRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            UiFactory.SetAnchoredPosition(retryRect, 0f, 60f);
            retryButton.onClick.AddListener(() => onRetry?.Invoke());

            overlay.gameObject.SetActive(false);
            return overlay;
        }

        public void Show(int finalScore)
        {
            _finalScoreValue.text = $"SCORE {finalScore}";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
