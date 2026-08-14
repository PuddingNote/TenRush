using System;
using System.Collections;
using UnityEngine;

namespace TenRush.UI
{
    /// <summary>
    /// DOTween 없이 코루틴만으로 만든 최소한의 트윈 모음. 이 프로젝트엔 아직
    /// DOTween이 설치돼 있지 않아서(에셋스토어 임포트 필요) 우선 이걸로 대체함 —
    /// docs/design/Decisions.md 참고. 나중에 DOTween을 넣으면 이 클래스만
    /// 교체하면 된다(호출부는 그대로 두고 내부 구현만 바꾸는 식).
    /// </summary>
    public static class UiTween
    {
        public static IEnumerator ScaleTo(Transform target, Vector3 from, Vector3 to, float seconds)
        {
            target.localScale = from;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(from, to, seconds <= 0f ? 1f : t / seconds);
                yield return null;
            }
            target.localScale = to;
        }

        public static IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float seconds)
        {
            group.alpha = from;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, seconds <= 0f ? 1f : t / seconds);
                yield return null;
            }
            group.alpha = to;
        }

        public static IEnumerator Shake(RectTransform target, float amplitude, float seconds)
        {
            Vector2 original = target.anchoredPosition;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                // 프로토타입의 shake 키프레임(좌 -> 우 반복)을 사인파로 근사.
                float progress = seconds <= 0f ? 1f : t / seconds;
                float offset = Mathf.Sin(progress * Mathf.PI * 4f) * amplitude * (1f - progress);
                target.anchoredPosition = original + new Vector2(offset, 0f);
                yield return null;
            }
            target.anchoredPosition = original;
        }

        public static IEnumerator RunThen(IEnumerator routine, Action onComplete)
        {
            yield return routine;
            onComplete?.Invoke();
        }
    }
}
