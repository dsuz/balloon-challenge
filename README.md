# Summary/Rule

Photon を使ったターン制ネットワークゲーム（二人プレイ）のサンプルです。ルールは以下の通りです。

1. プレイヤーは交互に風船に空気を送り込む
2. 風船を割ってしまったプレイヤーの負け

ゲームの流れは以下の通りです。

1. 風船に空気を送り込む順番が周ってきたプレイヤーにはそのためのボタンとゲージが表示される
2. ボタンを一回押すとゲージが上下する
3. もう一度ボタンを押すと、ゲージの位置に応じた量の空気が風船に送り込まれる
   - ゲージの位置が上であるほど多くの空気が送り込まれる
4. 風船が割れたらゲーム終了
5. 風船が割れなければ他のプレイヤーに順番が移る

# Requirement

動作させるには、以下のアセットをプロジェクトに追加でインポートする必要がある。

1. [PUN 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)
2. [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)

# Notes

![](https://user-images.githubusercontent.com/4126881/124361436-38a65100-dc6a-11eb-885c-73e9a114140e.JPEG)