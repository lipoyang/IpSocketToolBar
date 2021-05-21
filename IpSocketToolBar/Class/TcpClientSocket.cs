using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net; // IPAddress, IPEndPoint
using System.Net.Sockets; // TcpClient, NetworkStream
using System.Threading; // Thread, Timeout

namespace IpSocketToolBar
{
    /// <summary>
    /// TCPクライアントのパケット送受信器
    /// </summary>
    public class TcpClientSocket : TcpSocket
    {
        #region イベント

        /// <summary>
        /// パケットを受信したとき
        /// </summary>
        public event EventHandler Received = null;

        /// <summary>
        /// サーバと接続したとき
        /// </summary>
        public event EventHandler Connected = null;

        /// <summary>
        /// サーバから切断したとき
        /// </summary>
        public event EventHandler Disconnected = null;

        #endregion

        #region 公開フィールド

        /// <summary>
        /// 自分の固定ポート番号
        /// </summary>
        public int? FixedLocalPort = null;

        #endregion

        #region 内部フィールド

        // デバッグ用
        const string tag = "TCPクライアント: ";

        #endregion

        #region 公開メソッド

        /// <summary>
        /// クライアントを開始する
        /// (サーバへの接続試行を開始する)
        /// </summary>
        /// <param name="address">相手のIPアドレスまたはホスト名</param>
        /// <param name="port">相手のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(string address, int port)
        {
            // IPアドレスの解釈
            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
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
        /// クライアントを開始する
        /// (サーバへの接続試行を開始する)
        /// </summary>
        /// <param name="address">相手のIPアドレス</param>
        /// <param name="port">相手のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(IPAddress address, int port)
        {
            // 相手のIPアドレスとポート番号
            RemoteAddress = address.ToString();
            RemotePort = port;

            // 自分のポート番号は固定か自動割り当てか
            if(FixedLocalPort == null){
                client = new TcpClient();
            }else{
                var localEP = new IPEndPoint(IPAddress.Any, (int)FixedLocalPort);
                client = new TcpClient(localEP);
            }

            Console.WriteLine(tag + "接続試行開始");

            // スレッドを開始
            threadQuit = false;
            thread = new Thread(new ThreadStart(threadFunc));
            thread.Start();
            return true;
        }

        /// <summary>
        /// クライアントを停止する
        /// </summary>
        public void Close()
        {
            Console.WriteLine(tag + "停止要求");

            threadQuit = true; // スレッド終了要求
            this.Disconnect(); // 接続があれば切断する
            thread.Join();     // スレッド終了待ち
        }

        /// <summary>
        /// 接続を切断する
        /// </summary>
        private void Disconnect() // ※ privateとする
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

            while (!threadQuit) // 接続試行ループ
            {
                // サーバへの接続を試行する
                // ※ ブロッキング処理だが、this.Close()すれば例外発生して抜ける
                //    また、接続失敗でも例外発生して抜ける
                try
                {
                    client.Connect(RemoteAddress, RemotePort);
                }catch{
                    if (!threadQuit){
                        threadQuit = true;
                        Console.WriteLine(tag + "接続失敗");
                        if (Disconnected != null) Disconnected(this, EventArgs.Empty); // 切断イベント発行
                    }
                    break; // this.Close()の場合
                }

                // サーバとクライアントのIPアドレスとポート番号
                LocalAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                LocalPort     = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine(tag + "サーバ({0}:{1})と接続({2}:{3})",
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
                    //    this.Close()すれば例外発生して抜ける
                    try{
                        size = networkStream.Read(data, 0, data.Length);
                    }catch{
                        if (!threadQuit) Console.WriteLine(tag + "受信待ちタイムアウト");
                        break; // this.Close() or 受信タイムアウトの場合
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
                }// 受信待ちループ

                this.Disconnect();
                Console.WriteLine(tag + "切断完了");

                if (Disconnected != null) Disconnected(this, EventArgs.Empty); // 切断イベント発行

                threadQuit = true; // ※ 切断したら接続試行ループを抜ける

            } // 接続試行ループ

            Console.WriteLine(tag + "スレッド終了");
            IsOpen = false;
        }

        #endregion
    }
}
