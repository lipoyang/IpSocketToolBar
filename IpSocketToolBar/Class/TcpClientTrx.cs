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
    public class TcpClientTrx : TcpTranceiver
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
        /// サーバから切断されたとき
        /// </summary>
        public event EventHandler Disconnected = null;

        #endregion

        #region 公開プロパティ/フィールド

        /// <summary>
        /// 接続試行タイムアウト時間[ミリ秒]
        /// </summary>
        public int ConnectingTimeout = 5000;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// サーバへの接続を開始する
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
                    // ホスト名からIPアドレスを取得
                    ipAddress = Dns.GetHostEntry(address).AddressList[0];
                }catch{
                    return false;
                }
            }
            return Open(ipAddress, port);
        }

        /// <summary>
        /// サーバへの接続を開始する
        /// </summary>
        /// <param name="address">相手のIPアドレスまたはホスト名</param>
        /// <param name="port">相手のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(IPAddress address, int port)
        {
            // 相手のIPアドレスとポート番号
            RemoteAddress = address.ToString();
            RemotePort = port;

            // スレッドを開始
            threadQuit = false;
            thread = new Thread(new ThreadStart(threadFunc));
            thread.Start();
            IsOpen = true;

            return true;
        }

        /// <summary>
        /// サーバとの接続を停止する
        /// </summary>
        public void Close()
        {
            threadQuit = true; // スレッド終了要求
            this.Disconnect(); // 接続があれば切断する
            thread.Join();     // スレッド終了待ち
            IsOpen = false;
        }

        /// <summary>
        /// 接続を切断する (※このメソッドは非公開とする)
        /// </summary>
        private void Disconnect()
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
            while (!threadQuit) // 接続試行ループ
            {
                // サーバへの接続を試行する
                // ※ ブロッキング処理だが、this.Close()すれば例外発生して抜ける
                client = new TcpClient();
                try{
                    client.Connect(RemoteAddress, RemotePort);
                }catch{
                    break; // this.Close()の場合
                }

                // サーバとクライアントのIPアドレスとポート番号
                LocalAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                LocalPort     = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine("サーバ({0}:{1})と接続({2}:{3})",
                    RemoteAddress, RemotePort, LocalAddress, LocalPort);

                if (Connected != null) Connected(this, EventArgs.Empty);

                // クライアントのストリームを取得し、タイムアウト時間を設定
                networkStream = client.GetStream();
                networkStream.ReadTimeout = this.ReadTimeout;
                networkStream.WriteTimeout = this.WriteTimeout;

                // サーバから送られたデータを受信する
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
                        break; // this.Close()の場合
                    }
                    // Readが0を返したら切断されたと判断。
                    if (size == 0)
                    {
                        Console.WriteLine("サーバ切断");
                        if (Disconnected != null) Disconnected(this, EventArgs.Empty);
                        this.Disconnect();
                        break;
                    }
                    // 受信データあり。イベント発生
                    else
                    {
                        receivedPackets.Enqueue(data);
                        if (Received != null) Received(this, EventArgs.Empty);
                    }
                }// 受信待ちループ

            } // 接続試行ループ
        }

        #endregion
    }
}
