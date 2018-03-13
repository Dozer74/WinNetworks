using System;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using SharedUtils;
using Sockets = System.Net.Sockets;

namespace UdpClient
{
    public partial class ClientForm : Form
    {
        private const int ServerListenerPort = 8080;
        private const int ClientListenerPort = 8082;
        private const int ClientSenderPort = 8083;

        private Sockets.UdpClient udpServer;


        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            tbPcName.Text = Dns.GetHostName();
            tbIpAddress.Text = IpUtils.GetLocalIp(tbPcName.Text).ToString();
            tbMessage.Focus();

            udpServer = new Sockets.UdpClient(ClientListenerPort);
            var thread = new Thread(InitListener);
            thread.Start();

            lbStatus.Text = "Клиент запущен";
        }

        private async void InitListener()
        {
            while (true)
            {
                var result = await udpServer.ReceiveAsync();

                var message = DateTime.Now.ToString("t") + ": " + Encoding.UTF8.GetString(result.Buffer);

                lbMessages.Invoke(new Action(() => lbMessages.Items.Add(message)));
            }
        }

        private async void SendMessage(IPAddress serverIp, string message)
        {
            var endPoint = new IPEndPoint(serverIp, ServerListenerPort);
            var res = await UdpUtils.SendMessage(endPoint, ClientSenderPort, message);

            if (res != "")
            {
                MessageBox.Show(this,
                    res,
                    "Сообщение не доставлено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var message = tbMessage.Text;
            if (!IpUtils.ValidateParams(tbServerIp.Text, tbServerName.Text, message, out var errorMessage))
            {
                MessageBox.Show(this,
                    errorMessage,
                    "Некорректые параметры",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var serverIp = tbServerIp.Text != ""
                ? IPAddress.Parse(tbServerIp.Text)
                : IpUtils.GetLocalIp(tbServerName.Text);

            SendMessage(serverIp, message);
        }
    }
}