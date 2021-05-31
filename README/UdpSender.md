# UdpSender
UDP送信器クラスです。

## プロパティ
|  名前  |  説明  |
| ---- | ---- |
| LocalAddress |  自分のIPアドレス (読み取り専用)  |
| LocalPort | 自分のポート番号 (読み取り専用) |
| RemoteAddress |  相手のIPアドレス (読み取り専用)  |
| RemotePort |  相手のポート番号 (読み取り専用)  |
| IsOpen |  ソケットを開いているか？ (読み取り専用)  |

## フィールド
|  名前  |  説明  |
| ---- | ---- |
| FixedLocalPort | 自分の固定ポート番号 |

## メソッド
|  名前  |  説明  |
| ---- | ---- |
|  Open(address, port)  | ソケットを開きます。(送信器を開始します。)<br>address: 自分のIPアドレスまたはホスト名<br>port: 自分のポート番号<br>戻り値: 成否 |
|  Close()  |  ソケットを閉じます。(送信器を停止します。) |
|  Send(byte[] data) | バイト列データを送信します。 |
|  Send(string stringData) | 文字列データを送信します。 |
|  Send(PacketPayload packet) | パケットデータを送信します。 |
