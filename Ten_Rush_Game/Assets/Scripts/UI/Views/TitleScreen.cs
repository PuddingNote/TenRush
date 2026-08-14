using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>게임 제목 + 시작 버튼만 있는 최소 타이틀 화면.</summary>
    public sealed class TitleScreen : MonoBehaviour
    {
        private bool _confirmDialogOpen;

        public static TitleScreen Create(Transform parent, Action onStart)
        {
            var rootRect = UiFactory.CreatePanel(parent, "TitleScreen", UiTheme.Background);
            UiFactory.Stretch(rootRect);
            var screen = rootRect.gameObject.AddComponent<TitleScreen>();

            // 전부 상단 기준(anchor top-center)으로 배치. 좌표는 실측으로 확정된 값.
            var title = UiFactory.CreateText(rootRect, "Title", "TEN RUSH", UiTheme.TitleFontSize, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, -600f);
            UiFactory.SetSize(title.rectTransform, 1000f, 180f);

            var hint = UiFactory.CreateText(rootRect, "Hint", "Add up to 10 to clear tiles", UiTheme.HintFontSize, UiTheme.SubInk);
            UiFactory.SetAnchor(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(hint.rectTransform, 0f, -1100f);
            UiFactory.SetSize(hint.rectTransform, 900f, 70f);

            var startButton = UiFactory.CreateButton(rootRect, "StartButton", "START", 400f, 120f, UiTheme.Accent, Color.black, UiTheme.ButtonFontSize);
            var buttonRect = (RectTransform)startButton.transform;
            UiFactory.SetAnchor(buttonRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(buttonRect, 0f, -1200f);
            startButton.onClick.AddListener(() => onStart?.Invoke());

            return screen;
        }

        private void Update()
        {
            if (_confirmDialogOpen)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _confirmDialogOpen = true;
                ConfirmDialog.Create(
                    transform,
                    "Exit the game?",
                    "EXIT",
                    QuitGame,
                    onClosed: () => _confirmDialogOpen = false);
            }
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
