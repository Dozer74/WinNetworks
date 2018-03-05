using System;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;

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
            tbIpAddress.Text = GetLocalIp(tbPcName.Text);

            lbStatus.Text = "Клиент запущен";
        }

        private string GetLocalIp(string hostName)
        {
            var host = Dns.GetHostEntry(hostName);
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Не удалось получить IP";
        }

        private async void SendMessage(string ipAddress, string message)
        {
            var serverIp = IPAddress.Parse(ipAddress);
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
                    "Во время отправки сообщения возникла непредвиденная ошибка:\n"+ex.Message,
                    "Сообщение не доставлено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                updClient.Close();
            }
        }

        private bool ValidateParams(out string errorMessage)
        {
            if (tbServerIp.Text == "" && tbServerName.Text == "")
            {
                errorMessage = "Необходимо указать имя или ip-адрес сервера";
                return false;
            }

            if (tbServerIp.Text != "")
            {
                if (!IPAddress.TryParse(tbServerIp.Text, out var _))
                {
                    errorMessage = "Указан некорректный ip-адрес сервера";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!ValidateParams(out var errorMessage))
            {
                MessageBox.Show(this,
                    errorMessage,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var serverIp = tbServerIp.Text != ""
                ? tbServerIp.Text
                : GetLocalIp(tbPcName.Text);

            SendMessage(serverIp, tbMessage.Text);
        }
    }
}
