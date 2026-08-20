using UnityEngine;

namespace TenRush.UI
{
    /// <summary>
    /// 타일/버튼/선택 테두리/배경/다이얼로그에 쓸 흰색 스프라이트를 담아 두는
    /// ScriptableObject. UI 전체가 코드로만 지어져서 클립을 끌어다 놓을 씬
    /// 오브젝트가 없는 것과 같은 이유로(AudioLibrary와 동일 패턴), 에셋 하나로
    /// 인스펙터에서 바로 스프라이트를 끌어다 놓을 수 있게 한다.
    ///
    /// 사용법: 프로젝트 창에서 우클릭 → Create → TenRush → UI Sprite Library,
    /// 반드시 <c>Assets/Resources/UI/UiSpriteLibrary.asset</c> 경로에 저장할 것
    /// (UiSprites가 Resources.Load로 이 경로를 찾는다).
    ///
    /// 모든 스프라이트는 흰색으로 제작되고 Image.color로 틴트되므로, 실제 색상은
    /// UiTheme의 색상 상수가 그대로 결정한다 — 스프라이트를 넣어도 지금 보이는
    /// 색은 안 바뀐다.
    /// </summary>
    [CreateAssetMenu(fileName = "UiSpriteLibrary", menuName = "TenRush/UI Sprite Library")]
    public sealed class UiSpriteLibrary : ScriptableObject
    {
        [Header("숫자 타일 (white_square_rounded_128)")]
        [SerializeField] private Sprite tileSprite;

        [Header("타일 그림자 (tile_drop_shadow_128) — 타일 뒤에 깔림")]
        [SerializeField] private Sprite tileShadowSprite;

        [Header("타일 셰이딩 오버레이 (tile_shading_overlay_128) — 타일 위에 그대로 덮음")]
        [SerializeField] private Sprite tileShadingOverlaySprite;

        [Header("숫자 선택 테두리 (line_box_white_square_128)")]
        [SerializeField] private Sprite selectionBorderSprite;

        [Header("버튼 (white_square_rounded_128)")]
        [SerializeField] private Sprite buttonSprite;

        [Header("다이얼로그 UI (white_square_rounded_128)")]
        [SerializeField] private Sprite dialogSprite;

        [Header("배경 (white_square_128)")]
        [SerializeField] private Sprite backgroundSprite;

        [Header("설정 화면 슬라이더 핸들 (나중에 직접 추가)")]
        [SerializeField] private Sprite sliderHandleSprite;

        public Sprite TileSprite => tileSprite;
        public Sprite TileShadowSprite => tileShadowSprite;
        public Sprite TileShadingOverlaySprite => tileShadingOverlaySprite;
        public Sprite SelectionBorderSprite => selectionBorderSprite;
        public Sprite ButtonSprite => buttonSprite;
        public Sprite DialogSprite => dialogSprite;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite SliderHandleSprite => sliderHandleSprite;
    }
}
