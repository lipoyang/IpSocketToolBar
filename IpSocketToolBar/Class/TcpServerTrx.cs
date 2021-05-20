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
        /// クライアントと接続したとき
        /// </summary>
        public event EventHandler Connected = null;

        /// <summary>
        /// クライアントから切断したとき
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

        // TCPリスナ
        TcpListener listener = null;
        
        // デバッグ用
        const string tag = "TCPサーバ: ";

        #endregion

        #region 公開メソッド

        /// <summary>
        /// サーバを開始する
        /// (クライアントからの接続待ち受けを開始する)
        /// </summary>
        /// <param name="address">自分のIPアドレスまたはホスト名</param>
        /// <param name="port">自分のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(string address, int port)
        {
            // IPアドレスの解釈
            IPAddress ipAddress;
            if(!IPAddress.TryParse(address, out ipAddress))
            {
                try{
                    // ホスト名から(IPv4の)IPアドレスを取得
                    var list = Dns.GetHostEntry(address).AddressList;
                    ipAddress = list.First(a => a.AddressFamily == AddressFamily.InterNetwork);
                }catch{
                    return false;
                }
            }
            return Open(ipAddress, port);
        }

        /// <summary>
        /// サーバを開始する
        /// (クライアントからの接続待ち受けを開始する)
        /// </summary>
        /// <param name="address">自分のIPアドレス</param>
        /// <param name="port">自分のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(IPAddress address, int port)
        {
            try
            {
                // 自分のIPアドレスとポート番号
                LocalAddress = address.ToString();
                LocalPort = port;

                // 待ち受けを開始
                listener = new TcpListener(address, port);
                listener.Server.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.ReuseAddress, true); // ソケット再利用許可
                listener.Server.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.IpTimeToLive, this.TTL); // TTL(最大転送回数)
                listener.Start();

                Console.WriteLine(tag + "待ち受け開始 ({0}:{1})", LocalAddress, LocalPort);

                // スレッドを開始
                threadQuit = false;
                thread = new Thread(new ThreadStart(threadFunc));
                thread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// サーバを停止する
        /// </summary>
        public void Close()
        {
            Console.WriteLine(tag + "停止要求");

            threadQuit = true; // スレッド終了要求
            this.Disconnect(); // 接続があれば切断する
            listener.Stop();   // TCPリスナーを停止する
            thread.Join();     // スレッド終了待ち
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

        // スレッド関数
        private void threadFunc()
        {
            Console.WriteLine(tag + "スレッド開始");
            IsOpen = true;

            while (!threadQuit) // 接続待ちループ
            {
                // クライアントからの接続を待ち受ける
                // ※ ブロッキング処理だが、this.Close()すれば例外発生して抜ける
                try {
                    client = listener.AcceptTcpClient();
                }catch{
                    break; // this.Close()の場合
                }

                // サーバとクライアントのIPアドレスとポート番号
                LocalAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                LocalPort     = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                RemotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine(tag + "クライアント({0}:{1})と接続({2}:{3})",
                    RemoteAddress, RemotePort, LocalAddress, LocalPort);

                if (Connected != null) this.Connected(this, EventArgs.Empty); // 接続イベント発行

                // クライアントのストリームを取得し、タイムアウト時間を設定
                networkStream = client.GetStream();
                networkStream.ReadTimeout = this.ReadTimeout;
                networkStream.WriteTimeout = this.WriteTimeout;

                // データを受信する
                receivedPackets.Clear();
                byte[] data = new byte[1500];
                int size;
                while (true) // 受信待ちループ
                {
                    // データを受信する
                    // ※ ブロッキング処理だが、1パケット受信するごとに抜ける
                    //    (指定のlengthぶんデータがたまるのを待つわけではない)
                    //    何もパケットが来なければタイムアウトまで待つ
                    //    this.Close()/this.Disconnect()すれば例外発生して抜ける
                    try
                    {
                        size = networkStream.Read(data, 0, data.Length);
                    }catch{
                        if (!threadQuit){
                            if(networkStream == null){
                                Console.WriteLine(tag + "自分側からの切断");
                            }else{
                                Console.WriteLine(tag + "受信待ちタイムアウト");
                            }
                        }
                        break; // this.Close() or this.Disconnect() or 受信タイムアウトの場合
                    }
                    // Readが0を返したら切断されたと判断。
                    if (size == 0)
                    {
                        Console.WriteLine(tag + "相手側からの切断");
                        break;
                    }
                    // 受信データあり。イベント発生
                    else
                    {
                        receivedPackets.Enqueue(data);
                        if (Received != null) Received(this, EventArgs.Empty); // 受信イベント発行
                    }
                } // 受信待ちループ

                this.Disconnect();
                Console.WriteLine(tag + "切断完了");

                if (Disconnected != null) Disconnected(this, EventArgs.Empty); // 切断イベント発行

            } // 接続待ちループ

            Console.WriteLine(tag + "スレッド終了");
            IsOpen = false;
        }

        #endregion
    }
}
