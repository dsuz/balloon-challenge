using System;

/// <summary>
/// PunTurnManager.SendMove() で送るデータを定義する構造体
/// この構造体を JSON 形式にシリアライズして送ることを想定している。
/// 現時点では Value は float にしている。もっといろいろなデータを送りたいならこの構造体を直す必要がある。
/// </summary>
[Serializable]
public struct MoveData
{
    public MoveType MoveType;
    public float Value;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="moveType">何をするか</param>
    /// <param name="value">値</param>
    public MoveData(MoveType moveType, float value)
    {
        this.MoveType = moveType;
        this.Value = value;
    }
}

/// <summary>
/// PunTurnManager.SendMove() で送るデータの種類
/// 「何をするか」を定義する
/// </summary>
public enum MoveType
{
    /// <summary>風船の許容量をセットする</summary>
    SetCapacity,
    /// <summary>風船に空気を入れる</summary>
    Blow,
    /// <summary>風船を割った</summary>
    Crack,
}