using System;
using TMPro;
using UnityEngine;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 판이 시작되기 직전 3-2-1 카운트다운. 화면을 완전히 덮어서(불투명) 그
    /// 아래 보드가 안 보이고 탭도 안 먹히게 막는다 — 입력을 따로 막는 코드 없이
    /// 이 오버레이가 화면을 가리는 것만으로 충분하다(타일 탭은 이 오버레이의
    /// 불투명 배경이 raycast를 가로채서 자동으로 막힘).
    /// </summary>
    public sealed class CountdownOverlay : MonoBehaviour
    {
        private const float SecondsPerCount = 1f;

        private int _remaining;
        private float _timer;
        private TextMeshProUGUI _numberText;
        private Action _onComplete;

        /// <param name="seconds">카운트다운 초 수(예: 3 → "3","2","1" 순서로 1초씩).</param>
        public static CountdownOverlay Create(Transform parent, int seconds, Action onComplete)
        {
            var rootRect = UiFactory.CreatePanel(parent, "CountdownOverlay", UiTheme.CountdownBackground);
            UiFactory.Stretch(rootRect);
            var overlay = rootRect.gameObject.AddComponent<CountdownOverlay>();
            overlay._remaining = seconds;
            overlay._timer = SecondsPerCount;
            overlay._onComplete = onComplete;

            overlay._numberText = UiFactory.CreateText(rootRect, "Number", seconds.ToString(), UiTheme.CountdownFontSize, Color.white);
            UiFactory.Stretch((RectTransform)overlay._numberText.transform);

            return overlay;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            _timer += SecondsPerCount;
            _remaining--;

            if (_remaining <= 0)
            {
                var callback = _onComplete;
                Destroy(gameObject);
                callback?.Invoke();
                return;
            }

            _numberText.text = _remaining.ToString();
        }
    }
}
