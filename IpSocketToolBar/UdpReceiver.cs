﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; // Thread, Timeout
using System.Net; // IPAddress, IPEndPoint
using System.Net.Sockets; // UdpClient

namespace IpSocketToolBar
{
    /// <summary>
    /// UDPパケット受信器
    /// </summary>
    public class UdpReceiver
    {
        #region イベント

        /// <summary>
        /// パケットを受信したとき
        /// </summary>
        public event EventHandler Received = null;

        #endregion

        #region 公開プロパティ/フィールド

        /// <summary>
        /// 自分のIPアドレス (読み取り専用)
        /// </summary>
        public string LocalAddress { protected set; get; }
        /// <summary>
        /// 自分のポート番号 (読み取り専用)
        /// </summary>
        public int LocalPort { protected set; get; }
        /// <summary>
        /// 相手のIPアドレス (読み取り専用)
        /// </summary>
        public string RemoteAddress { protected set; get; }
        /// <summary>
        /// 相手のポート番号 (読み取り専用)
        /// </summary>
        public int RemotePort { protected set; get; }

        /// <summary>
        /// 受信ポーリング周期[ミリ秒]
        /// </summary>
        public int PollingInterval = 20;

        #endregion

        #region 内部フィールド

        // サーバのスレッド
        Thread ReceiverThread;
        bool ReceiverThreadQuit = false;

        // UDPクライアント
        UdpClient client = null;

        // 受信パケットのキュー
        protected readonly Queue<byte[]> receivedPackets = new Queue<byte[]>();

        #endregion

        #region 公開メソッド

        /// <summary>
        /// 受信待ち受けを開始する
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
        /// 受信待ち受けを開始する
        /// </summary>
        /// <param name="address">自分のIPアドレス</param>
        /// <param name="port">自分のポート番号</param>
        public void Open(IPAddress address, int port)
        {
            // 自分
            IPEndPoint localEP = new IPEndPoint(address, port);
            client = new UdpClient(localEP);

            LocalAddress = address.ToString();
            LocalPort = port;

            // 受信パケットのキューをクリア
            receivedPackets.Clear();

            // 受信スレッドを開始
            ReceiverThreadQuit = false;
            ReceiverThread = new Thread(new ThreadStart(ReceiverThreadFunc));
            ReceiverThread.Start();
        }

        /// <summary>
        /// 受信待ち受けを停止する
        /// </summary>
        public void Close()
        {
            //UdpClientを閉じる
            client.Close();

            // 受信スレッド停止
            ReceiverThreadQuit = true;
            ReceiverThread.Join();
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

        /// <summary>
        /// バイト列データの受信を待つ
        /// </summary>
        /// <param name="timeout">タイムアウト時間[ミリ秒]</param>
        /// <returns>バイト列データ</returns>
        public byte[] WaitBytes(int timeout)
        {
            DateTime startTime = DateTime.Now;

            // 受信パケットのキューをいったん破棄
            receivedPackets.Clear();

            // パケット受信かタイムアウトまで
            while (true)
            {
                // 受信パケットがあれば返す
                if (receivedPackets.Count > 0)
                {
                    byte[] data = receivedPackets.Dequeue();
                    return data;
                }
                // タイムアウト判定
                DateTime endTime = DateTime.Now;
                TimeSpan ts = endTime - startTime;
                int elasped = (int)ts.TotalMilliseconds;
                if (elasped >= timeout) break;

                Thread.Sleep(PollingInterval);
            }
            return null;
        }
        /// <summary>
        /// 文字列データの受信を待つ
        /// </summary>
        /// <param name="timeout">タイムアウト時間[ミリ秒]</param>
        /// <returns>文字列データ</returns>
        public string WaitString(int timeout)
        {
            byte[] data = WaitBytes(timeout);
            string str = Encoding.ASCII.GetString(data);
            return str;
        }
        /// <summary>
        /// パケットの受信を待つ
        /// </summary>
        /// <param name="timeout">タイムアウト時間[ミリ秒]</param>
        /// <returns>パケットのペイロード</returns>
        public PacketPayload WaitPacket(int timeout)
        {
            byte[] data = WaitBytes(timeout);
            PacketPayload packet = new PacketPayload(data);
            return packet;
        }

        #endregion

        #region 内部メソッド

        // サーバのスレッド関数
        private void ReceiverThreadFunc()
        {
            while (!ReceiverThreadQuit)
            {
                try
                {
                    //データを受信する (TODO ※Closeで即座に中断できるか？)
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    // 相手(送信元)のIPアドレスとポート
                    RemoteAddress = remoteEP.Address.ToString();
                    RemotePort = remoteEP.Port;
                    Console.WriteLine("送信元:{0}:{1}", RemoteAddress, RemotePort);

                    // 受信イベント発生
                    if (Received != null)
                    {
                        receivedPackets.Enqueue(data);
                        Received(this, EventArgs.Empty);
                    }
                }
                catch
                {
                    ;
                }
            }
        }
        #endregion
    }
}
