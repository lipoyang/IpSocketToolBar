﻿using System;
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
        /// サーバ側と接続したとき
        /// </summary>
        public event EventHandler Connected = null;

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
        /// コンストラクタ
        /// </summary>
        public TcpClientTrx()
        {
            IsOpen = false;
        }

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

            IsOpen = true; // TODO
        }

        /// <summary>
        /// クライアントからの接続待ち受けを停止する
        /// </summary>
        public void Stop()
        {
            // サーバのスレッドを停止
            ClientThreadQuit = true;
            this.Close(); // 接続があれば閉じる
            ClientThread.Join();
            IsOpen = false;
        }

        /// <summary>
        /// 接続をこちらから閉じる
        /// </summary>
        public void Close()
        {
            //閉じる
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
        private void ClientThreadFunc()
        {
            while (!ClientThreadQuit)
            {
                // サーバと接続
                try {
                    client = new TcpClient(RemoteAddress, RemotePort);
                } catch {
                    continue;
                }

                LocalAddress  = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                LocalPort     = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                RemoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                RemotePort    = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine("サーバー({0}:{1})と接続({2}:{3})",
                    RemoteAddress, RemotePort, LocalAddress, LocalPort);

                if (Connected != null) Connected(this, EventArgs.Empty);

                // クライアントのストリームを取得し、タイムアウト時間を設定
                networkStream = client.GetStream();
                networkStream.ReadTimeout = this.AliveTimeOut;
                networkStream.WriteTimeout = this.AliveTimeOut; // TODO

                // サーバから送られたデータを受信する
                receivedPackets.Clear();
                byte[] data = new byte[1500];
                int size;
                while (true)
                {
                    // データを受信する
                    // ※ ブロッキング処理だが、1パケット受信するごとに抜ける
                    //    指定のlengthぶんデータがたまるのを待つわけではない
                    //    何もパケットが来なければタイムアウトまで待つ
                    // TODO this.Close()で抜けるか要確認
                    try
                    {
                        size = networkStream.Read(data, 0, data.Length);
                    }
                    catch
                    {
                        break; // this.Close()の場合
                    }
                    // Readが0を返したら切断と判断。
                    if (size == 0)
                    {
                        break;
                    }
                    // 受信データあり。イベント発生
                    else
                    {
                        receivedPackets.Enqueue(data);
                        if (Received != null) Received(this, EventArgs.Empty);
                    }
                }//while(true)

                Console.WriteLine("サーバ切断");
                if (Disconnected != null)
                {
                    Disconnected(this, EventArgs.Empty);
                }
                this.Close();
            }
        }

#endregion
    }
}
