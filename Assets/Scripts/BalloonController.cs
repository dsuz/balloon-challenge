using UnityEngine;
using DG.Tweening;

/// <summary>
/// バルーンの動きを制御するコンポーネント。
/// バルーン（のスプライトかモデル）にアタッチして使う。
/// アニメーションに DOTween を使っている。
/// </summary>
public class BalloonController : MonoBehaviour
{
    /// <summary>バルーンが割れた時に生成するエフェクト</summary>
    [SerializeField] GameObject m_crackEffectPrefab = default;
    /// <summary>バルーンが膨らんだ時の最大 Scale</summary>
    [SerializeField] float m_maxScale = 3;

    /// <summary>
    /// 空気を送り込む。空気を送り込まれたら膨らんで揺れ出す。
    /// 引数には「何%の許容用にするか」を直接指定する。
    /// </summary>
    /// <param name="capacityRatio">許容量の何%かを0~1で指定する。（例: 0 = 0%, 1 = 100%）</param>
    public void PumpUp(float capacityRatio)
    {
        // 膨らませる
        float scale = capacityRatio * m_maxScale;
        scale = Mathf.Max(scale, 1f);
        this.transform.DOScale(scale, 0.5f).SetLink(this.gameObject);
        // 揺らす
        Shake(capacityRatio);
    }

    /// <summary>
    /// バルーンを揺らすアニメーションを再生する。
    /// 引数は PumpUp と同じ。
    /// </summary>
    /// <param name="capacityRatio"></param>
    void Shake(float capacityRatio)
    {
        // ここの数値設定は適当かつハードコードされているので、後で適切に直す。
        int strength = Mathf.RoundToInt(10 * capacityRatio);
        this.transform.DOShakePosition(10, strength: 0.1f, vibrato: strength, fadeOut: false).SetLoops(-1).SetLink(this.gameObject);
        this.transform.DOShakeRotation(10, strength: strength, vibrato: strength, fadeOut: false).SetLoops(-1).SetLink(this.gameObject);
    }

    /// <summary>
    /// バルーンを割る時に呼ぶ。
    /// </summary>
    public void Crack()
    {
        Instantiate(m_crackEffectPrefab);
        Destroy(this.gameObject);
    }
}
