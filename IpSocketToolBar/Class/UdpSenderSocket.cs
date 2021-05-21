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
    /// UDPパケット送信器
    /// </summary>
    public class UdpSenderSocket : IpSocket
    {
        #region 公開フィールド

        /// <summary>
        /// 自分の固定ポート番号
        /// </summary>
        public int? FixedLocalPort = null;

        #endregion

        #region 内部フィールド

        // UDPクライアント
        UdpClient client = null;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// 送信器を開始する
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
        /// 送信器を開始する
        /// </summary>
        /// <param name="address">相手のIPアドレス</param>
        /// <param name="port">相手のポート番号</param>
        /// <returns>成否</returns>
        public bool Open(IPAddress address, int port)
        {
            try
            {
                // 相手のIPアドレスとポート番号
                IPEndPoint remoteEP = new IPEndPoint(address, port);
                RemoteAddress = address.ToString();
                RemotePort = port;

                // 自分のポート番号は固定か自動割り当てか
                if (FixedLocalPort == null){
                    client = new UdpClient();
                }else{
                    client = new UdpClient((int)FixedLocalPort);
                }
                // UDPはコネクションレスなので、事前に接続先と接続を確立するわけではない。
                // ただConnect()で送信先を指定しておくとSend()の引数に送信先を省略できる。
                client.Connect(remoteEP);

                IsOpen = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 送信器を停止する
        /// </summary>
        public void Close()
        {
            client.Close();
            client = null;
            IsOpen = false;
        }

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <param name="data">バイト列データ</param>
        public void Send(byte[] data)
        {
            if (client != null)
            {
                // 非同期で送信開始
                _ = client.BeginSend(data, data.Length, null, null);
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

        #endregion
    }
}
