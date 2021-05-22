using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpSocketToolBar
{
    /// <summary>
    /// IPソケット
    /// </summary>
    public abstract class IpSocket
    {
        #region 公開プロパティ

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
        /// ソケットを開いているか？
        /// </summary>
        public bool IsOpen { get; protected set; }

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public IpSocket()
        {
            IsOpen = false;
        }

        #endregion
    }
}
