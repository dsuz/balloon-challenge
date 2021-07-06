using System;   // Array.ForEach を使うため
using UnityEngine;
using DG.Tweening;  // DOTween を使うため
// Photon 用の名前空間を参照する
using Photon.Pun;
using Photon.Pun.UtilityScripts;    // PunTurnManager, IPunTurnManagerCallbacks を使うため
using Photon.Realtime;

/// <summary>
/// ゲームを管理するコンポーネント。
/// 適当なオブジェクトにアタッチして使う。
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    /// <summary>ランダムで決定されるキャパシティの最大値</summary>
    [SerializeField] float m_maxCapacity = 120;
    /// <summary>ランダムで決定されるキャパシティの最小値</summary>
    [SerializeField] float m_minCapacity = 100;
    /// <summary>バルーン</summary>
    [SerializeField] BalloonController m_balloon = default;
    /// <summary>BGM を鳴らす AudioSource</summary>
    [SerializeField] AudioSource m_bgm = default;
    /// <summary>危険になると BGM の Pitch を上げていくが、その最大値</summary>
    [SerializeField] float m_maxBgmPitch = 2;
    /// <summary>PunTurnManager コンポーネントを指定する</summary>
    [SerializeField] PunTurnManager m_turnManager = default;
    /// <summary>自分が操作する時にのみ表示する GameObject</summary>
    [SerializeField] GameObject[] m_myControllerObjects = default;
    /// <summary>ランダムで決定されたキャパシティ</summary>
    float m_capacity;
    /// <summary>現在吹き込まれた空気の量</summary>
    float m_blowAmount;
    /// <summary>現在順番が周ってきているプレイヤーが何番目のプレイヤーなのか示す index</summary>
    int m_activePlayerIndex = 0;

    /// <summary>
    /// 現在順番が周ってきているプレイヤーの Player オブジェクト
    /// </summary>
    Player ActivePlayer
    {
        get { return PhotonNetwork.PlayerList[m_activePlayerIndex]; }
    }

    void Start()
    {
        // 最初は操作パネルを消しておく
        Array.ForEach(m_myControllerObjects, e => e.SetActive(false));
    }

    public void Listen()
    {
        // ターン管理のイベントを Listen する
        m_turnManager.TurnManagerListener = this;

        // MasterClient 側で Balloon の Capacity を決める
        if (PhotonNetwork.IsMasterClient)
        {
            InitCapacity();
        }
    }

    /// <summary>
    /// 初期化（キャパシティの決定を）する。Master クライアントから呼ばれ、決定したキャパシティを通知する。
    /// </summary>
    void InitCapacity()
    {
        float capacity = UnityEngine.Random.Range(m_minCapacity, m_maxCapacity);
        // 決定した風船の容量を SendMove で全員に通知する
        MoveData moveData = new MoveData(MoveType.SetCapacity, capacity);
        m_turnManager.SendMove(JsonUtility.ToJson(moveData) , false);
    }

    /// <summary>
    /// ゲームを初期化して許容量をセットし、空気の量をゼロにする
    /// </summary>
    /// <param name="capacity"></param>
    void InitGame(float capacity)
    {
        m_capacity = capacity;
        Debug.Log($"Capacity: {m_capacity}");
        m_blowAmount = 0;
    }

    /// <summary>
    /// 空気を送り込む
    /// </summary>
    /// <param name="blow">送り込む空気の量</param>
    public void Pump(float blow)
    {
        // 割れたかどうか判定する
        if (m_blowAmount + blow > m_capacity)
        {
            Debug.Log("Baloon cracked.");
            // 割れたら操作パネルを消す
            Array.ForEach(m_myControllerObjects, e => e.SetActive(false));
            // 割ってしまったことを通知する
            MoveData moveData = new MoveData(MoveType.Crack, blow);
            m_turnManager.SendMove(JsonUtility.ToJson(moveData), false);    // 自分の番は Finish しない（ゲームは終わりで、それをしなくていいから）
        }
        else
        {
            Debug.Log("Baloon is safe. Move to next player.");
            // 割れなかったら次のプレイヤーに操作を移す (move)
            MoveData moveData = new MoveData(MoveType.Blow, blow);
            m_turnManager.SendMove(JsonUtility.ToJson(moveData), true);
        }
    }

    /// <summary>
    /// 風船に空気を送り込む
    /// PunTurnManager からイベントを受け取って呼ばれる
    /// </summary>
    /// <param name="blow">送り込む空気の量</param>
    void OnPump(float blow)
    {
        m_blowAmount += blow;
        float capacityRatio = m_blowAmount / m_capacity;
        Debug.Log($"Pumped. Current / Max: {m_blowAmount} / {m_capacity}, {(int)(capacityRatio * 100)} %");
        m_balloon.PumpUp(capacityRatio);
        PitchBgm(capacityRatio);
    }

    /// <summary>
    /// 風船を割った時に呼ばれる
    /// </summary>
    /// <param name="playerActorNumber">風船を割ったプレイヤーの ActorNumber</param>
    void OnCrack(int playerActorNumber)
    {
        m_bgm.Stop();
        m_balloon.Crack();
        Debug.Log($"Player {playerActorNumber} cracked balloon.");
    }

    /// <summary>
    /// Move/Finish で送られてくる情報を処理する
    /// </summary>
    /// <param name="moveData">送られてきたデータ</param>
    /// <param name="playerActorNumber">データを送ってきたプレイヤーの ActorNumber</param>
    void OnMoveOrFinish(MoveData moveData, int playerActorNumber)
    {
        switch (moveData.MoveType)
        {
            case MoveType.SetCapacity:
                InitGame(moveData.Value);
                break;
            case MoveType.Blow:
                OnPump(moveData.Value);
                break;
            case MoveType.Crack:
                OnCrack(playerActorNumber);
                break;
            default:
                Debug.LogError($"Unknown MoveType: {moveData.MoveType.ToString()}");
                break;
        }
    }

    /// <summary>
    /// BGM の pitch を操作する
    /// </summary>
    /// <param name="capacityRatio">許容量の何%かを0~1で指定する。（例: 0 = 0%, 1 = 100%）</param>
    void PitchBgm(float capacityRatio)
    {
        float pitch = capacityRatio * m_maxBgmPitch;
        pitch = Mathf.Max(pitch, 1);
        m_bgm.DOPitch(pitch, 1); // duration は適当に設定した（ハードコードするのはよろしくない）
    }

    /// <summary>
    /// アクティブなプレイヤーを判断するため、内部的にインデックスを操作する
    /// </summary>
    void MoveToNextPlayer()
    {
        m_activePlayerIndex = (m_activePlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        Debug.Log($"Active player changed to {PhotonNetwork.PlayerList[m_activePlayerIndex].ActorNumber}");
    }

    #region IPunTurnManagerCallbacks の実装
    void IPunTurnManagerCallbacks.OnTurnBegins(int turn)
    {
        Debug.Log($"Enter OnTurnBegins. turn: {turn}");
        m_activePlayerIndex = 0;    // 最初のプレイヤーからターンを始める
        bool controllerVisibleFlag = false;

        // 自分の番なら操作パネルを表示する
        if (this.ActivePlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            Debug.Log("This is my turn.");
            controllerVisibleFlag = true;
        }

        Array.ForEach(m_myControllerObjects, e => e.SetActive(controllerVisibleFlag));
    }

    void IPunTurnManagerCallbacks.OnPlayerMove(Player player, int turn, object move)
    {
        string json = (string)move;
        Debug.Log($"Enter OnPlayerMove. player: {player.ActorNumber}, turn: {turn}, move: {json}");
        OnMoveOrFinish(JsonUtility.FromJson<MoveData>(json), player.ActorNumber);
    }

    void IPunTurnManagerCallbacks.OnPlayerFinished(Player player, int turn, object move)
    {
        string json = (string)move;
        Debug.Log($"Enter OnPlayerFinished. player: {player.ActorNumber}, turn: {turn}, move: {json}");
        OnMoveOrFinish(JsonUtility.FromJson<MoveData>(json), player.ActorNumber);
        MoveToNextPlayer();

        // 全員が終わっている場合は何もせず、続きの処理は OnTurnCompleted に任せる。まだ順番を終わらせていないプレイヤーがいる場合は、順番が周ってきているプレイヤーに操作をさせる
        if (!m_turnManager.IsCompletedByAll)
        {
            // 自分の番なら操作パネルを出す
            if (this.ActivePlayer.Equals(PhotonNetwork.LocalPlayer))
            {
                Debug.Log("This is my turn.");
                Array.ForEach(m_myControllerObjects, e => e.SetActive(true));
            }
            else
            {
                Debug.Log("Not my turn.");
                Array.ForEach(m_myControllerObjects, e => e.SetActive(false));
            }
        }
    }

    void IPunTurnManagerCallbacks.OnTurnCompleted(int turn)
    {
        Debug.Log($"Enter OnTurnCompleted. turn: {turn}");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("I am MasterClient. I begin turn.");
            m_turnManager.BeginTurn();
        }
    }

    void IPunTurnManagerCallbacks.OnTurnTimeEnds(int turn)
    {
        Debug.Log($"Enter OnTurnTimeEnds. turn: {turn}");
    }
    #endregion
}
