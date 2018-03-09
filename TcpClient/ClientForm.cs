using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharedUtils;
using Sockets = System.Net.Sockets;

namespace TcpClient
{
    public partial class ClientForm : Form
    {
        private const int ServerListenerPort = 8090;
        private const int ClientListenerPort = 8091;

        private Sockets.TcpListener tcpServer;
        private Sockets.TcpClient tcpSender;
        private Sockets.TcpClient tcpReciever;

        private bool properlyDisconnected;


        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            tbPcName.Text = Dns.GetHostName();
            var localIp = IpUtils.GetLocalIp(tbPcName.Text);
            tbIpAddress.Text = localIp.ToString();
            tbServerIp.Text = localIp.ToString();

            tcpServer = new Sockets.TcpListener(localIp, ClientListenerPort);
            tcpServer.Start();

            var thread = new Thread(InitListener);
            thread.Start();

            lbStatus.Text = "Клиент запущен";
        }
        

        private async void InitListener()
        {
            while (true)
            {
                tcpReciever = await tcpServer.AcceptTcpClientAsync();
                var stream = tcpReciever.GetStream();
                properlyDisconnected = false;

                while (true)
                {
                    var message = TcpUtils.ReadMessage(stream);
                    if (string.IsNullOrEmpty(message))
                    {
                        if (!properlyDisconnected)
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show(this,
                                    "Соединение с сервером потеряно. Подключение будет закрыто",
                                    "Сервер отключен",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Disconnect();
                            }));
                        }
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
            
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show(this,
                    "Сообщение не может быть пустым",
                    "Некорректные параметры",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            TcpUtils.SendMessage(message, tcpSender.GetStream());

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!IpUtils.ValidateParams(tbServerIp.Text, tbServerName.Text, out var errorMessage))
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

            try
            {
                tcpSender = new Sockets.TcpClient();
                tcpSender.Connect(new IPEndPoint(serverIp, ServerListenerPort));
                lbConnectionStatus.Text = $"Подключен к {serverIp}";
                btnConnect.Enabled = false;
                btnSend.Enabled = true;
                btnDisconnect.Enabled = true;
            }
            catch
            {
                MessageBox.Show(this,
                    "Не удалось подключиться к удаленному серверу. Проверьте правильность ip-адреса или имени сервера.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            properlyDisconnected = true;

            try
            {
                if (tcpSender.Connected)
                {
                    tcpSender.GetStream().Close();
                    tcpSender.Close();
                }

                if (tcpReciever.Connected)
                {
                    tcpReciever.GetStream().Close();
                    tcpReciever.Close();
                }
            }
            catch
            {
                // ignored
            }
            
            lbConnectionStatus.Text = "Не подключен";
            btnConnect.Enabled = true;
            btnSend.Enabled = false;
            btnDisconnect.Enabled = false;
        }
    }
}