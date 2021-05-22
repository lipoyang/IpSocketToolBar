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
        public bool MoreInformed
        {
            get => moreInformed;
            set{
                moreInformed = value;
                // TODO
            }
        }
        bool moreInformed = false;

        #endregion

        #region 内部フィールド

        // サーバか？
        internal bool isServer = false;

        // ソケットの状態
        enum IpSocketStatus
        {
            Closed,
            Opened,
            Connected
        }
        IpSocketStatus status = IpSocketStatus.Closed;

        // ロック用
        readonly object lockObj = new object();

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
        /// ソケットを開いたとき
        /// </summary>
        internal void Opened()
        {
            lock (lockObj)
            {
                setText(isServer ? "接続待ち" : "接続試行");
                status = IpSocketStatus.Opened;
            }
        }

        /// <summary>
        /// ソケットを閉じたとき
        /// </summary>
        internal void Closed()
        {
            lock (lockObj)
            {
                if(status == IpSocketStatus.Connected)
                {
                    setText("切断", "自分側から切断しました");
                    waitDo(() => {
                        if (status == IpSocketStatus.Closed) setText("停止中");
                    });
                }else{
                    setText("停止中");
                }
                status = IpSocketStatus.Closed;
            }
        }

        /// <summary>
        /// ソケットが接続したとき
        /// </summary>
        /// <param name="address">IPアドレス</param>
        /// <param name="port">ポート番号</param>
        internal void Connected(string address, int port)
        {
            lock (lockObj)
            {
                string text = isServer ? "相手のアドレス " : "自分のアドレス ";
                text += address;
                text += " ポート番号 ";
                text += port.ToString();
                setText("接続済み", text);

                status = IpSocketStatus.Connected;
            }
        }

        /// <summary>
        /// ソケットが切断したとき
        /// </summary>
        /// <param name="reason">切断理由</param>
        internal void Disconnected(DisconnectReason reason)
        {
            lock (lockObj)
            {
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
                    if (isServer){
                        if (status == IpSocketStatus.Opened) setText("接続待ち");
                    }else{
                        if (status == IpSocketStatus.Closed) setText("停止中");
                    }
                });

                // サーバは切断しても閉じない。クライアントは切断したら閉じる
                status = isServer ? IpSocketStatus.Opened : IpSocketStatus.Closed;
            }
        }
        #endregion

        #region 内部メソッド (private)

        // ステータスの表示
        private void setText(string mainStatus, string subStatus ="")
        {
            this.Invoke((Action)(() => {
                textMainStatus.Text = mainStatus;
                textSubStatus.Text = subStatus;
            }));
        }

        // 一定時間後に実行(表示更新用)
        private void waitDo(Action action)
        {
            timer.Stop();
            timer.Interval = 2000; // 2秒後に実行
            timer.Tick += (sender, e) => {
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
        // 表示更新用タイマ
        Timer timer;

        // リソース管理用
        IContainer components = null;

        // 使用中のリソースをすべてクリーンアップします
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // コンポーネントの初期化
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textMainStatus = new ToolStripLabel();
            this.textSubStatus = new ToolStripLabel();
            this.timer = new Timer(this.components); // リソース管理のための引数
            this.SuspendLayout();

            var separator = new ToolStripSeparator();

            this.textMainStatus.ToolTipText = "ステータスの概要を表示します";
            this.textMainStatus.Text = "";

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
