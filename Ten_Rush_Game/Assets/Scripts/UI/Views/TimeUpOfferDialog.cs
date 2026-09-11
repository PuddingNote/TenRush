using System;
using TenRush.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 타임업 순간 뜨는 "광고 보고 +15초?" 오퍼. WATCH AD를 누르면 리워드 광고를
    /// 보여주고, NO THANKS를 누르거나 <see cref="AutoDeclineSeconds"/>초 안에
    /// 아무 것도 안 누르면 자동으로 사양 처리된다(그대로 결과 화면으로).
    ///
    /// 판당 1회 제한이나 광고 로드 여부 확인은 호출하는 쪽(GameplayScreen)의 몫이다
    /// — 이 다이얼로그는 뜬 이상 무조건 광고가 준비돼 있다고 가정한다.
    /// </summary>
    public sealed class TimeUpOfferDialog : MonoBehaviour
    {
        private const float AutoDeclineSeconds = 5f;

        private Action<bool> _onResolved;
        private bool _resolved;
        private float _remaining;
        private TextMeshProUGUI _countdownText;
        private int _lastDisplaySeconds = -1;

        /// <param name="onResolved">true = 광고를 끝까지 봐서 연장 보상을 받음, false = 사양(버튼/시간초과/광고 실패 전부 포함).</param>
        public static TimeUpOfferDialog Create(Transform parent, Action<bool> onResolved)
        {
            var rootRect = UiFactory.CreatePanel(parent, "TimeUpOfferDialog", new Color(0.04f, 0.04f, 0.1f, 0.82f));
            UiFactory.Stretch(rootRect);
            var dialog = rootRect.gameObject.AddComponent<TimeUpOfferDialog>();
            dialog._onResolved = onResolved;
            dialog._remaining = AutoDeclineSeconds;

            // 카드/여백을 넉넉하게 키웠다(폰트 크기는 그대로) — "Watch an ad for +15
            // seconds?"가 2줄로 줄바꿈되는데 이전 카드는 그걸 감안 못 해서 카운트다운
            // 텍스트랑 겹쳤었음.
            var cardRect = UiFactory.CreatePanel(rootRect, "Card", UiTheme.BoardBackground, UiSprites.Dialog);
            UiFactory.SetAnchor(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetSize(cardRect, 900f, 660f);

            var title = UiFactory.CreateText(cardRect, "Title", "TIME'S UP", 64f, UiTheme.Ink);
            UiFactory.SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(title.rectTransform, 0f, -90f);
            UiFactory.SetSize(title.rectTransform, 700f, 100f);

            var message = UiFactory.CreateText(cardRect, "Message", "Watch an ad for\n+15 seconds?", UiTheme.DialogMessageFontSize, UiTheme.SubInk);
            UiFactory.SetAnchor(message.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(message.rectTransform, 0f, -210f);
            UiFactory.SetSize(message.rectTransform, 780f, 150f); // 2줄 줄바꿈까지 여유있게

            dialog._countdownText = UiFactory.CreateText(cardRect, "Countdown", string.Empty, 32f, UiTheme.Select);
            UiFactory.SetAnchor(dialog._countdownText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.SetAnchoredPosition(dialog._countdownText.rectTransform, 0f, -390f);
            UiFactory.SetSize(dialog._countdownText.rectTransform, 400f, 50f);

            var watchButton = UiFactory.CreateButton(cardRect, "WatchAdButton", "WATCH AD", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.Accent, Color.black, UiTheme.DialogButtonFontSize);
            var watchRect = (RectTransform)watchButton.transform;
            UiFactory.SetAnchor(watchRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(watchRect, -UiTheme.DialogButtonRowOffsetX, -245f);
            watchButton.onClick.AddListener(dialog.RequestAd);

            var declineButton = UiFactory.CreateButton(cardRect, "DeclineButton", "NO THANKS", UiTheme.DialogButtonWidth, UiTheme.DialogButtonHeight, UiTheme.SubInk, Color.black, UiTheme.DialogButtonFontSize);
            var declineRect = (RectTransform)declineButton.transform;
            UiFactory.SetAnchor(declineRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(declineRect, UiTheme.DialogButtonRowOffsetX, -245f);
            declineButton.onClick.AddListener(dialog.Decline);

            dialog.UpdateCountdownText();

            return dialog;
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            UpdateCountdownText();

            if (_remaining <= 0f)
                Decline();
        }

        private void UpdateCountdownText()
        {
            // 정수가 실제로 바뀔 때만 문자열을 새로 만든다(매 프레임 string 보간은
            // GC 쓰레기를 만들어서 모바일에서 끊김의 원인이 된다).
            int seconds = Mathf.Max(0, Mathf.CeilToInt(_remaining));
            if (seconds == _lastDisplaySeconds)
                return;

            _lastDisplaySeconds = seconds;
            _countdownText.text = $"Auto-skip in {seconds}s";
        }

        private void RequestAd()
        {
            if (_resolved)
                return;
            _resolved = true;

            Destroy(gameObject);
            AdManager.ShowRewarded(
                onRewardEarned: () => _onResolved?.Invoke(true),
                onDeclinedOrUnavailable: () => _onResolved?.Invoke(false));
        }

        private void Decline()
        {
            if (_resolved)
                return;
            _resolved = true;

            Destroy(gameObject);
            _onResolved?.Invoke(false);
        }
    }
}
