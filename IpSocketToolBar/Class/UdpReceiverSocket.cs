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
    public class UdpReceiverSocket : IpSocket
    {
        #region イベント

        /// <summary>
        /// パケットを受信したとき
        /// </summary>
        public event EventHandler Received = null;

        #endregion

        #region 公開フィールド

        /// <summary>
        /// 受信ポーリング周期[ミリ秒]
        /// </summary>
        public int PollingInterval = 20;

        #endregion

        #region 内部フィールド

        // サーバのスレッド
        Thread thread;
        bool threadQuit = false;

        // UDPクライアント
        UdpClient client = null;

        // 受信パケットのキュー
        protected readonly Queue<byte[]> receivedPackets = new Queue<byte[]>();

        #endregion

        #region 公開メソッド

        /// <summary>
        /// 受信器を開始する
        /// </summary>
        /// <param name="address">自分のIPアドレスまたはホスト名</param>
        /// <param name="port">自分のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(string address, int port)
        {
            // IPアドレスの解釈
            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
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
        /// 受信器を開始する
        /// </summary>
        /// <param name="address">自分のIPアドレス</param>
        /// <param name="port">自分のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(IPAddress address, int port)
        {
            try
            {
                // 自分のIPアドレスとポート番号
                IPEndPoint localEP = new IPEndPoint(address, port);
                LocalAddress = address.ToString();
                LocalPort = port;
                
                client = new UdpClient(localEP);

                // 受信パケットのキューをクリア
                receivedPackets.Clear();

                // 受信スレッドを開始
                threadQuit = false;
                thread = new Thread(new ThreadStart(threadFunc));
                thread.Start();
                IsOpen = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 受信器を停止する
        /// </summary>
        public void Close()
        {
            threadQuit = true; // スレッド終了要求
            client.Close();    // 接続があれば切断する
            thread.Join();     // スレッド終了待ち
            IsOpen = false;
        }

        /// <summary>
        /// 受信したバイト列データを取得する
        /// </summary>
        /// <returns>バイト列データ</returns>
        public byte[] GetBytes()
        {
            if (receivedPackets.Count == 0) return null;
            byte[] data = receivedPackets.Dequeue();
            return data;
        }
        /// <summary>
        /// 受信した文字列データを取得する
        /// </summary>
        /// <returns>文字列データ</returns>
        public string GetString()
        {
            if (receivedPackets.Count == 0) return null;
            byte[] data = receivedPackets.Dequeue();
            if (data != null)
            {
                string str = Encoding.ASCII.GetString(data);
                return str;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 受信したパケットデータを取得する
        /// </summary>
        /// <returns>パケットデータ</returns>
        public PacketData GetPacket()
        {
            if (receivedPackets.Count == 0) return null;
            byte[] data = receivedPackets.Dequeue();
            if (data != null)
            {
                PacketData packet = new PacketData(data);
                return packet;
            }
            else
            {
                return null;
            }
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
            //receivedPackets.Clear();

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
            if (data != null)
            {
                string str = Encoding.ASCII.GetString(data);
                return str;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// パケットデータの受信を待つ
        /// </summary>
        /// <param name="timeout">タイムアウト時間[ミリ秒]</param>
        /// <returns>パケットデータ</returns>
        public PacketData WaitPacket(int timeout)
        {
            byte[] data = WaitBytes(timeout);
            if (data != null)
            {
                PacketData packet = new PacketData(data);
                return packet;
            }
            else
            {
                return null;
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
                    //データを受信する (※ブロッキング処理だが、this.Close()で中断)
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    // 相手(送信元)のIPアドレスとポート
                    RemoteAddress = remoteEP.Address.ToString();
                    RemotePort = remoteEP.Port;
                    //Console.WriteLine("UDP受信器: 送信元:{0}:{1}", RemoteAddress, RemotePort);

                    receivedPackets.Enqueue(data);

                    // 受信イベント発生
                    Received?.Invoke(this, EventArgs.Empty);
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
