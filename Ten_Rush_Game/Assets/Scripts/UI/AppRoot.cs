using TenRush.Managers;
using TenRush.UI.Views;
using UnityEngine;

namespace TenRush.UI
{
    /// <summary>
    /// 게임의 유일한 진입점. 씬 파일에 아무 것도 안 넣어도(빈 씬이어도) 이 정적
    /// 메서드가 자동으로 실행되어 Canvas/EventSystem부터 타이틀 화면까지 전부
    /// 코드로 짓는다 — 프리팹도, 씬에 미리 배치해 둘 오브젝트도 없다.
    /// </summary>
    public static class AppRoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            AdManager.Initialize(); // 리워드/전면 광고를 최대한 일찍 로드해 둔다(막상 필요할 때 안 기다리게).

            UiFactory.CreateEventSystem();
            var canvas = UiFactory.CreateRootCanvas("RootCanvas", out _);
            var contentRoot = UiFactory.CreateLetterboxedContentRoot(canvas.transform);

            // AspectRatioFitter는 실제 레이아웃을 다음 캔버스 리빌드 때 계산한다.
            // 그 전에 자식 UI가 contentRoot.rect 크기를 읽으면(타이머 바 등) 값이
            // 아직 기본값일 수 있어서, 여기서 한 번 강제로 즉시 계산시켜 둔다.
            Canvas.ForceUpdateCanvases();

            AudioManager.PlayBgmIfConfigured();

            ShowTitle(contentRoot);
        }

        private static void ShowTitle(Transform root)
        {
            TitleScreen title = null;
            title = TitleScreen.Create(root, onStart: () =>
            {
                Object.Destroy(title.gameObject);
                ShowGameplay(root);
            });
        }

        private static void ShowGameplay(Transform root)
        {
            GameplayScreen screen = null;
            screen = GameplayScreen.Create(root, onExitToTitle: () =>
            {
                Object.Destroy(screen.gameObject);
                ShowTitle(root);
            });
        }
    }
}
