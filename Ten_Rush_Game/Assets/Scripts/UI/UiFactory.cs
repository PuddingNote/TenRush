using TenRush.Managers;
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

        /// <summary>
        /// 기기 화면비가 9:16이 아니어도(태블릿, 늘어난 화면 등) 실제 게임 화면은
        /// 항상 1080x1920 비율로 고정하고, 남는 공간은 검은 여백(letterbox)으로
        /// 채운다. 반환된 RectTransform 밑에 실제 UI를 지으면 된다.
        /// </summary>
        public static RectTransform CreateLetterboxedContentRoot(Transform canvasTransform)
        {
            var background = CreatePanel(canvasTransform, "LetterboxBackground", Color.black);
            Stretch(background);

            // AspectRatioFitter의 FitInParent 모드는 앵커가 스트레치(0,0)-(1,1)가 아니라
            // 한 점(중앙)이어야 sizeDelta를 실제 크기로 취급해 정상 동작한다.
            var contentRoot = CreatePanel(canvasTransform, "ContentRoot", Color.clear);
            SetAnchor(contentRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            SetAnchoredPosition(contentRoot, 0f, 0f);

            var fitter = contentRoot.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1080f / 1920f;

            return contentRoot;
        }

        public static EventSystem CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var module = go.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            return go.GetComponent<EventSystem>();
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0f; // 완전 투명한 레이아웃용 패널은 클릭을 가로채지 않게 한다.

            if (sprite != null)
            {
                // 전부 흰색 원본이라 색은 위 image.color가 그대로 결정한다.
                // Sliced로 그려서 크기가 달라져도(버튼마다 폭이 다름 등) 모서리가 안 뭉개진다.
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }

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
            var buttonRect = CreatePanel(parent, name, background, UiSprites.Button);
            buttonRect.sizeDelta = new Vector2(width, height);

            var button = buttonRect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.15f);
            colors.disabledColor = Color.Lerp(background, Color.black, 0.4f);
            button.colors = colors;

            var text = CreateText(buttonRect, "Label", label, fontSize, labelColor);
            Stretch((RectTransform)text.transform);

            // 이 팩토리로 만든 버튼은(타일 제외 — TileButton은 CreateButton을 안 씀)
            // 전부 클릭할 때 자동으로 버튼 선택음이 나게 한다. 각 화면에서 따로
            // 챙기지 않아도 되게 하려는 것.
            button.onClick.AddListener(AudioManager.PlayButtonClick);

            return button;
        }

        /// <summary>
        /// 0~1 범위의 가로 슬라이더. Unity 기본 Slider 프리팹과 동일한 구조
        /// (Background/FillArea+Fill/HandleSlideArea+Handle)를 코드로 그대로 짓는다.
        /// </summary>
        public static Slider CreateSlider(Transform parent, string name, float width, float height, float value)
        {
            const float handleSize = 36f;
            const float trackHeight = 14f;
            const float handleVerticalOverhang = 5f; // 아래 Handle 주석 참고

            var rootRect = CreatePanel(parent, name, Color.clear);
            SetSize(rootRect, width, height);
            var slider = rootRect.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            var bgRect = CreatePanel(rootRect, "Background", new Color(1f, 1f, 1f, 0.12f));
            SetAnchor(bgRect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f));
            SetSize(bgRect, 0f, trackHeight);

            // FillArea/HandleArea를 핸들 반지름만큼 안쪽으로 밀어넣지 않고 트랙과
            // 똑같이 좌우 끝까지 채운다. 그래야 값이 0일 때 왼쪽에 "칠해지지 않는
            // 빈 공간"이 안 생기고, 채움(초록)이 트랙 맨 왼쪽부터 시작한다.
            // 대신 핸들이 양 끝에서 트랙 밖으로 반쯤 걸치는데(일반적인 슬라이더 모양),
            // 카드 안에 여백이 충분해서 잘리지 않는다.
            var fillAreaRect = CreatePanel(rootRect, "FillArea", Color.clear);
            SetAnchor(fillAreaRect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f));
            SetSize(fillAreaRect, 0f, trackHeight);

            var fillRect = CreatePanel(fillAreaRect, "Fill", UiTheme.Accent);
            SetAnchor(fillRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
            SetSize(fillRect, 0f, 0f);

            var handleAreaRect = CreatePanel(rootRect, "HandleSlideArea", Color.clear);
            SetAnchor(handleAreaRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
            SetSize(handleAreaRect, 0f, 0f);

            // 주의: Slider.UpdateVisuals()는 handleRect의 세로 앵커를 항상 (0,1)
            // 풀스트레치로 강제로 덮어쓴다(가로축만 값에 따라 점으로 바꿈) — 여기서
            // 미리 세로를 점으로 고정해도 소용없다. 세로 스트레치 상태에서는
            // sizeDelta.y가 "부모 크기에서 얼마나 더/덜 튀어나오는가"가 되므로,
            // Top/Bottom이 정확히 -5px가 되도록 sizeDelta.y = 5*2 = 10으로 계산한다.
            var handleRect = CreatePanel(handleAreaRect, "Handle", UiTheme.Select, UiSprites.SliderHandle);
            SetSize(handleRect, handleSize, handleVerticalOverhang * 2f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleRect.GetComponent<Image>();
            slider.value = value;

            return slider;
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
