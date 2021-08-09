# シンプルモーショントラッカー(SimpleMotionTraker)
カメラでの顏認識によりヘッドトラッキングを行う仮想トラッカーです。

[VirtualMotionCapture](https://github.com/sh-akira/VirtualMotionCapture)に
[VMCProtocol](https://github.com/sh-akira/VirtualMotionCaptureProtocol)で仮想トラッカー情報を送ることで、
カメラのみのでアバターの操作を可能にします。

# ダウンロード
[SimpleMotionTracker公式サイト](https://yuki-natsuno-vt.github.io/SimpleMotionTraker/)のダウンロードページを確認してください。  

# 更新履歴
Ver 0.5  
・入力デバイスによる指のコントロールを追加

Ver 0.4  
・顏検出精度向上

Ver 0.3  
・動体検知での簡易ハンドトラッキングを追加
・VRプレイエリアとのオフセット指定を追加
・フェイストラッキング、アイトラッキングの精度向上
・処理負荷の改善

Ver 0.2  
・影響倍率の軸別設定を追加  
・アイトラッキングを追加  
・自動調整の発動待ち時間を追加  
・ミラー（左右反転）を追加  
・設定をファイルに保存/読み込み機能を追加  

Ver 0.1  
・α版リリース

# ビルド手順
環境  
・Unity 2019.4.14f (64-bit)   
・Windows10  
Assets直下に以下のパッケージをインポートしてください。  
・VRM([UniVRM-0.57.1_60a9.unitypackage](https://github.com/vrm-c/UniVRM/releases/tag/v0.57.1))  
・uOSC([uOSC-v0.0.2.unitypackage](https://github.com/hecomi/uOSC/releases/tag/v0.0.2))  
ShapeDXのインポート  
・Assets/Script/ynv/Input.cs でエラーが出るのでVisualStudioで開く。  
・VisualStudio メニュー → ツール → NuGetパッケージマネージャ → ソリューションのNuGetパッケージの管理 → 「このソリューションに一部のNuGetパッケージが見つかりません。クリックするとオンラインパッケージソースから復元します。」の[復元]をクリック。  
・Packages/SharpDX.4.2.0 と Packages/SharpDX.DirectInput.4.2.0 内の lib/net45/*.dll を Assets/Plagins にコピーする。  
ランドマークデータの準備  
・[Dlib FaceLandmark Detector](https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314) に含まれる sp_human_face_68_for_mobile.dat を Assets/StreamingAssets に配置  
## exeの出力
Unityの File -> Build SettingsからBuildを行ってください。  
出力したexeと同じ階層に data フォルダをコピーして配置してください。

# FAQ
Q. 無料で利用ですか？商用利用可能ですか？作者の表記は必要ですか？  
A. 無料で利用できます。商用利用もOKです。表記は特に必要ないですが、宣伝してもらえると嬉しいです。[Twitter](https://twitter.com/yuki_natsuno_vt)

Q. 改変、再配布してもいいですか？  
A. 3rd party licenseを含めライセンス違反にならない場合において再配布可能です。
