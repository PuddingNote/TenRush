using TenRush.Core;
using TenRush.Core.Model;
using TenRush.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 한 판의 화면 전체(보드 + HUD + 게임오버 오버레이)를 구성하고, 매 프레임
    /// GameRound.Tick()을 호출해 타이머를 흘려보낸다. 규칙은 GameRound가, 표시는
    /// BoardView/HudView가 맡고 이 클래스는 그 둘을 잇기만 한다.
    /// </summary>
    public sealed class GameplayScreen : MonoBehaviour
    {
        private GameRound _round;
        private BoardView _board;
        private HudView _hud;
        private GameOverOverlay _gameOverOverlay;
        private bool _gameOverShown;

        private System.Action _onExitToTitle;
        private bool _confirmDialogOpen;

        public static GameplayScreen Create(Transform parent, System.Action onExitToTitle)
        {
            var rootRect = UiFactory.CreatePanel(parent, "GameplayScreen", UiTheme.Background);
            UiFactory.Stretch(rootRect);
            var screen = rootRect.gameObject.AddComponent<GameplayScreen>();
            screen._onExitToTitle = onExitToTitle;

            screen._round = new GameRound();

            var boardRect = UiFactory.CreatePanel(rootRect, "BoardAnchor", Color.clear);
            UiFactory.SetAnchor(boardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.SetAnchoredPosition(boardRect, 0f, -150f);
            UiFactory.SetSize(boardRect, 0f, 0f);

            screen._board = BoardView.Create(boardRect, screen._round);
            screen._board.TapProcessed += screen.OnTapProcessed;

            screen._hud = HudView.Create(rootRect);
            screen._hud.SetScore(0);
            screen._hud.SetTime(GridConstants.RoundTimeSeconds);

            screen._gameOverOverlay = GameOverOverlay.Create(rootRect, screen.OnRetryRequested, () => onExitToTitle?.Invoke());

            return screen;
        }

        private void Update()
        {
            if (!_round.IsGameOver)
            {
                _round.Tick(Time.deltaTime);
                _hud.SetTime(_round.TimeRemainingSeconds);

                if (_round.IsGameOver && !_gameOverShown)
                {
                    _gameOverShown = true;
                    int best = HighScoreStore.SaveIfHigher(_round.Score);
                    _gameOverOverlay.Show(_round.Score, best);
                }
            }

            if (!_gameOverShown && !_confirmDialogOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _confirmDialogOpen = true;
                ConfirmDialog.Create(
                    transform,
                    "Return to title?",
                    "TITLE",
                    () => _onExitToTitle?.Invoke(),
                    onClosed: () => _confirmDialogOpen = false);
            }
        }

        private void OnTapProcessed(TapResult result)
        {
            _hud.SetScore(_round.Score);
            _hud.ShowComboIfRelevant(result);
        }

        private void OnRetryRequested()
        {
            _round.Reset();
            _gameOverShown = false;
            _gameOverOverlay.Hide();
            _board.RefreshAll();
            _hud.SetScore(0);
            _hud.SetTime(_round.TimeRemainingSeconds);
        }
    }
}
