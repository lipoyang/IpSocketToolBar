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
    // UDP受信器のテスト2
    public partial class FormUdpReceiver2 : Form
    {
        public FormUdpReceiver2()
        {
            InitializeComponent();
        }

        // ソケット
        UdpSenderSocket socketS;
        UdpReceiverSocket socketR;

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
            udpReceiverToolStrip.Begin(@"SETTING.INI", this.Text);

            // ソケット
            socketR = udpReceiverToolStrip.Socket;
            socketS = new UdpSenderSocket();

            // パケット数カウンタ表示
            updateCounter();
        }

        // 終了処理
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームのFormClosingイベントで終了処理を呼ぶ
            udpReceiverToolStrip.End();

            if (socketS.IsOpen) socketS.Close();
        }

        // シリアルポートが開いたとき
        private void serialPortToolStrip_Opened(object sender, EventArgs e)
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
            var packet = new PacketPayload(3);
            packet.SetByte(1, AsciiCode.ACK);
            // パケット送信
            socketS.Send(packet);

            sendAckNum++;
        }

        // NAK送信
        private void sendNak()
        {
            // パケット作成
            var packet = new PacketPayload(3);
            packet.SetByte(1, AsciiCode.NAK);
            // パケット送信
            socketS.Send(packet);

            sendNakNum++;
        }

        // パケットを受信したとき
        private void Receiver_PacketReceived(object sender, EventArgs e)
        {
            while (true)
            {
                // パケットを取得
                var packet = socketR.GetPacket();
                if (packet == null) break;
                recvPackNum++;

                // 送信元に返信する設定
                if (socketS.IsOpen) socketS.Close();
                socketS.Open(socketR.RemoteAddress, socketR.RemotePort);

                // パケットを解釈
                bool ack = false;
                if (packet.GetHex(1, 2, out int val)){
                    if (val <= 100){
                        ack = true;
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
                    if (ack){
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
