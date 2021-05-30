# UdpReceiverToolStrip
UDP受信器のツールバーです。ToolStripクラスを継承しています。

![図](udp_receiver.png)

## イベント
|  名前  |  説明  |
| ---- | ---- |
|  Opened  |  ソケットを開いたとき  |
|  Closed  |  ソケットを閉じたとき  |
|  Received  | パケットを受信した時  |

## プロパティ
|  名前  |  説明  |
| ---- | ---- |
|  Scoket  |  ソケット (UdpReceiverSocketクラスのオブジェクト)  |

## メソッド
|  名前  |  説明  |
| ---- | ---- |
|  Begin(iniFileName, section, port)  |  初期化処理。<br> フォームの開始時(Loadイベント)に呼んでください。<br> iniFileName: 設定INIファイルのパス(省略可)<br>section: 設定INIファイルのセクション名(省略可)<br>port: 既定のポート番号(省略可) |
|  End()  |  終了処理。<br> フォームの終了時(FormClosingイベント)に呼んでください。 |
|  Open()  |  ソケットを開きます。開始ボタンを押すのと同じ動作です。 |
|  Close()  |  ソケットを閉じます。停止ボタンを押すのと同じ動作です。 |
