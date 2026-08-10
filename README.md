# Roguelike FPS

## ゲーム概要

Unity / C#で開発している3DローグライクFPSです。
「Enter the Gungeon」「Slay the Spire」から着想を得て、ランダムに選ばれた部屋を攻略しながら、武器やパッシブアイテムを組み合わせてビルドを構築し、ボス撃破を目指します。

現在も個人で開発を続けています。

## 開発環境

* Unity
* C#

## プレイ動画

https://github.com/user-attachments/assets/33667362-4dbf-4551-a228-3964d535dbbf

## 技術的に力を入れた点

### 1. ScriptableObjectを活用した拡張可能な銃器設計

射撃方法、射撃モード、リロードアニメーションなどをScriptableObjectとして分離しています。

それぞれを組み合わせることで、GunBase本体を大きく変更することなく、異なる特徴を持つ武器を追加できる設計にしています。

### 2. データ駆動・イベント駆動型のパッシブシステム

射撃、リロード、命中、敵撃破、被弾など、ゲーム中のさまざまなタイミングでイベントフックを呼び出しています。

パッシブアイテムは共通のPassiveEffectを継承し、必要なイベントだけをオーバーライドすることで、それぞれ異なる効果を実装しています。

また、パッシブによるステータス変更はPlayerStatsで管理し、固定値と倍率を分けて扱っています。

## 主要なソースコード

* [GunBase.cs](Assets/_Project/Scripts/Weapons/Gunbase.cs)
  武器の基底処理
* [PassiveEffect.cs](Assets/_Project/Scripts/passive/PassiveEffect.cs)
  パッシブ効果の基底クラス
* [PlayerStats.cs](Assets/_Project/Scripts/Player/stats.cs)
  プレイヤーのステータス管理
* [PlayerManager.cs](Assets/_Project/Scripts/Player/PlayerManager.cs)
  各システムの連携管理
