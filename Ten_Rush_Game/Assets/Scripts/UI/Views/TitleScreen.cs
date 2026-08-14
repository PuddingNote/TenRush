using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>게임 제목 + Settings/Start 버튼이 있는 최소 타이틀 화면.</summary>
    public sealed class TitleScreen : MonoBehaviour
    {
        private bool _confirmDialogOpen;
        private bool _settingsDialogOpen;

        public static TitleScreen Create(Transform parent, Action onStart)
        {
            var rootRect = UiFactory.CreatePanel(parent, "TitleScreen", UiTheme.Background, UiSprites.Background);
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

            // SETTINGS(왼쪽) / START(오른쪽) 한 줄 배치. 라벨이 길어서(SETTINGS)
            // 다이얼로그 버튼과 같은 폰트 크기(55)로 맞춰 넘치지 않게 한다.
            const float rowOffsetX = 220f; // 버튼 폭 400 + 간격 40 기준

            var settingsButton = UiFactory.CreateButton(rootRect, "SettingsButton", "SETTINGS", 400f, 120f, UiTheme.SubInk, Color.black, UiTheme.DialogButtonFontSize);
            var settingsRect = (RectTransform)settingsButton.transform;
            UiFactory.SetAnchor(settingsRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(settingsRect, -rowOffsetX, -1200f);
            settingsButton.onClick.AddListener(() => screen.OpenSettings());

            var startButton = UiFactory.CreateButton(rootRect, "StartButton", "START", 400f, 120f, UiTheme.Accent, Color.black, UiTheme.DialogButtonFontSize);
            var buttonRect = (RectTransform)startButton.transform;
            UiFactory.SetAnchor(buttonRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(buttonRect, rowOffsetX, -1200f);
            startButton.onClick.AddListener(() => onStart?.Invoke());

            return screen;
        }

        private void OpenSettings()
        {
            if (_settingsDialogOpen)
                return;

            _settingsDialogOpen = true;
            SettingsDialog.Create(transform, onClosed: () => _settingsDialogOpen = false);
        }

        private void Update()
        {
            if (_confirmDialogOpen || _settingsDialogOpen)
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
