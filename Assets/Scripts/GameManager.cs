using UnityEngine;

/// <summary>
/// ゲームを管理するコンポーネント。
/// 適当なオブジェクトにアタッチして使う。
/// </summary>
public class GameManager : MonoBehaviour
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
    /// <summary>ランダムで決定されたキャパシティ</summary>
    float m_capacity;
    /// <summary>現在吹き込まれた空気の量</summary>
    float m_blowAmount;

    private void Start()
    {
        Init();
    }

    /// <summary>
    /// 初期化する。
    /// </summary>
    public void Init()
    {
        m_capacity = Random.Range(m_minCapacity, m_maxCapacity);
        Debug.Log($"Capacity: {m_capacity}");
        m_blowAmount = 0;
    }

    /// <summary>
    /// 空気を送り込む
    /// </summary>
    /// <param name="blow">送り込む空気の量</param>
    public void Pump(float blow)
    {
        m_blowAmount += blow;
        float capacityRatio = m_blowAmount / m_capacity;
        Debug.Log($"Current / Max: {m_blowAmount} / {m_capacity}, {(int)(capacityRatio * 100)} %");
        m_balloon.PumpUp(capacityRatio);
        PitchBgm(capacityRatio);

        // 割れたかどうか判定する
        if (m_blowAmount > m_capacity)
        {
            m_bgm.Stop();
            m_balloon.Crack();
        }
    }

    /// <summary>
    /// BGM の pitch を操作する
    /// </summary>
    /// <param name="capacityRatio">許容量の何%かを0~1で指定する。（例: 0 = 0%, 1 = 100%）</param>
    void PitchBgm(float capacityRatio)
    {
        float pitch = capacityRatio * m_maxBgmPitch;
        pitch = Mathf.Max(pitch, 1f);
        m_bgm.pitch = pitch;
    }
}
