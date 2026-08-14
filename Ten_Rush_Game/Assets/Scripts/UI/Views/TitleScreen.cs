using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>게임 제목 + 시작 버튼만 있는 최소 타이틀 화면.</summary>
    public sealed class TitleScreen : MonoBehaviour
    {
        public static TitleScreen Create(Transform parent, Action onStart)
        {
            var rootRect = UiFactory.CreatePanel(parent, "TitleScreen", UiTheme.Background);
            UiFactory.Stretch(rootRect);
            var screen = rootRect.gameObject.AddComponent<TitleScreen>();

            var title = UiFactory.CreateText(rootRect, "Title", "TEN RUSH", UiTheme.TitleFontSize, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, 80f);
            UiFactory.SetSize(title.rectTransform, 900f, 140f);

            var hint = UiFactory.CreateText(rootRect, "Hint", "Add up to 10 to clear tiles", UiTheme.HintFontSize, UiTheme.SubInk);
            UiFactory.SetAnchor(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(hint.rectTransform, 0f, -10f);
            UiFactory.SetSize(hint.rectTransform, 700f, 60f);

            var startButton = UiFactory.CreateButton(rootRect, "StartButton", "START", 320f, 100f, UiTheme.Accent, Color.black, UiTheme.ButtonFontSize);
            var buttonRect = (RectTransform)startButton.transform;
            UiFactory.SetAnchor(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(buttonRect, 0f, -160f);
            startButton.onClick.AddListener(() => onStart?.Invoke());

            return screen;
        }
    }
}
