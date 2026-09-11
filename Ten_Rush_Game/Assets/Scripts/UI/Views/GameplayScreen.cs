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

        /// <summary>타임업 연장 오퍼를 이미 띄웠는지(봤든 사양했든) — 판당 1회만.</summary>
        private bool _timeExtensionOffered;
        private bool _offerDialogOpen;

        private System.Action _onExitToTitle;
        private bool _confirmDialogOpen;

        /// <summary>시작/재시작 직후 3-2-1 카운트다운이 도는 동안 true — 타이머도, 뒤로가기 확인창도 안 뜬다.</summary>
        private bool _countdownActive;

        private const int CountdownSeconds = 3;

        public static GameplayScreen Create(Transform parent, System.Action onExitToTitle)
        {
            var rootRect = UiFactory.CreatePanel(parent, "GameplayScreen", UiTheme.Background, UiSprites.Background);
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

            screen.StartCountdown();

            return screen;
        }

        private void StartCountdown()
        {
            _countdownActive = true;
            CountdownOverlay.Create(transform, CountdownSeconds, () => _countdownActive = false);
        }

        private void Update()
        {
            if (_countdownActive)
                return;

            if (!_round.IsGameOver)
            {
                _round.Tick(Time.deltaTime);
                _hud.SetTime(_round.TimeRemainingSeconds);

                if (_round.IsGameOver && !_gameOverShown)
                    HandleRoundEnded();
            }

            if (!_gameOverShown && !_confirmDialogOpen && !_offerDialogOpen
                && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
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

        /// <summary>타이머가 0에 닿은 순간. 아직 이번 판에서 연장 오퍼를 안 썼고 광고도 준비돼 있으면 오퍼부터 보여준다.</summary>
        private void HandleRoundEnded()
        {
            if (!_timeExtensionOffered && AdManager.IsRewardedReady)
            {
                _timeExtensionOffered = true;
                _offerDialogOpen = true;
                TimeUpOfferDialog.Create(transform, extended =>
                {
                    _offerDialogOpen = false;
                    if (extended)
                    {
                        _round.ExtendTime(15f);
                        _hud.SetTime(_round.TimeRemainingSeconds);
                    }
                    else
                    {
                        ShowResultsScreen();
                    }
                });
            }
            else
            {
                ShowResultsScreen();
            }
        }

        private void ShowResultsScreen()
        {
            _gameOverShown = true;
            AdFrequencyStore.RecordRoundPlayed();
            int best = HighScoreStore.SaveIfHigher(_round.Score);
            _gameOverOverlay.Show(_round.Score, best);
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
            _timeExtensionOffered = false;
            _gameOverOverlay.Hide();
            _board.RefreshAll();
            _hud.SetScore(0);
            _hud.SetTime(_round.TimeRemainingSeconds);
            StartCountdown();
        }
    }
}
