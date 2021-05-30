# TcpClientSocket
TCPクライアントの送受信器クラスです。

## イベント
|  名前  |  説明  |
| ---- | ---- |
|  Connected  |  サーバと接続したとき  |
|  Disconnected  |  サーバから切断したとき  |
|  Received  |  パケットを受信したとき  |

## プロパティ
|  名前  |  説明  |
| ---- | ---- |
| LocalAddress |  自分のIPアドレス (読み取り専用)  |
| LocalPort | 自分のポート番号 (読み取り専用) |
| RemoteAddress |  相手のIPアドレス (読み取り専用)  |
| RemotePort |  相手のポート番号 (読み取り専用)  |
| IsOpen |  ソケットを開いているか？ (読み取り専用)  |
| IsConnected |  サーバと接続されているか？ (読み取り専用)  |
| DisconnectReason |  切断した原因 (読み取り専用)  |

## フィールド
|  名前  |  説明  |
| ---- | ---- |
| ReadTimeout |  受信タイムアウト時間[ミリ秒]  |
| WriteTimeout |  送信タイムアウト時間[ミリ秒]  |
| PollingInterval |  受信ポーリング周期[ミリ秒]  |
| FixedLocalPort | 自分の固定ポート番号 |

## メソッド
|  名前  |  説明  |
| ---- | ---- |
|  Open(address, port)  | ソケットを開きます。(クライアントを開始し、サーバへの接続試行をはじめます。)<br>address: 自分のIPアドレスまたはホスト名<br>port: 自分のポート番号<br>戻り値: 成否 |
|  Close()  |  ソケットを閉じます。(サーバとの接続を切断し、クライアントを停止します。) |
|  Send(byte[] data) | バイト列データを送信します。 |
|  Send(string stringData) | 文字列データを送信します。 |
|  Send(PacketPayload packet) | パケットを送信します。 |
|  GetBytes() | 受信したバイト列データを取得します。 |
|  GetString() | 受信した文字列データを取得します。 |
|  GetPacket() | 受信したパケットのペイロードを取得します。 |
|  WaitBytes() | バイト列データの受信を待ちます。 |
|  WaitString() | 文字列データの受信を待ちます。 |
|  WaitPacket() | パケットの受信を待ちます。 |

## 注意点
非同期処理APIと同期処理APIの利用は排他です。

### 非同期処理API
* Receivedイベントにハンドラを登録します。
* パケット受信があるとReceivedイベントが発生します。
* GetBytes()またはGetString()またはGetPacket()で受信データを取得します。

### 同期処理API
* WaitBytes()またはWaitString()またはWaitPacket()でパケット受信を待って受信データを取得します。
