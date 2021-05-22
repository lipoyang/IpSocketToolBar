using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace IpSocketToolBar
{
    /// <summary>
    /// ステータス表示のツールバー
    /// </summary>
    public class StatusToolStrip : ToolStrip
    {
        #region 公開プロパティ

        /// <summary>
        /// 詳細情報を表示するか？
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public bool MoreInformed
        {
            get => moreInformed;
            set{
                if(moreInformed != value) {
                    moreInformed = value;
                    if (moreInformed) {
                        this.Items.Add(separator);
                        this.Items.Add(textSubStatus);
                    } else {
                        this.Items.Remove(separator);
                        this.Items.Remove(textSubStatus);
                    }
                }
            }
        }
        bool moreInformed = true;

        #endregion

        #region 内部フィールド

        // TCPソケット
        internal TcpSocket tcpSocket = null;

        // ロック用
        readonly object lockObj = new object();

        // 表示更新用タイマ
        // ※ System.Windows.Forms.Timer だとUIスレッド以外では使えない
        readonly System.Timers.Timer timer = new System.Timers.Timer();

        // 切断直後か？ (切断原因の表示用)
        bool disconnetedNow = false;

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StatusToolStrip()
        {
            // コンポーネントの初期化
            InitializeComponent();
        }
        
        #endregion

        #region 内部メソッド (internal)

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        internal void UpdateStatus()
        {
            lock (lockObj)
            {
                // 切断メッセージ表示中は「停止中」への表示更新を保留
                if (!tcpSocket.IsOpen && disconnetedNow) return;

                updateStatusText();
            }
        }

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        /// <param name="reason">切断理由</param>
        internal void UpdateStatus(DisconnectReason reason)
        {
            lock (lockObj)
            {
                disconnetedNow = true;
                string text = "";
                switch (reason)
                {
                    case DisconnectReason.ByMe:
                        text = "自分側から切断しました";
                        break;
                    case DisconnectReason.ByHim:
                        text = "相手側から切断しました";
                        break;
                    case DisconnectReason.Timeout:
                        text = "タイムアウトで切断しました";
                        break;
                    case DisconnectReason.Failed:
                        text = "接続に失敗しました";
                        break;
                }
                setText("切断", text);

                waitDo(() => {
                    disconnetedNow = false;
                    updateStatusText();
                });
            }
        }
        #endregion

        #region 内部メソッド (private)

        // ステータス表示の文字列を更新
        private void updateStatusText()
        {
            if (tcpSocket.IsOpen)
            {
                bool isServer = (tcpSocket is TcpServerSocket);

                if (tcpSocket.IsConnected)
                {
                    string text = isServer ? "相手のアドレス " : "自分のアドレス ";
                    text += isServer ? tcpSocket.RemoteAddress : tcpSocket.LocalAddress;
                    text += " ポート番号 ";
                    text += (isServer ? tcpSocket.RemotePort : tcpSocket.LocalPort).ToString();
                    setText("接続済み", text);
                }
                else
                {
                    setText(isServer ? "接続待ち" : "接続試行中");
                }
            }
            else
            {
                setText("停止中");
            }
        }

        // ステータス表示の文字列を設定
        private void setText(string mainStatus, string subStatus ="")
        {
            this.BeginInvoke((Action)(() => {
                textMainStatus.Text = mainStatus;
                textSubStatus.Text = subStatus;
            }));
        }

        // 一定時間後に実行(表示更新用)
        private void waitDo(Action action)
        {
            timer.Stop();
            timer.Interval = 3000; // 3秒後に実行
            timer.Elapsed += (sender, e) => {
                lock (lockObj)
                {
                    timer.Stop();
                    action();
                }
            };
            timer.Start();
        }

        // サイズ変更時の処理
        protected override void OnSizeChanged(EventArgs e)
        {
            int h = this.Font.Height;
            Size size1 = new Size(h * 6, h);
            textMainStatus.Size = size1;
            Size size2 = new Size(h * 20, h);
            textSubStatus.Size = size2;

            base.OnSizeChanged(e);
        }

        // フォント変更時の処理
        protected override void OnFontChanged(EventArgs e)
        {
            textMainStatus.Font = this.Font;
            textSubStatus.Font = this.Font;

            base.OnFontChanged(e);
        }

        #endregion

        #region 初期化処理(デザイナーの生成コードから流用)

        // ステータス表示(概要)
        ToolStripLabel textMainStatus;
        // ステータス表示(詳細)
        ToolStripLabel textSubStatus;
        // セパレータ(縦棒)
        ToolStripSeparator separator;

        // コンポーネントの初期化
        private void InitializeComponent()
        {
            this.textMainStatus = new ToolStripLabel();
            this.textSubStatus = new ToolStripLabel();
            this.separator = new ToolStripSeparator();

            this.SuspendLayout();

            this.textMainStatus.ToolTipText = "ステータスの概要を表示します";
            this.textMainStatus.Text = "停止中";

            this.textSubStatus.ToolTipText = "ステータスの詳細を表示します";
            this.textSubStatus.Text = "";

            this.Items.Add(textMainStatus);
            this.Items.Add(separator);
            this.Items.Add(textSubStatus);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
