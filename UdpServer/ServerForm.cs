using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UdpServer
{
    public partial class ServerForm : Form
    {
        private const int LocalPort = 8080;
        private UdpClient udpServer;

        public ServerForm()
        {
            InitializeComponent();
        }

        private async void ServerForm_Load(object sender, EventArgs e)
        {
            udpServer = new UdpClient(LocalPort);

            var thread = new Thread(InitServer);
            thread.Start();

            lbStatus.Text = "Сервер запущен";
        }

        private async void InitServer()
        {
            while (true)
            {
                var result = await udpServer.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);
                lbMessages.Invoke(new Action( () => lbMessages.Items.Add(message)));
            }
        }
    }
}
