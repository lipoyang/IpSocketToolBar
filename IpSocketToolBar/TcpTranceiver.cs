using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; // Thread, Timeout
using System.Net.Sockets; // TcpClient, NetworkStream

namespace IpSocketToolBar
{
    /// <summary>
    /// TCPクライアントのパケット送受信器 (TcpServerTrx, TcpClientTrx の基底クラス)
    /// </summary>
    public class TcpTranceiver
    {
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
        /// 通信を開いているか？
        /// </summary>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// 接続維持タイムアウト時間[ミリ秒]
        /// </summary>
        public int AliveTimeOut = Timeout.Infinite;

        /// <summary>
        /// 受信ポーリング周期[ミリ秒]
        /// </summary>
        public int PollingInterval = 20;

        #endregion

        #region 内部フィールド

        // TCPクライアント, ストリーム
        protected TcpClient client = null;
        protected NetworkStream networkStream = null;

        // 受信パケットのキュー
        protected readonly Queue<byte[]> receivedPackets = new Queue<byte[]>();

        #endregion

        #region 公開メソッド

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="data">バイト列データ</param>
        public void Send(byte[] data)
        {
            if (networkStream != null)
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
        /// 受信したパケットのペイロードを取得する
        /// </summary>
        /// <returns>パケットのペイロード</returns>
        public PacketPayload GetPacket()
        {
            byte[] data = receivedPackets.Dequeue();
            if (data != null)
            {
                PacketPayload packet = new PacketPayload(data);
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
            if(data != null)
            {
                string str = Encoding.ASCII.GetString(data);
                str = str.Remove(str.IndexOf("\0"));
                return str;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// パケットの受信を待つ
        /// </summary>
        /// <param name="timeout">タイムアウト時間[ミリ秒]</param>
        /// <returns>パケットのペイロード</returns>
        public PacketPayload WaitPacket(int timeout)
        {
            byte[] data = WaitBytes(timeout);
            if(data != null)
            {
                PacketPayload packet = new PacketPayload(data);
                return packet;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
