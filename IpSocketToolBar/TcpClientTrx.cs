using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net; // IPAddress, IPEndPoint
using System.Net.Sockets; // TcpClient, NetworkStream
using System.Threading; // Thread

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
        /// サーバ側から切断されたとき
        /// </summary>
        public event EventHandler Disconnected = null;

        #endregion

        #region 公開プロパティ/フィールド

        /// <summary>
        /// 接続試行タイムアウト時間[ミリ秒]
        /// </summary>
        public int ConnectingTimeout = 5000;

        #endregion

        #region 内部フィールド

        // サーバのスレッド
        Thread ClientThread;
        bool ClientThreadQuit = false;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// サーバに接続する
        /// </summary>
        /// <param name="address">相手のIPアドレスまたはホスト名("localhost"など)</param>
        /// <param name="port">相手のポート番号</param>
        public void Connect(string address, int port)
        {
            // 自分のIPアドレスとポート番号
            RemoteAddress = address;
            RemotePort = port;

            // サーバのスレッドを開始
            ClientThreadQuit = false;
            ClientThread = new Thread(new ThreadStart(ClientThreadFunc));
            ClientThread.Start();
        }

        #endregion

        #region 内部メソッド

        // サーバのスレッド関数
        private void ClientThreadFunc()
        {
            while (!ClientThreadQuit)
            {
                // サーバと接続
                // client = new TcpClient(RemoteAddress, RemotePort); // これだとブロッキング処理になる

                // 非同期で接続試行開始
                IAsyncResult ar = client.BeginConnect(RemoteAddress, RemotePort, null, null);
                WaitHandle wh = ar.AsyncWaitHandle;
                try{
                    // ここで接続完了を待つ（タイムアウトあり）
                    if (!ar.AsyncWaitHandle.WaitOne(ConnectingTimeout, false)) {
                        client.Close();
                    }
                    client.EndConnect(ar);
                }finally{
                    wh.Close();
                }
                if (!client.Connected) continue; // タイムアウトした場合

                LocalAddress  = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                LocalPort     = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine("サーバー({0}:{1})と接続({2}:{3})",
                    RemoteAddress, RemotePort, LocalAddress, LocalPort);

                // クライアントのストリームを取得し、タイムアウト時間を設定
                networkStream = client.GetStream();
                networkStream.ReadTimeout = this.AliveTimeOut;
                networkStream.WriteTimeout = this.AliveTimeOut; // TODO

                // サーバから送られたデータを受信する
                receivedPackets.Clear();
                byte[] data = new byte[1500];
                int size = 0;
                while (true)
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
                        Console.WriteLine("サーバ切断"); // TODO
                        if (Disconnected != null)
                        {
                            Disconnected(this, EventArgs.Empty);
                        }
                        this.Close();
                        break;
                    }
                    // 受信データあり。イベント発生
                    else if (Received != null)
                    {
                        receivedPackets.Enqueue(data);
                        Received(this, EventArgs.Empty);
                    }
                }//while(true)
            }
        }

        #endregion
    }
}
