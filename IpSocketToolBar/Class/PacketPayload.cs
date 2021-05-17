using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpSocketToolBar
{
    /// <summary>
    /// パケットのペイロード
    /// </summary>
    public class PacketPayload
    {
        #region 公開フィールド

        /// <summary>
        /// ペイロードのバイト列
        /// </summary>
        public byte[] Data;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">バイト列</param>
        public PacketPayload(byte[] data)
        {
            this.Data = data;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="stringData">文字列</param>
        public PacketPayload(string stringData)
        {
            this.Data = Encoding.ASCII.GetBytes(stringData);
        }

        #endregion
    }
}
