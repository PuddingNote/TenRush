using System;
using TenRush.Core;
using TenRush.Core.Model;
using TenRush.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace TenRush.UI.Views
{
    /// <summary>
    /// 그리드 전체(7x6 TileButton)를 만들고 탭을 GameRound로 전달한 뒤, 결과에 따라
    /// 시각적으로 반영한다. 애니메이션 타이밍만 UI가 들고 있고, 실제 판정/점수/
    /// 콤보/막힘방지는 전부 GameRound(Core)가 결정한다.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private GameRound _round;
        private TileButton[,] _tiles;
        private GridPosition? _highlighted;

        public event Action<TapResult> TapProcessed;

        public static BoardView Create(Transform parent, GameRound round)
        {
            var rect = UiFactory.CreatePanel(parent, "Board", UiTheme.BoardBackground);
            var board = rect.gameObject.AddComponent<BoardView>();
            board._round = round;

            var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(UiTheme.TileSize, UiTheme.TileSize);
            grid.spacing = new Vector2(UiTheme.TileSpacing, UiTheme.TileSpacing);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridConstants.Cols;

            float width = GridConstants.Cols * UiTheme.TileSize + (GridConstants.Cols - 1) * UiTheme.TileSpacing;
            float height = GridConstants.Rows * UiTheme.TileSize + (GridConstants.Rows - 1) * UiTheme.TileSpacing;
            UiFactory.SetSize(rect, width, height);

            board._tiles = new TileButton[GridConstants.Rows, GridConstants.Cols];
            for (int r = 0; r < GridConstants.Rows; r++)
            {
                for (int c = 0; c < GridConstants.Cols; c++)
                {
                    var tile = TileButton.Create(rect, r, c, board.OnTileTapped);
                    board._tiles[r, c] = tile;
                }
            }

            board.RefreshAll();
            return board;
        }

        public void RefreshAll()
        {
            for (int r = 0; r < GridConstants.Rows; r++)
                for (int c = 0; c < GridConstants.Cols; c++)
                    _tiles[r, c].SetValue(_round.Grid[r, c]);
        }

        private void OnTileTapped(int row, int col)
        {
            double nowMs = Time.unscaledTime * 1000d;
            var result = _round.Tap(row, col, nowMs);
            Apply(result);
            TapProcessed?.Invoke(result);
        }

        private void Apply(TapResult result)
        {
            switch (result.Outcome)
            {
                case TapOutcome.Selected:
                    SetHighlight(result.Selected);
                    // 타일 선택음(AudioManager.PlayTileSelect)은 2026-09-15에 뺐음 — 한 판에
                    // 타일을 워낙 많이 선택하다 보니 같은 소리가 너무 반복돼서 거슬린다는
                    // 피드백. 라이브러리/메서드는 남겨 뒀으니 필요하면 바로 되살릴 수 있음.
                    break;

                case TapOutcome.Deselected:
                    SetHighlight(null);
                    break;

                case TapOutcome.Mismatched:
                    _tiles[result.MismatchPrevious.Row, result.MismatchPrevious.Col].PlayMismatchShake();
                    _tiles[result.MismatchTapped.Row, result.MismatchTapped.Col].PlayMismatchShake();
                    SetHighlight(null); // 선택을 완전히 해제 — 사용자 피드백으로 체이닝 제거(2026-08-14)
                    AudioManager.PlayMismatch();
                    break;

                case TapOutcome.Matched:
                    SetHighlight(null);
                    AudioManager.PlayMatch(result.ComboCount);
                    AnimateMatch(result.MatchedA, result.MatchedB);
                    break;
            }
        }

        private void SetHighlight(GridPosition? pos)
        {
            if (_highlighted.HasValue)
                _tiles[_highlighted.Value.Row, _highlighted.Value.Col].SetSelected(false);

            _highlighted = pos;

            if (_highlighted.HasValue)
                _tiles[_highlighted.Value.Row, _highlighted.Value.Col].SetSelected(true);
        }

        private void AnimateMatch(GridPosition a, GridPosition b)
        {
            int pending = 2;
            void OnOneCleared()
            {
                pending--;
                if (pending > 0)
                    return;

                var resolution = _round.ResolvePendingMatch();
                if (resolution == MatchResolution.FullReshuffle)
                {
                    RefreshAll();
                    for (int r = 0; r < GridConstants.Rows; r++)
                        for (int c = 0; c < GridConstants.Cols; c++)
                            _tiles[r, c].PlayDropIn();
                }
                else
                {
                    _tiles[a.Row, a.Col].SetValue(_round.Grid[a]);
                    _tiles[a.Row, a.Col].PlayDropIn();
                    _tiles[b.Row, b.Col].SetValue(_round.Grid[b]);
                    _tiles[b.Row, b.Col].PlayDropIn();
                }
            }

            _tiles[a.Row, a.Col].PlayClear(OnOneCleared);
            _tiles[b.Row, b.Col].PlayClear(OnOneCleared);
        }
    }
}
