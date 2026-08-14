using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TenRush.UI
{
    /// <summary>
    /// 프리팹 없이 런타임 코드로 UI를 짓기 위한 팩토리 메서드 모음(Dice Battle의
    /// UiFactory 패턴 재사용). 실제 화면(TitleScreen, BoardView 등)은 이 팩토리를
    /// 조합해서 계층을 쌓는다.
    /// </summary>
    public static class UiFactory
    {
        public static Canvas CreateRootCanvas(string name, out CanvasScaler scaler)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static EventSystem CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var module = go.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            return go.GetComponent<EventSystem>();
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0f; // 완전 투명한 레이아웃용 패널은 클릭을 가로채지 않게 한다.

            return rect;
        }

        public static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            string content,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = UiTheme.Font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;

            return text;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            float width,
            float height,
            Color background,
            Color labelColor,
            float fontSize)
        {
            var buttonRect = CreatePanel(parent, name, background);
            buttonRect.sizeDelta = new Vector2(width, height);

            var button = buttonRect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.15f);
            colors.disabledColor = Color.Lerp(background, Color.black, 0.4f);
            button.colors = colors;

            var text = CreateText(buttonRect, "Label", label, fontSize, labelColor);
            Stretch((RectTransform)text.transform);

            return button;
        }

        /// <summary>
        /// LayoutGroup이 없는 부모 밑에 자식을 넣을 땐 자식이 부모 크기를 상속받지
        /// 않는다(Unity 기본 100x100 등으로 시작함). 그 함정을 피하기 위한 헬퍼.
        /// </summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void SetSize(RectTransform rect, float width, float height)
        {
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void SetAnchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
        }

        public static void SetAnchoredPosition(RectTransform rect, float x, float y)
        {
            rect.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>
        /// 고정 폭 요소를 VerticalLayoutGroup/HorizontalLayoutGroup 안에 넣을 때,
        /// childForceExpand 때문에 늘어나는 걸 막으려면 그 요소만을 위한 줄로
        /// 감싸고 그 줄의 forceExpand를 꺼야 한다(재사용 노트 5장 함정 1번).
        /// </summary>
        public static HorizontalLayoutGroup WrapInNonExpandingRow(Transform parent, string name, TextAnchor alignment)
        {
            var rowRect = CreatePanel(parent, name, Color.clear);
            var row = rowRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            row.childAlignment = alignment;
            return row;
        }
    }
}
