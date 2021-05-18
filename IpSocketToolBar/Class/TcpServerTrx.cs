using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net; // IPAddress, IPEndPoint
using System.Net.Sockets; //TcpListener, TcpClient, NetworkStream
using System.Threading; // Thread, Timeout

namespace IpSocketToolBar
{
    /// <summary>
    /// TCPサーバのパケット送受信器
    /// </summary>
    public class TcpServerTrx : TcpTranceiver
    {
        #region イベント

        /// <summary>
        /// パケットを受信したとき
        /// </summary>
        public event EventHandler Received = null;

        /// <summary>
        /// クライアント側から接続されたとき
        /// </summary>
        public event EventHandler Connected = null;

        /// <summary>
        /// クライアント側から切断されたとき
        /// </summary>
        public event EventHandler Disconnected = null;

        #endregion

        #region 公開プロパティ/フィールド

        /// <summary>
        /// TTL(最大転送回数) 255までの整数
        /// </summary>
        public int TTL = 255;

        #endregion

        #region 内部フィールド

        // サーバのスレッド
        Thread thread;
        bool threadQuit = false;

        // TCPリスナ
        TcpListener listener = null;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TcpServerTrx()
        {
            IsOpen = false;
        }

        /// <summary>
        /// クライアントからの接続待ち受けを開始する
        /// </summary>
        /// <param name="address">自分のIPアドレスまたはホスト名("localhost"など)</param>
        /// <param name="port">自分のポート番号</param>
        public void Open(string address, int port)
        {
            IPAddress ipAddress;

            try {
                // IPアドレスの解釈
                ipAddress = IPAddress.Parse(address);
            } catch {
                // ホスト名からIPアドレスを取得
                ipAddress = Dns.GetHostEntry(address).AddressList[0];
            }
            Open(ipAddress, port);
        }

        /// <summary>
        /// クライアントからの接続待ち受けを開始する
        /// </summary>
        /// <param name="address">自分のIPアドレス</param>
        /// <param name="port">自分のポート番号</param>
        public void Open(IPAddress address, int port)
        {
            // 待ち受けを開始
            listener = new TcpListener(address, port);
            listener.Server.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.ReuseAddress, true); // ソケット再利用許可
            listener.Server.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.IpTimeToLive, this.TTL); // TTL(最大転送回数)
            listener.Start();

            // 自分のIPアドレスとポート番号
            LocalAddress = address.ToString();
            LocalPort = port;
            Console.WriteLine("待ち受け開始 ({0}:{1})", LocalAddress, LocalPort);

            // スレッドを開始
            threadQuit = false;
            thread = new Thread(new ThreadStart(threadFunc));
            thread.Start();
            IsOpen = true;
        }

        /// <summary>
        /// クライアントからの接続待ち受けを停止する
        /// </summary>
        public void Close()
        {
            threadQuit = true; // スレッド終了要求
            this.Disconnect(); // 接続があれば切断する
            listener.Stop();   // TCPリスナーを停止する
            thread.Join();     // スレッド終了待ち
            IsOpen = false;
        }

        /// <summary>
        /// 接続を切断する
        /// </summary>
        public void Disconnect()
        {
            if (networkStream != null)
            {
                networkStream.Close();
                networkStream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        #endregion

        #region 内部メソッド

        // サーバのスレッド関数
        private void threadFunc()
        {
            while (!threadQuit)
            {
                try
                {
                    // クライアントからの接続を待ち受ける
                    // ※ ブロッキング処理だが、this.Close()すれば例外発生して抜ける
                    client = listener.AcceptTcpClient();

                    // クライアントのIPアドレスとポート番号を取得
                    RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                    Console.WriteLine("クライアント接続({0}:{1})", RemoteAddress, RemotePort);

                    if (Connected != null) Connected(this, EventArgs.Empty);

                    // クライアントのストリームを取得し、タイムアウト時間を設定
                    networkStream = client.GetStream();
                    networkStream.ReadTimeout = this.AliveTimeOut;
                    networkStream.WriteTimeout = this.AliveTimeOut;

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
                        // TODO this.Disconnect()で抜けるか要確認
                        size = networkStream.Read(data, 0, data.Length);

                        // Readが0を返したらクライアント側からの切断と判断。
                        if (size == 0)
                        {
                            Console.WriteLine("クライアント切断"); // TODO
                            if (Disconnected != null) {
                                Disconnected(this, EventArgs.Empty);
                            }
                            this.Disconnect();
                            break;
                        }
                        // 受信データあり。イベント発生
                        else
                        {
                            receivedPackets.Enqueue(data);
                            if (Received != null) Received(this, EventArgs.Empty);
                        }
                    }//while(true)
                }
                catch
                {
                    if (!threadQuit)
                    {
                        Console.WriteLine("サーバスレッドで予期しない例外");
                        this.Close();
                    }
                }
            }// while (!ServerThreadQuit)
        }

        #endregion
    }
}
