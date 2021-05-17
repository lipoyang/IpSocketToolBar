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
    /// UDP受信のツールバー
    /// </summary>
    [DefaultEvent("Received")]
    public class UdpReceiverToolStrip : ToolStrip
    {
        #region イベント

        /// <summary>
        /// 受信器を開始した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Started = null;

        /// <summary>
        /// 受信器を停止した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Stopped = null;

        /// <summary>
        /// データを受信した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Rreceived = null;

        #endregion

        #region プロパティ

        /// <summary>
        /// UDP受信器
        /// </summary>
        public UdpReceiver Receiver { get => udpReceiver; }

        #endregion

        #region 内部処理

        // UDP受信器
        UdpReceiver udpReceiver;

        #endregion
    }
}
