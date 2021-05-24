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
    // テスト2
    public partial class FormTcpServer2 : Form
    {
        public FormTcpServer2()
        {
            InitializeComponent();
        }

        // 受信スレッド
        Thread threadRx;
        bool threadRxQuit;

        // 開始処理
        private void Form_Load(object sender, EventArgs e)
        {
            // フォームのLoadイベントで開始処理を呼ぶ
            tcpServerToolStrip.Begin(@"SETTING.INI", this.Text);
            //tcpServerToolStrip.Socket.ReadTimeout = 5000;

            // 受信スレッド開始
            threadRxQuit = false;
            threadRx = new Thread(new ThreadStart(threadRxFunc));
            threadRx.Start();
        }

        // 終了処理
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 受信スレッド終了
            threadRxQuit = true;
            threadRx.Join();

            // フォームのFormClosingイベントで終了処理を呼ぶ
            tcpServerToolStrip.End();
        }

        // 受信スレッド関数
        private void threadRxFunc()
        {
            // シリアルポート
            var server = tcpServerToolStrip.Socket;

            while (!threadRxQuit)
            {
                if (server.IsOpen)
                {
                    // コマンドラインを受信
                    string command = server.WaitString(100);
                    if(command != null)
                    {
                        // テキストボックスに表示
                        this.BeginInvoke((Action)(() => {
                            textBox2.Text += command + "\r\n";
                        }));
                    }
                }
            }
        }

        // 送信ボタン
        private void buttonSend_Click(object sender, EventArgs e)
        {
            // シリアルポート
            var server = tcpServerToolStrip.Socket;
            if (!server.IsOpen) return;

            // コマンドラインを送信
            if(textBox1.Text.Length > 0)
            {
                string command = textBox1.Text;
                server.Send(command);
            }
        }

        // クリアボタン
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
    }
}
