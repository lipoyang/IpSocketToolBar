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
        /// 開始した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Opened = null;

        /// <summary>
        /// 停止した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Closed = null;

        /// <summary>
        /// データを受信した時のイベント
        /// </summary>
        [Browsable(true)]
        [Category("拡張機能")]
        public event EventHandler Rreceived
        {
            add => socket.Received += value;
            remove => socket.Received -= value;
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// ソケット
        /// </summary>
        public UdpReceiverSocket Socket { get => socket; }

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
            textIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonOpen.Enabled = true;
            buttonClose.Enabled = false;
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
            // 開始していたら停止する
            try{
                if (socket.IsOpen){
                    socket.Close();
                }
            }catch{
                ;
            }

            // COMポートとボーレートの既定値を設定ファイルに保存
            iniFile.WriteString(iniSection, "IP_ADDRESS", defaultIpAddress);
            iniFile.WriteInteger(iniSection, "PORT", defaultPort);
        }

        /// <summary>
        /// ソケットを開く
        /// </summary>
        /// <returns>成否</returns>
        public bool Open()
        {
            this.Invoke((Action)(() => {
                buttonOpen.PerformClick();
            }));
            return socket.IsOpen;
        }

        /// <summary>
        /// ソケットを閉じる
        /// </summary>
        public void Close()
        {
            this.Invoke((Action)(() => {
                buttonClose.PerformClick();
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

        // ソケット
        readonly　UdpReceiverSocket socket = new UdpReceiverSocket();

        // 開始ボタンクリック時の処理
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            // IPアドレス/ホスト名のチェック
            IPAddress ipAddress;
            string ipAddressStr = textIpAddress.Text;
            if (ipAddressStr == ""){
                showErrorMessage("IPアドレスを選択してください");
                return;
            }else{
                // IPアドレスの文字列か？
                if (!IPAddress.TryParse(ipAddressStr, out ipAddress)){
                    try{
                        // ホスト名から(IPv4の)IPアドレスを取得
                        var list = Dns.GetHostEntry(ipAddressStr).AddressList;
                        ipAddress = list.First(a => a.AddressFamily == AddressFamily.InterNetwork);
                    }catch{
                        showErrorMessage("IPアドレスまたはホスト名が不正です");
                        return;
                    }
                }
            }

            // ポート番号のチェック
            if (!int.TryParse(textPort.Text, out int port))
            {
                showErrorMessage("ポート番号が不正です");
                return;
            }
            // ソケットを開く
            if (!socket.Open(ipAddress, port)){
                showErrorMessage("ソケットが開けません");
                return;
            }

            // IPアドレスとポート番号を更新 (ここで毎回ファイル保存はしない)
            defaultIpAddress = ipAddressStr;
            defaultPort = port;

            textIpAddress.Enabled = false;
            textPort.Enabled = false;
            buttonOpen.Enabled = false;
            buttonClose.Enabled = true;

            // イベント発行
            if(Opened != null) Opened(this, EventArgs.Empty);

            this.Update(); // 受信が始まるので念のために強制的に表示更新
        }

        // 停止ボタンクリック時の処理
        private void buttonClose_Click(object sender, EventArgs e)
        {
            // ポートを閉じる
            socket.Close();

            textIpAddress.Enabled = true;
            textPort.Enabled = true;
            buttonOpen.Enabled = true;
            buttonClose.Enabled = false;

            // イベント発行
            if (Closed != null) Closed(this, EventArgs.Empty);
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

        // IPアドレスリストのドロップダウン時の処理
        private void listIpAddress_DropDown(object sender, EventArgs e)
        {
            // IPアドレスリストの更新
            updateIpAddressList();
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
            textIpAddress.Items.Clear();
            textIpAddress.Items.AddRange(ipAddressList.ToArray());

            // 既定値を選択
            textIpAddress.SelectedItem = defaultIpAddress;
            // 無ければ0番目の項目を選択
            if ((string)textIpAddress.SelectedItem != defaultIpAddress)
            {
                textIpAddress.SelectedIndex = 0;
            }
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

        // IPアドレスリスト
        ToolStripComboBox textIpAddress;
        // ポート番号ボックス
        ToolStripTextBox textPort;
        // 開始ボタン
        ToolStripButton buttonOpen;
        // 停止ボタン
        ToolStripButton buttonClose;

        // コンポーネントの初期化
        private void InitializeComponent()
        {
            this.textIpAddress = new ToolStripComboBox();
            this.textPort = new ToolStripTextBox();
            this.buttonOpen = new ToolStripButton();
            this.buttonClose = new ToolStripButton();
            this.SuspendLayout();

            var labelIpAddress = new ToolStripLabel("自分のアドレス");
            var labelPort = new ToolStripLabel("ポート番号");

            this.textIpAddress.DropDownStyle = ComboBoxStyle.DropDownList; // 編集不可
            this.textIpAddress.DropDown += listIpAddress_DropDown;
            this.textIpAddress.ToolTipText = "自分のIPアドレスの選択";
            this.textIpAddress.Items.Clear();

            this.textPort.ToolTipText = "ポート番号の指定";
            this.textPort.Text = "";

            this.buttonOpen.Text = "開始";
            this.buttonOpen.ToolTipText = "開始";
            this.buttonOpen.Click += buttonOpen_Click;

            this.buttonClose.Text = "停止";
            this.buttonClose.ToolTipText = "停止";
            this.buttonClose.Click += buttonClose_Click;

            this.Items.Add(labelIpAddress);
            this.Items.Add(textIpAddress);
            this.Items.Add(labelPort);
            this.Items.Add(textPort);
            this.Items.Add(buttonOpen);
            this.Items.Add(buttonClose);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
