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
    // TCPサーバのテスト2
    public partial class FormTcpServer2 : Form
    {
        public FormTcpServer2()
        {
            InitializeComponent();
        }

        // ソケット
        TcpServerSocket socket;

        // 受信パケット数
        int recvPackNum = 0;
        // 正常応答の数
        int sendAckNum = 0;
        // 異常応答の数
        int sendNakNum = 0;

        // 開始処理
        private void Form_Load(object sender, EventArgs e)
        {
            // フォームのLoadイベントで開始処理を呼ぶ
            tcpServerToolStrip.Begin(@"SETTING.INI", this.Text);

            // ソケット
            socket = tcpServerToolStrip.Socket;

            // パケット数カウンタ表示
            updateCounter();
        }

        // 終了処理
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームのFormClosingイベントで終了処理を呼ぶ
            tcpServerToolStrip.End();
        }

        // ソケットが接続したとき
        private void tcpServerToolStrip_Connected(object sender, EventArgs e)
        {
            recvPackNum = 0;
            sendAckNum = 0;
            sendNakNum = 0;
            updateCounter();
        }

        // ACK送信
        private void sendAck()
        {
            // パケット作成
            var packet = new PacketData(1);
            packet.SetByte(0, AsciiCode.ACK);
            // パケット送信
            socket.Send(packet);

            sendAckNum++;
        }

        // NAK送信
        private void sendNak()
        {
            // パケット作成
            var packet = new PacketData(1);
            packet.SetByte(0, AsciiCode.NAK);
            // パケット送信
            socket.Send(packet);

            sendNakNum++;
        }

        // パケットを受信したとき
        private void tcpServerToolStrip_Received(object sender, EventArgs e)
        {
            while (true)
            {
                // パケットを取得
                var packet = socket.GetPacket();
                if (packet == null) break;
                recvPackNum++;

                // パケットを解釈
                int val = 0;
                bool ack = false;
                if(packet.GetChar(0) == 'D') {
                    if (packet.GetHex(1, 2, out val)) {
                        if (val <= 100) {
                            ack = true;
                        }
                    }
                }
                // ACK応答 or NAK応答
                if (ack){
                    sendAck();
                }else{
                    sendNak();
                }
                // 表示更新
                this.BeginInvoke((Action)(() => {
                    if (ack) {
                        progressBar.SetValue(val);
                    }
                    updateCounter();
                }));
            }
        }

        // パケット数表示更新
        private void updateCounter()
        {
            textBox1.Text = recvPackNum.ToString();
            textBox2.Text = sendAckNum.ToString();
            textBox3.Text = sendNakNum.ToString();
        }
    }
}
