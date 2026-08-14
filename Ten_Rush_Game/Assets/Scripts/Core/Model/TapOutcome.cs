namespace TenRush.Core.Model
{
    /// <summary>한 번의 탭(<see cref="GameRound.Tap"/> 호출)이 만들어낸 결과 종류.</summary>
    public enum TapOutcome
    {
        /// <summary>게임오버 상태이거나, 매치 애니메이션 처리 중(Locked)이라 무시됨.</summary>
        Ignored,

        /// <summary>선택된 타일이 없던 상태에서 첫 타일을 선택함.</summary>
        Selected,

        /// <summary>이미 선택돼 있던 타일을 다시 탭해 선택 해제함.</summary>
        Deselected,

        /// <summary>두 타일의 합이 10이라 매치 성공. 실제 값 교체는 <see cref="GameRound.ResolvePendingMatch"/>에서 일어난다.</summary>
        Matched,

        /// <summary>두 타일의 합이 10이 아니라 실패. 방금 탭한 타일이 새 선택으로 이어진다.</summary>
        Mismatched
    }
}
