using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net; // IPAddress, IPEndPoint
using System.Net.Sockets; //TcpListener, NetworkStream
using System.Threading; // Thread

namespace IpSocketToolBar
{
    public class TcpServerTrx
    {
        #region イベント

        /// <summary>
        /// パケットを受信したとき
        /// </summary>
        public event EventHandler Received = null;

        /// <summary>
        /// クライアント側から切断されたとき
        /// </summary>
        public event EventHandler Disconnected = null;

        #endregion

        #region 公開プロパティ/フィールド

        /// <summary>
        /// 自分のIPアドレス (読み取り専用)
        /// </summary>
        public IPAddress LocalAddress { private set; get; }
        /// <summary>
        /// 自分のポート番号 (読み取り専用)
        /// </summary>
        public int LocalPort { private set; get; }
        /// <summary>
        /// 相手のIPアドレス (読み取り専用)
        /// </summary>
        public IPAddress RemoteAddress { private set; get; }
        /// <summary>
        /// 相手のポート番号 (読み取り専用)
        /// </summary>
        public int RemotePort { private set; get; }

        /// <summary>
        /// 送受信タイムアウト時間[ミリ秒]
        /// </summary>
        public int TimeOut = Timeout.Infinite;
        /// <summary>
        /// TTL(最大転送回数) 255までの整数
        /// </summary>
        public int TTL = 255;

        #endregion

        #region 内部フィールド

        // サーバのスレッド
        Thread ServerThread;
        bool ServerThreadQuit = false;

        // TCPリスナ(自分), TCPクライアント(相手), ストリーム
        TcpListener listener = null;
        TcpClient client = null;
        NetworkStream networkStream = null;

        // 受信パケットのキュー
        readonly Queue<byte[]> receivedPackets = new Queue<byte[]>();

        #endregion

        #region 公開メソッド

        /// <summary>
        /// 接続待ち受け開始
        /// </summary>
        /// <param name="address">自分のIPアドレスまたはホスト名("localhost"など)</param>
        /// <param name="port">自分のポート番号</param>
        public void Start(string address, int port)
        {
            IPAddress ipAddress;

            try {
                // IPアドレスの解釈
                ipAddress = IPAddress.Parse(address);
            } catch {
                // ホスト名からIPアドレスを取得
                ipAddress = Dns.GetHostEntry(address).AddressList[0];
            }
            Start(ipAddress, port);
        }

        /// <summary>
        /// 接続待ち受け開始
        /// </summary>
        /// <param name="address">自分のIPアドレス</param>
        /// <param name="port">自分のポート番号</param>
        public void Start(IPAddress address, int port)
        {
            // TcpListenerオブジェクトを生成し、待ち受けを開始
            listener = new TcpListener(address, port);
            listener.Server.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.ReuseAddress, true); // ソケット再利用許可
            listener.Server.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.IpTimeToLive, this.TTL); // TTL(最大転送回数)
            listener.Start();

            // 自分のIPアドレスとポート番号
            LocalAddress = address;
            LocalPort = port;
            //LocalAddress = ((System.Net.IPEndPoint)listener.LocalEndpoint).Address;
            //LocalPort    = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            Console.WriteLine("待ち受け開始 ({0}:{1})", address, port);

            // サーバのスレッドを開始
            ServerThreadQuit = false;
            ServerThread = new Thread(new ThreadStart(ServerThreadFunc));
            ServerThread.Start();
        }

        /// <summary>
        /// 接続待ち受け停止
        /// </summary>
        public void Stop()
        {
            // 接続があれば閉じる
            this.Close();

            // TCPリスナーを停止する
            listener.Stop();

            // サーボスレッド停止
            ServerThreadQuit = true;
            ServerThread.Join();
        }

        /// <summary>
        /// 接続をこちらから閉じる
        /// </summary>
        public void Close()
        {
            //閉じる
            if (networkStream != null) {
                networkStream.Close();
                networkStream = null;
            }
            if (client != null) {
                client.Close();
                client = null;
            }
        }

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="data">バイト列データ</param>
        public void Send(byte[] data)
        {
            if(networkStream != null)
            {
                // 非同期で送信開始
                _ = networkStream.WriteAsync(data, 0, data.Length);
            }
        }
        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="data">文字列データ</param>
        public void Send(string stringData)
        {
            byte[] data = Encoding.ASCII.GetBytes(stringData);
            Send(data);
        }
        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="packet">パケットのペイロード</param>
        public void Send(PacketPayload packet)
        {
            Send(packet.Data);
        }

        /// <summary>
        /// 受信したバイト列データを取得する
        /// </summary>
        /// <returns>バイト列データ</returns>
        public byte[] GetBytes()
        {
            byte[] data = receivedPackets.Dequeue();
            return data;
        }
        /// <summary>
        /// 受信した文字列データを取得する
        /// </summary>
        /// <returns>文字列データ</returns>
        public string GetString()
        {
            byte[] data = receivedPackets.Dequeue();
            string str = Encoding.ASCII.GetString(data);
            return str;
        }
        /// <summary>
        /// 受信したパケットのペイロードを取得する
        /// </summary>
        /// <returns>パケットのペイロード</returns>
        public PacketPayload GetPacket()
        {
            byte[] data = receivedPackets.Dequeue();
            PacketPayload packet = new PacketPayload(data);
            return packet;
        }

        #endregion

        #region 内部メソッド

        // サーバのスレッド関数
        private void ServerThreadFunc()
        {
            while (!ServerThreadQuit)
            {
                try
                {
                    // クライアントからの接続を待ち受ける
                    // ※ ブロッキング処理だが、this.Stop()すれば例外発生して抜ける
                    client = listener.AcceptTcpClient();

                    // クライアントのIPアドレスとポート番号を取得
                    RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                    Console.WriteLine("クライアント接続({0}:{1})", RemoteAddress, RemotePort);

                    // クライアントのストリームを取得し、タイムアウト時間を設定
                    networkStream = client.GetStream();
                    networkStream.ReadTimeout = this.TimeOut;
                    networkStream.WriteTimeout = this.TimeOut;

                    // クライアントから送られたデータを受信する
                    receivedPackets.Clear();
                    byte[] data = new byte[1500];
                    int size = 0;
                    while(true)
                    {
                        // データを受信する
                        // ※ ブロッキング処理だが、1パケット受信するごとに抜ける
                        //    指定のlengthぶんデータがたまるのを待つわけではない
                        //    何もパケットが来なければタイムアウトまで待つ
                        // TODO this.Close()で抜けるか要確認
                        size = networkStream.Read(data, 0, data.Length);

                        // Readが0を返したらクライアント側からの切断と判断。
                        if (size == 0)
                        {
                            Console.WriteLine("クライアント切断"); // TODO
                            if (Disconnected != null) {
                                Disconnected(this, EventArgs.Empty);
                            }
                            this.Close();
                            break;
                        }
                        // 受信データあり。イベント発生
                        else if(Received != null)
                        {
                            receivedPackets.Enqueue(data);
                            Received(this, EventArgs.Empty);
                        }
                    }//while(true)
                }
                catch
                {
                    if (!ServerThreadQuit)
                    {
                        Console.WriteLine("サーバスレッドで予期しない例外");
                        this.Stop();
                    }
                }
            }// while (!ServerThreadQuit)
        }

        #endregion
    }
}
