using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharedUtils;

namespace UdpServer
{
    public partial class ServerForm : Form
    {
        private const int ServerListenerPort = 8080;
        private const int ServerSenderPort = 8081;
        private const int ClientListenerPort = 8082;

        private UdpClient udpServer;
        private IPAddress lastClientIp;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            tbPcName.Text = Dns.GetHostName();
            tbIpAddress.Text = IpUtils.GetLocalIp(tbPcName.Text).ToString();

            udpServer = new UdpClient(ServerListenerPort);
            var thread = new Thread(InitListener);
            thread.Start();

            lbStatus.Text = "Сервер запущен";
        }

        private async void InitListener()
        {
            while (true)
            {
                var result = await udpServer.ReceiveAsync();
                lastClientIp = result.RemoteEndPoint.Address;

                var message = DateTime.Now.ToString("t") + ": " + Encoding.UTF8.GetString(result.Buffer);
                lbMessages.Invoke(new Action(() => lbMessages.Items.Add(message)));
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var message = tbMessage.Text;

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show(this,
                    "Сообщение не может быть пустым.");
                return;
            }

            if (lastClientIp == null)
            {
                MessageBox.Show(this,
                    "Сервер ещё не получил ни одного сообщения от клиентов. Некому отправлять ответ.");
                return;
            }

            var endPoint = new IPEndPoint(lastClientIp, ClientListenerPort);
            SendMessage(endPoint, message);
        }


        private async void SendMessage(IPEndPoint clientEndPoint, string message)
        {
            var res = await UdpUtils.SendMessage(clientEndPoint, ServerSenderPort, message);

            if (res != "")
            {
                MessageBox.Show(this,
                    res,
                    "Сообщение не доставлено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}