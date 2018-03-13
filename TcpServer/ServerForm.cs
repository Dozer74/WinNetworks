using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharedUtils;

namespace TcpServer
{
    public partial class ServerForm : Form
    {
        private const int ServerListenerPort = 8090;
        private const int ClientListenerPort = 8091;
        
        private TcpClient tcpClient;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            tbPcName.Text = Dns.GetHostName();
            var localIp = IpUtils.GetLocalIp(tbPcName.Text);
            tbIpAddress.Text = localIp.ToString();



            var thread = new Thread(InitListener);
            thread.Start();

            lbStatus.Text = "Сервер запущен";
        }

        private async void InitListener()
        {
            var tcpServer = new TcpListener(IPAddress.Any, ServerListenerPort);
            tcpServer.Start(); 
            while (true)
            {
                var client = await tcpServer.AcceptTcpClientAsync();
                var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(clientIp,ClientListenerPort));

                Invoke(new Action(
                    () => {
                        lbConnectionStatus.Text = $"Подключен к {clientIp}";
                        btnSend.Enabled = true;
                    }));

                var stream = client.GetStream();

                while (true)
                {
                    var message = TcpUtils.ReadMessage(stream);
                    if (string.IsNullOrEmpty(message))
                    {
                        Invoke(new Action(
                            () => {
                                lbConnectionStatus.Text = "Не подключен";
                                btnSend.Enabled = false;
                            }));
                        break;
                    }

                    message = DateTime.Now.ToString("t") + ": " + message;
                    lbMessages.Invoke(new Action(() => lbMessages.Items.Add(message)));
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var message = tbMessage.Text;

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show(this,
                    "Сообщение не может быть пустым.",
                    "Некорректные параметры",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!TcpUtils.SendMessage(message, tcpClient.GetStream()))
            {
                MessageBox.Show(this,
                    "Не удалось отправить сообщение: клиент разорвал соединение. Подключение будет закрыто",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
    }
}