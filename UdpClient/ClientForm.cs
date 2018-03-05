using System;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using SharedUtils;

namespace UdpClient
{
    public partial class ClientForm : Form
    {
        private const int ClientPort = 8081;
        private const int ServerPort = 8080;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            tbPcName.Text = Dns.GetHostName();
            tbIpAddress.Text = IpUtils.GetLocalIp(tbPcName.Text).ToString();

            lbStatus.Text = "Клиент запущен";
        }

        private async void SendMessage(IPAddress serverIp, string message)
        {
            var endPoint = new IPEndPoint(serverIp, ServerPort);
            var updClient = new System.Net.Sockets.UdpClient(ClientPort);

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                var res = await updClient.SendAsync(data, data.Length, endPoint);
            }
            catch (SocketException)
            {
                MessageBox.Show(this,
                    "Не удалось подключиться к удаленному серверу. Проверьте правильность ip-адреса или имени сервера.",
                    "Сообщение не доставлено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Во время отправки сообщения возникла непредвиденная ошибка:\n" + ex.Message,
                    "Сообщение не доставлено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                updClient.Close();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
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

            SendMessage(serverIp, tbMessage.Text);
        }
    }
}