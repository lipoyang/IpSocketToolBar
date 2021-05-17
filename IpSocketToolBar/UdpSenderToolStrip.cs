using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace IpSocketToolBar
{
    /// <summary>
    /// UDP送信のツールバー
    /// </summary>
    public class UdpSenderToolStrip : ToolStrip
    {
        #region イベント

        /// <summary>
        /// 送信器を開始した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Started = null;

        /// <summary>
        /// 送信器を停止した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Stopped = null;

        #endregion

        #region プロパティ

        /// <summary>
        /// UDP送信器
        /// </summary>
        public UdpSender Sender { get => udpSender; }

        #endregion

        #region 内部処理

        // UDP送信器
        UdpSender udpSender;

        #endregion
    }
}
