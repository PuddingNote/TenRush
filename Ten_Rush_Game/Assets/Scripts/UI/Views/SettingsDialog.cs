using System;
using TenRush.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>BGM/SFX On·Off + 볼륨 슬라이더. 닫기 버튼 하나만 있는 설정 다이얼로그.</summary>
    public sealed class SettingsDialog : MonoBehaviour
    {
        private Action _onClosed;
        private bool _closed;

        public static SettingsDialog Create(Transform parent, Action onClosed)
        {
            var rootRect = UiFactory.CreatePanel(parent, "SettingsDialog", new Color(0.04f, 0.04f, 0.1f, 0.82f));
            UiFactory.Stretch(rootRect);
            var dialog = rootRect.gameObject.AddComponent<SettingsDialog>();
            dialog._onClosed = onClosed;

            var cardRect = UiFactory.CreatePanel(rootRect, "Card", UiTheme.BoardBackground, UiSprites.Dialog);
            UiFactory.SetAnchor(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetSize(cardRect, 920f, 760f);

            var title = UiFactory.CreateText(cardRect, "Title", "SETTINGS", 64f, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, -90f);
            UiFactory.SetSize(title.rectTransform, 700f, 100f);

            CreateAudioRow(
                cardRect, "BGM", -260f,
                AudioSettingsStore.BgmEnabled, AudioSettingsStore.BgmVolume,
                enabled =>
                {
                    AudioSettingsStore.BgmEnabled = enabled;
                    AudioManager.ApplyBgmVolume();
                },
                volume =>
                {
                    AudioSettingsStore.BgmVolume = volume;
                    AudioManager.ApplyBgmVolume();
                });

            CreateAudioRow(
                cardRect, "SFX", -430f,
                AudioSettingsStore.SfxEnabled, AudioSettingsStore.SfxVolume,
                enabled => AudioSettingsStore.SfxEnabled = enabled,
                volume => AudioSettingsStore.SfxVolume = volume);

            var closeButton = UiFactory.CreateButton(cardRect, "CloseButton", "CLOSE", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.Accent, Color.black, UiTheme.DialogButtonFontSize);
            var closeRect = (RectTransform)closeButton.transform;
            UiFactory.SetAnchor(closeRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            UiFactory.SetAnchoredPosition(closeRect, 0f, 90f);
            closeButton.onClick.AddListener(dialog.Close);

            return dialog;
        }

        /// <summary>라벨 + On/Off 토글 + 볼륨 슬라이더 한 줄. rowCenterY는 카드 위쪽 기준 음수 오프셋(그 줄의 세로 중심).</summary>
        private static void CreateAudioRow(
            Transform card,
            string label,
            float rowCenterY,
            bool initialEnabled,
            float initialVolume,
            Action<bool> onToggle,
            Action<float> onVolume)
        {
            var labelText = UiFactory.CreateText(card, $"{label}Label", label, 44f, UiTheme.Ink, TextAlignmentOptions.Left);
            UiFactory.SetAnchor(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
            UiFactory.SetAnchoredPosition(labelText.rectTransform, 60f, rowCenterY);
            UiFactory.SetSize(labelText.rectTransform, 180f, 70f);

            var toggleButton = CreateToggleButton(card, $"{label}Toggle", initialEnabled, onToggle);
            var toggleRect = (RectTransform)toggleButton.transform;
            UiFactory.SetAnchor(toggleRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
            UiFactory.SetAnchoredPosition(toggleRect, 260f, rowCenterY);

            var slider = UiFactory.CreateSlider(card, $"{label}Slider", 380f, 60f, initialVolume);
            UiFactory.SetAnchor((RectTransform)slider.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
            UiFactory.SetAnchoredPosition((RectTransform)slider.transform, 460f, rowCenterY);
            slider.onValueChanged.AddListener(v => onVolume?.Invoke(v));
        }

        private static Button CreateToggleButton(Transform parent, string name, bool initialOn, Action<bool> onChanged)
        {
            bool isOn = initialOn;
            var button = UiFactory.CreateButton(parent, name, isOn ? "ON" : "OFF", 150f, 90f, isOn ? UiTheme.Accent : UiTheme.SubInk, Color.black, UiTheme.DialogButtonFontSize);
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            var image = button.GetComponent<Image>();

            button.onClick.AddListener(() =>
            {
                isOn = !isOn;
                label.text = isOn ? "ON" : "OFF";
                image.color = isOn ? UiTheme.Accent : UiTheme.SubInk;
                onChanged?.Invoke(isOn);
            });

            return button;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void Close()
        {
            if (_closed)
                return;
            _closed = true;

            Destroy(gameObject);
            _onClosed?.Invoke();
        }
    }
}
