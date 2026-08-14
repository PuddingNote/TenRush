namespace TenRush.Core.Model
{
    /// <summary><see cref="GameRound.ResolvePendingMatch"/>가 보드를 어떻게 갱신했는지.</summary>
    public enum MatchResolution
    {
        /// <summary>처리할 대기 중인 매치가 없었음(비정상 호출).</summary>
        NoOp,

        /// <summary>매치된 두 칸만 새 값으로 교체됨.</summary>
        TilesReplaced,

        /// <summary>교체 후 유효 페어가 하나도 없어 보드 전체를 리셔플함.</summary>
        FullReshuffle
    }
}
