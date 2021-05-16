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
        Thread ServerThread;
        bool ServerThreadQuit = false;

        // TCPリスナ
        TcpListener listener = null;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// クライアントからの接続待ち受けを開始する
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
        /// クライアントからの接続待ち受けを開始する
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
            //LocalAddress = address.ToString();
            //LocalPort = port;
            LocalAddress = ((System.Net.IPEndPoint)listener.LocalEndpoint).Address.ToString();
            LocalPort    = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            Console.WriteLine("待ち受け開始 ({0}:{1})", LocalAddress, LocalPort);

            // サーバのスレッドを開始
            ServerThreadQuit = false;
            ServerThread = new Thread(new ThreadStart(ServerThreadFunc));
            ServerThread.Start();
        }

        /// <summary>
        /// クライアントからの接続待ち受けを停止する
        /// </summary>
        public void Stop()
        {
            // 接続があれば閉じる
            this.Close();

            // TCPリスナーを停止する
            listener.Stop();

            // サーバのスレッドを停止
            ServerThreadQuit = true;
            ServerThread.Join();
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
                    RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                    Console.WriteLine("クライアント接続({0}:{1})", RemoteAddress, RemotePort);

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
