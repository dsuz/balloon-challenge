public static class Consts
{
    // Photon のイベントコード定義
    // 1, 2 が PunTurnManager で使われているので 10 から始めている。

    /// <summary>バルーンの許容用をセットする</summary>
    public const byte SetCapacity = 10;
    /// <summary>バルーンに空気を送る</summary>
    public const byte Pump = 11;
    /// <summary>バルーンが割れた</summary>
    public const byte Crack = 12;
}
