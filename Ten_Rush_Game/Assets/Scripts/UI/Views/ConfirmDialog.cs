using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 메시지 + BACK(취소, 왼쪽) + 확인(오른쪽) 버튼 두 개가 한 줄에 나란히 있는
    /// 범용 확인창. 안드로이드 뒤로가기(Escape 키로 매핑됨)를 눌러도 BACK과
    /// 동일하게 취소로 처리한다.
    /// </summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        private Action _onClosed;
        private bool _closed;

        /// <param name="onClosed">확인/취소 어느 쪽이든 창이 닫힐 때 항상 호출됨 — 호출한 쪽이 "다이얼로그 열림" 플래그를 되돌리는 용도.</param>
        public static ConfirmDialog Create(
            Transform parent,
            string message,
            string confirmLabel,
            Action onConfirm,
            Action onClosed = null)
        {
            var rootRect = UiFactory.CreatePanel(parent, "ConfirmDialog", new Color(0.04f, 0.04f, 0.1f, 0.82f));
            UiFactory.Stretch(rootRect);
            var dialog = rootRect.gameObject.AddComponent<ConfirmDialog>();
            dialog._onClosed = onClosed;

            var cardRect = UiFactory.CreatePanel(rootRect, "Card", UiTheme.BoardBackground, UiSprites.Dialog);
            UiFactory.SetAnchor(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetSize(cardRect, 840f, 420f);

            var text = UiFactory.CreateText(cardRect, "Message", message, UiTheme.DialogMessageFontSize, UiTheme.Ink);
            UiFactory.SetAnchor(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(text.rectTransform, 0f, -60f);
            UiFactory.SetSize(text.rectTransform, 740f, 140f);

            // BACK(왼쪽) / 확인(오른쪽) 한 줄 배치.
            var backButton = UiFactory.CreateButton(cardRect, "BackButton", "BACK", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.SubInk, Color.black, UiTheme.DialogButtonFontSize);
            var backRect = (RectTransform)backButton.transform;
            UiFactory.SetAnchor(backRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(backRect, -UiTheme.DialogButtonRowOffsetX, -80f);
            backButton.onClick.AddListener(dialog.Close);

            var confirmButton = UiFactory.CreateButton(cardRect, "ConfirmButton", confirmLabel, UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.Danger, Color.black, UiTheme.DialogButtonFontSize);
            var confirmRect = (RectTransform)confirmButton.transform;
            UiFactory.SetAnchor(confirmRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(confirmRect, UiTheme.DialogButtonRowOffsetX, -80f);
            confirmButton.onClick.AddListener(() =>
            {
                dialog.Close();
                onConfirm?.Invoke();
            });

            return dialog;
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
