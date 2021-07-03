using System.Collections;   // コルーチンを使うため
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// パワーゲージを制御するコンポーネント
/// 適当なオブジェクトにアタッチして使う。
/// </summary>
public class PowerGaugeController : MonoBehaviour
{
    /// <summary>ゲーム マネージャー</summary>
    [SerializeField] GameManager m_gm = default;
    /// <summary>パワーゲージとなる Slider</summary>
    [SerializeField] Slider m_powerGauge = default;
    /// <summary>ゲージが上下する速度</summary>
    [SerializeField] float m_gaugeSpeed = 3;
    Coroutine m_coroutine = default;

    /// <summary>
    /// ゲージを動かす／止める時に呼ぶ。
    /// ゲージが動いていない時に呼ばれたら、ゲージを動かす。
    /// ゲージが動いている時に呼ばれたら、ゲージを止めて空気を送り込む。
    /// 空気を送り込む量は Slider.value になるため、送り込む量を調節したい場合は Slider の Min Value/Max Value で調整する。
    /// </summary>
    public void StartAndStopGauge()
    {
        if (m_coroutine == null)
        {
            m_coroutine = StartCoroutine(PingPongGauge());
        }
        else
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
            Debug.Log($"Pump value: {m_powerGauge.value}");
            m_gm.Pump(m_powerGauge.value);
        }
    }

    /// <summary>
    /// ゲージを上下させる。
    /// </summary>
    /// <returns></returns>
    IEnumerator PingPongGauge()
    {
        float timer = 0;

        while (true)
        {
            m_powerGauge.value = Mathf.PingPong(m_gaugeSpeed * timer, m_powerGauge.maxValue);
            timer += Time.deltaTime;    // 放置しておくといずれオーバーフローする。「制限時間を設けて強制的に押した事にする」機能を後で加えることになるだろうからこのままにしておく。
            yield return new WaitForEndOfFrame();
        }
    }
}
