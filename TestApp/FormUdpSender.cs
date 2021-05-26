using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using IpSocketToolBar;

namespace TestApp
{
    // UDP送信器のテスト
    public partial class FormUdpSender : Form
    {
        public FormUdpSender()
        {
            InitializeComponent();
        }

        // 開始処理
        private void Form_Load(object sender, EventArgs e)
        {
            // フォームのLoadイベントで開始処理を呼ぶ
            udpSenderToolStrip.Begin(@"SETTING.INI", this.Text);
            //udpSenderToolStrip.Socket.FixedLocalPort = 4567;
        }

        // 終了処理
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームのFormClosingイベントで終了処理を呼ぶ
            udpSenderToolStrip.End();
        }

        // 送信ボタン
        private void buttonSend_Click(object sender, EventArgs e)
        {
            var server = udpSenderToolStrip.Socket;
            if (!server.IsOpen) return;

            // コマンドラインを送信
            if(textBox1.Text.Length > 0)
            {
                string command = textBox1.Text;
                server.Send(command);
            }
        }
    }
}
