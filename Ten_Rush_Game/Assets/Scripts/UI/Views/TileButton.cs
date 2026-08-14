using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 그리드 한 칸의 시각적 표현. 게임 규칙은 전혀 모르고(값 표시/선택 하이라이트/
    /// 애니메이션만 담당), 탭이 발생하면 Row/Col을 콜백으로 그대로 올려보낸다.
    /// </summary>
    public sealed class TileButton : MonoBehaviour, IPointerClickHandler
    {
        public int Row { get; private set; }
        public int Col { get; private set; }

        private Image _background;
        private TextMeshProUGUI _label;
        private Outline _selectionOutline;
        private Action<int, int> _onTapped;

        public static TileButton Create(Transform parent, int row, int col, Action<int, int> onTapped)
        {
            var rect = UiFactory.CreatePanel(parent, $"Tile_{row}_{col}", UiTheme.BoardBackground);
            var tile = rect.gameObject.AddComponent<TileButton>();
            tile.Row = row;
            tile.Col = col;
            tile._onTapped = onTapped;
            tile._background = rect.GetComponent<Image>();

            tile._selectionOutline = rect.gameObject.AddComponent<Outline>();
            tile._selectionOutline.effectColor = UiTheme.Select;
            tile._selectionOutline.effectDistance = new Vector2(3f, -3f);
            tile._selectionOutline.enabled = false;

            var textRect = UiFactory.CreateText(rect, "Value", string.Empty, UiTheme.TileFontSize, Color.black);
            UiFactory.Stretch((RectTransform)textRect.transform);
            tile._label = textRect;

            return tile;
        }

        public void SetValue(int value)
        {
            _label.text = value.ToString();
            _background.color = TileColor.For(value);
        }

        public void SetSelected(bool selected)
        {
            _selectionOutline.enabled = selected;
            transform.localScale = selected ? Vector3.one * UiTheme.TileCornerBoost : Vector3.one;
        }

        public void PlayMismatchShake()
        {
            StopAllCoroutines();
            StartCoroutine(UiTween.Shake((RectTransform)transform, amplitude: 6f, UiTheme.MismatchShakeSeconds));
        }

        /// <summary>매치 확정 시 즉시 호출 — 값은 아직 안 바꾸고 사라지는 느낌만 준다.</summary>
        public void PlayClear(Action onComplete)
        {
            StopAllCoroutines();
            var group = GetOrAddCanvasGroup();
            StartCoroutine(UiTween.RunThen(
                UiTween.ScaleTo(transform, Vector3.one, Vector3.one * 0.4f, UiTheme.ClearAnimSeconds),
                () =>
                {
                    onComplete?.Invoke();
                }));
            StartCoroutine(UiTween.FadeCanvasGroup(group, 1f, 0f, UiTheme.ClearAnimSeconds));
        }

        /// <summary>ResolvePendingMatch() 이후, 새 값이 반영된 뒤 다시 나타나는 연출.</summary>
        public void PlayDropIn()
        {
            StopAllCoroutines();
            SetSelected(false);
            var group = GetOrAddCanvasGroup();
            group.alpha = 1f;
            StartCoroutine(UiTween.ScaleTo(transform, Vector3.one * 0.4f, Vector3.one, UiTheme.DropInAnimSeconds));
        }

        private CanvasGroup GetOrAddCanvasGroup()
        {
            var group = GetComponent<CanvasGroup>();
            if (group == null)
                group = gameObject.AddComponent<CanvasGroup>();
            return group;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onTapped?.Invoke(Row, Col);
        }
    }
}
