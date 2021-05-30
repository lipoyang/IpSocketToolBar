# IpSocketToolBar
IPソケット通信ツールバー (Windows Forms) とソケット通信のためのクラスライブラリ

![図](README/img.png)

## クラス

|  名前  |  説明  |
| ---- | ---- |
| [TcpClientToolStrip](README/TcpClientToolStrip.md)  |  TCPクライアントのツールバーです。ToolStripクラスを継承しています。 |
| [TcpServerToolStrip](README/TcpServerToolStrip.md)  |  TCPクライアントのツールバーです。ToolStripクラスを継承しています。  |
| [UdpSenderToolStrip](README/UdpSenderToolStrip.md)  |  UDP送信器のツールバーです。ToolStripクラスを継承しています。  |
| [UdpReceiverToolStrip](README/UdpReceiverToolStrip.md)  |  UDP受信器のツールバーです。ToolStripクラスを継承しています。  |
| [StatusToolStrip](README/StatusToolStrip.md)  |  TCP通信のステータス表示のツールバーです。ToolStripクラスを継承しています。  |
| [TcpClientSocket](README/TcpClientSocket.md)  | TCPクライアントソケット |
| [TcpServerSocket](README/TcpServerSocket.md)  | TCPサーバーソケット |
| [UdpSenderSocket](README/UdpSenderSocket.md)  | UDP送信器ソケット |
| [UdpReceiverSocket](README/UdpReceiverSocket.md)  | UDP受信器ソケット |
| [PacketPayload](README/PacketPayload.md)  | IPパケットのペイロード |

※ TCP送受信はストリームではなくパケット単位で一つの電文として扱うことを想定した仕様です。

<br>

## 列挙型

|  名前  |  説明  |
| ---- | ---- |
|  AsciiCode  |  アスキー制御キャラクタコード  |
|  Endian  |  ビッグエンディアンかリトルエンディアンか  |

