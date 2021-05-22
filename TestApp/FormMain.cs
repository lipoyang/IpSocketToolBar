using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var form = new FormTcpServer() { Text = "TCPサーバ" };
            form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new FormTcpClient() { Text = "TCPクライアント" };
            form.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var form = new FormUdpReceiver() { Text = "UDP受信器" };
            form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var form = new FormUdpSender() { Text = "UDP送信器" };
            form.Show();
        }
    }
}
