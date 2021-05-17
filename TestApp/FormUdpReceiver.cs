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
    // UDP受信器テスト
    public partial class FormUdpReceiver : Form
    {
        public FormUdpReceiver()
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
            udpReceiverToolStrip.Begin(@"SETTING.INI", this.Text);

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
            udpReceiverToolStrip.End();
        }

        // 受信スレッド関数
        private void threadRxFunc()
        {
            // シリアルポート
            var receiver = udpReceiverToolStrip.Receiver;

            while (!threadRxQuit)
            {
                if (receiver.IsOpen)
                {
                    // コマンドラインを受信
                    string command = receiver.WaitString(100);
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

        // クリアボタン
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
    }
}
