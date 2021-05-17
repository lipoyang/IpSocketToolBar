using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Net; // IPAddress
using System.Net.Sockets; // AddressFamily
using IniFileSharp;

namespace IpSocketToolBar
{
    /// <summary>
    /// TCPクライアントのツールバー
    /// </summary>
    [DefaultEvent("Received")]
    public class TcpClientToolStrip : ToolStrip
    {
        #region イベント

        /// <summary>
        /// クライアントと接続した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Connected
        {
            add => tcpClient.Connected += value;
            remove => tcpClient.Connected -= value;
        }

        /// <summary>
        /// クライアントと切断された時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Disconnected
        {
            add => tcpClient.Disconnected += value;
            remove => tcpClient.Disconnected -= value;
        }

        /// <summary>
        /// データを受信した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Rreceived
        {
            add => tcpClient.Received += value;
            remove => tcpClient.Received -= value;
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// TCPクライアント
        /// </summary>
        public TcpClientTrx Client { get => tcpClient; }

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TcpClientToolStrip()
        {
            // コンポーネントの初期化
            InitializeComponent();

            // 表示状態の初期化
            textIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        /// <summary>
        /// 初期化処理。フォームの開始時(Loadイベント)に呼んでください。
        /// </summary>
        /// <param name="iniFileName">設定INIファイルのパス</param>
        /// <param name="section">設定INIファイルのセクション名</param>
        public void Begin(string iniFileName = @".\SETTING.INI", string section = "TCP_CLIENT")
        {
            // IPアドレスとポート番号の既定値を設定ファイルから読み出し
            iniFile = new IniFile(iniFileName);
            iniSection = section;
            defaultIpAddress = iniFile.ReadString(iniSection, "IP_ADDRESS", "");
            defaultPort = iniFile.ReadInteger(iniSection, "PORT", 1234);

            // IPアドレスリストの更新
            textIpAddress.Text = defaultIpAddress;
            textPort.Text = defaultPort.ToString();
        }

        /// <summary>
        /// 終了処理。フォームの終了時(FormClosingイベント)に呼んでください。
        /// </summary>
        public void End()
        {
            // TCPサーバが開始していたら停止する
            try{
                if (tcpClient.IsOpen){
                    tcpClient.Close();
                }
            }catch{
                ;
            }

            // COMポートとボーレートの既定値を設定ファイルに保存
            iniFile.WriteString(iniSection, "IP_ADDRESS", defaultIpAddress);
            iniFile.WriteInteger(iniSection, "PORT", defaultPort);
        }

        /// <summary>
        /// TCPサーバを開く
        /// </summary>
        /// <returns>成否</returns>
        public bool Open()
        {
            this.Invoke((Action)(() => {
                buttonStart.PerformClick();
            }));
            return tcpClient.IsOpen;
        }

        /// <summary>
        /// TCPサーバを閉じる
        /// </summary>
        public void Close()
        {
            this.Invoke((Action)(() => {
                buttonStop.PerformClick();
            }));
        }

        #endregion

        #region 内部処理

        // 設定ファイル
        IniFile iniFile;
        // 設定ファイルのセクション名
        string iniSection;
        // デフォルトのIPアドレスとポート番号
        string defaultIpAddress = "127.0.0.1";
        int defaultPort = 1234;

        // TCPクライアント
        readonly TcpClientTrx tcpClient = new TcpClientTrx();

        // 開始ボタンクリック時の処理
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // IPアドレスのチェック
            IPAddress ipAddress;
            string ipAddressStr = textIpAddress.Text;
            if (ipAddressStr == ""){
                showErrorMessage("IPアドレスを指定してください");
                return;
            }
            else{
                try{
                    ipAddress = IPAddress.Parse(ipAddressStr);
                }catch{
                    showErrorMessage("IPアドレスが不正です");
                    return;
                }
            }

            // ポート番号のチェック
            if (!int.TryParse(textPort.Text, out int port))
            {
                showErrorMessage("ポート番号が不正です");
                return;
            }
            // ポートを開く
            tcpClient.Connect(ipAddressStr, port);

            // IPアドレスとポート番号を更新 (ここで毎回ファイル保存はしない)
            defaultIpAddress = ipAddressStr;
            defaultPort = port;

            textIpAddress.Enabled = false;
            textPort.Enabled = false;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;

            // イベント発行
            //if(Started != null) Started(this, EventArgs.Empty);

            this.Update(); // 受信が始まるので念のために強制的に表示更新
        }

        // 停止ボタンクリック時の処理
        private void buttonStop_Click(object sender, EventArgs e)
        {
            // ポートを閉じる
            tcpClient.Stop();

            textIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;

            // イベント発行
            //if (Stopped != null) Stopped(this, EventArgs.Empty);
        }

        // エラーメッセージの表示
        private void showErrorMessage(string text)
        {
            MessageBox.Show(
                text,
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
                );
        }

        // サイズ変更時の処理
        protected override void OnSizeChanged(EventArgs e)
        {
            int h = this.Font.Height;
            Size size1 = new Size(h * 8, h);
            textIpAddress.Size = size1;
            Size size2 = new Size(h * 3, h);
            textPort.Size = size2;

            base.OnSizeChanged(e);
        }

        // フォント変更時の処理
        protected override void OnFontChanged(EventArgs e)
        {
            textIpAddress.Font = this.Font;
            textPort.Font = this.Font;

            base.OnFontChanged(e);
        }

        // マウスが入った時の処理
        // (フォーカスが無いときのボタンクリックが効かない問題の対策)
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!this.Focused)
            {
                this.Focus();
            }
        }

        #endregion

        #region 初期化処理(デザイナーの生成コードから流用)

        // IPアドレスボックス
        ToolStripTextBox textIpAddress;
        // ポート番号ボックス
        ToolStripTextBox textPort;
        // 開始ボタン
        ToolStripButton buttonStart;
        // 停止ボタン
        ToolStripButton buttonStop;

        // コンポーネントの初期化
        private void InitializeComponent()
        {
            this.textIpAddress = new ToolStripTextBox();
            this.textPort = new ToolStripTextBox();
            this.buttonStart = new ToolStripButton();
            this.buttonStop = new ToolStripButton();
            this.SuspendLayout();

            var labelIpAddress = new ToolStripLabel("相手のアドレス");
            var labelPort = new ToolStripLabel("ポート番号");

            this.textIpAddress.ToolTipText = "相手のIPアドレスの指定";
            this.textIpAddress.Text = "";

            this.textPort.ToolTipText = "ポート番号の指定";
            this.textPort.Text = "";

            this.buttonStart.Text = "接続";
            this.buttonStart.ToolTipText = "接続";
            this.buttonStart.Click += buttonStart_Click;

            this.buttonStop.Text = "切断";
            this.buttonStop.ToolTipText = "切断";
            this.buttonStop.Click += buttonStop_Click;

            this.Items.Add(labelIpAddress);
            this.Items.Add(textIpAddress);
            this.Items.Add(labelPort);
            this.Items.Add(textPort);
            this.Items.Add(buttonStart);
            this.Items.Add(buttonStop);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
