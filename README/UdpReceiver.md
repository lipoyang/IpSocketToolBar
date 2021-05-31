# UdpReceiver
UDP受信器クラスです。

## イベント
|  名前  |  説明  |
| ---- | ---- |
|  Received  |  パケットを受信したとき  |

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
| PollingInterval |  受信ポーリング周期[ミリ秒]  |

## メソッド
|  名前  |  説明  |
| ---- | ---- |
|  Open(address, port)  | ソケットを開きます。(受信器を開始します。)<br>address: 自分のIPアドレスまたはホスト名<br>port: 自分のポート番号<br>戻り値: 成否 |
|  Close()  |  ソケットを閉じます。(受信器を停止します。) |
|  GetBytes() | 受信したバイト列データを取得します。 |
|  GetString() | 受信した文字列データを取得します。 |
|  GetPacket() | 受信したパケットデータを取得します。 |
|  WaitBytes() | バイト列データの受信を待ちます。 |
|  WaitString() | 文字列データの受信を待ちます。 |
|  WaitPacket() | パケットデータの受信を待ちます。 |

## 注意点
非同期処理APIと同期処理APIの利用は排他です。

### 非同期処理API
* Receivedイベントにハンドラを登録します。
* パケット受信があるとReceivedイベントが発生します。
* GetBytes()またはGetString()またはGetPacket()で受信データを取得します。

### 同期処理API
* WaitBytes()またはWaitString()またはWaitPacket()でパケット受信を待って受信データを取得します。
