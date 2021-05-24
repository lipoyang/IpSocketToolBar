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
            var form = new FormTcpServer();
            form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new FormTcpClient();
            form.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var form = new FormUdpReceiver();
            form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var form = new FormUdpSender();
            form.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var formA = new FormTcpClient2();
            formA.Show();
            var formB = new FormTcpServer2();
            formB.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var formA = new FormUdpSender2();
            formA.Show();
            var formB = new FormUdpReceiver2();
            formB.Show();
        }
    }
}
