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
            UiFactory.CreateEventSystem();
            var canvas = UiFactory.CreateRootCanvas("RootCanvas", out _);

            ShowTitle(canvas.transform);
        }

        private static void ShowTitle(Transform root)
        {
            TitleScreen title = null;
            title = TitleScreen.Create(root, () =>
            {
                Object.Destroy(title.gameObject);
                GameplayScreen.Create(root);
            });
        }
    }
}
