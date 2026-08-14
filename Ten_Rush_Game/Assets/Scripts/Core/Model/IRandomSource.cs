namespace TenRush.Core.Model
{
    /// <summary>
    /// 타일 값 생성원. 실제 게임은 <see cref="SystemRandomSource"/>를 쓰고,
    /// 테스트는 정해진 값을 순서대로 내보내는 가짜 구현으로 갈아 끼운다.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>1~9(MinTileValue~MaxTileValue) 범위의 새 타일 값 하나를 반환한다.</summary>
        int NextTileValue();
    }
}
