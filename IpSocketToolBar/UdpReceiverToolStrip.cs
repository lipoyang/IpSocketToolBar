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
        public event EventHandler Rreceived
        {
            add => udpReceiver.Received += value;
            remove => udpReceiver.Received -= value;
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// UDP受信器
        /// </summary>
        public UdpReceiver Receiver { get => udpReceiver; }

        #endregion

        #region 公開メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UdpReceiverToolStrip()
        {
            // コンポーネントの初期化
            InitializeComponent();

            // 表示状態の初期化
            listIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        /// <summary>
        /// 初期化処理。フォームの開始時(Loadイベント)に呼んでください。
        /// </summary>
        /// <param name="iniFileName">設定INIファイルのパス</param>
        /// <param name="section">設定INIファイルのセクション名</param>
        public void Begin(string iniFileName = @".\SETTING.INI", string section = "UDP_RECEIVER")
        {
            // IPアドレスとポート番号の既定値を設定ファイルから読み出し
            iniFile = new IniFile(iniFileName);
            iniSection = section;
            defaultIpAddress = iniFile.ReadString(iniSection, "IP_ADDRESS", "");
            defaultPort = iniFile.ReadInteger(iniSection, "PORT", 1234);

            // IPアドレスリストの更新
            updateIpAddressList();
            textPort.Text = defaultPort.ToString();
        }

        /// <summary>
        /// 終了処理。フォームの終了時(FormClosingイベント)に呼んでください。
        /// </summary>
        public void End()
        {
            // UDP受信器が開始していたら停止する
            try{
                if (udpReceiver.IsOpen){
                    udpReceiver.Close();
                }
            }catch{
                ;
            }

            // COMポートとボーレートの既定値を設定ファイルに保存
            iniFile.WriteString(iniSection, "IP_ADDRESS", defaultIpAddress);
            iniFile.WriteInteger(iniSection, "PORT", defaultPort);
        }

        /// <summary>
        /// UDP受信器を開く
        /// </summary>
        /// <returns>成否</returns>
        public bool Open()
        {
            this.Invoke((Action)(() => {
                buttonStart.PerformClick();
            }));
            return udpReceiver.IsOpen;
        }

        /// <summary>
        /// UDP受信器を閉じる
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

        // UDP受信器
        readonly　UdpReceiver udpReceiver = new UdpReceiver();

        // IPアドレスリストのドロップダウン時の処理
        private void listIpAddress_DropDown(object sender, EventArgs e)
        {
            // IPアドレスリストの更新
            updateIpAddressList();
        }

        // 開始ボタンクリック時の処理
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // IPアドレスのチェック
            IPAddress ipAddress;
            string ipAddressStr = listIpAddress.Text;
            if (ipAddressStr == ""){
                showErrorMessage("IPアドレスを選択してください");
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
            udpReceiver.Open(ipAddressStr, port);

            // IPアドレスとポート番号を更新 (ここで毎回ファイル保存はしない)
            defaultIpAddress = ipAddressStr;
            defaultPort = port;

            listIpAddress.Enabled = false;
            textPort.Enabled = false;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;

            // イベント発行
            if(Started != null) Started(this, EventArgs.Empty);

            this.Update(); // 受信が始まるので念のために強制的に表示更新
        }

        // 停止ボタンクリック時の処理
        private void buttonStop_Click(object sender, EventArgs e)
        {
            // ポートを閉じる
            udpReceiver.Close();

            listIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;

            // イベント発行
            if (Stopped != null) Stopped(this, EventArgs.Empty);
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

        // 自分のIPアドレスのリストの更新
        private void updateIpAddressList()
        {
            // 自分のIPアドレスを列挙
            string hostname = Dns.GetHostName();
            IPAddress[] ipAddressAll = Dns.GetHostAddresses(hostname);
            List<string> ipAddressList = new List<string>();
            foreach (IPAddress address in ipAddressAll)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddressList.Add(address.ToString());
                }
            }
            ipAddressList.Add("127.0.0.1");
            ipAddressList.Add("0.0.0.0");

            // COMポートリストの更新
            listIpAddress.Items.Clear();
            listIpAddress.Items.AddRange(ipAddressList.ToArray());

            // 既定値を選択
            listIpAddress.SelectedItem = defaultIpAddress;
            // 無ければ0番目の項目を選択
            if ((string)listIpAddress.SelectedItem != defaultIpAddress)
            {
                listIpAddress.SelectedIndex = 0;
            }
        }

        // サイズ変更時の処理
        protected override void OnSizeChanged(EventArgs e)
        {
            int h = this.Font.Height;
            Size size1 = new Size(h * 8, h);
            listIpAddress.Size = size1;
            Size size2 = new Size(h * 3, h);
            textPort.Size = size2;

            base.OnSizeChanged(e);
        }

        // フォント変更時の処理
        protected override void OnFontChanged(EventArgs e)
        {
            listIpAddress.Font = this.Font;
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

        // IPアドレスリスト
        ToolStripComboBox listIpAddress;
        // ポート番号ボックス
        ToolStripTextBox textPort;
        // 開始ボタン
        ToolStripButton buttonStart;
        // 停止ボタン
        ToolStripButton buttonStop;

        // コンポーネントの初期化
        private void InitializeComponent()
        {
            this.listIpAddress = new ToolStripComboBox();
            this.textPort = new ToolStripTextBox();
            this.buttonStart = new ToolStripButton();
            this.buttonStop = new ToolStripButton();
            this.SuspendLayout();

            var labelIpAddress = new ToolStripLabel("自分のアドレス");
            var labelPort = new ToolStripLabel("ポート番号");

            this.listIpAddress.DropDownStyle = ComboBoxStyle.DropDownList; // 編集不可
            this.listIpAddress.DropDown += listIpAddress_DropDown;
            this.listIpAddress.ToolTipText = "自分のIPアドレスの選択";
            this.listIpAddress.Items.Clear();

            this.textPort.ToolTipText = "ポート番号の指定";
            this.textPort.Text = "";

            this.buttonStart.Text = "開始";
            this.buttonStart.ToolTipText = "開始";
            this.buttonStart.Click += buttonStart_Click;

            this.buttonStop.Text = "停止";
            this.buttonStop.ToolTipText = "停止";
            this.buttonStop.Click += buttonStop_Click;

            this.Items.Add(labelIpAddress);
            this.Items.Add(listIpAddress);
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
