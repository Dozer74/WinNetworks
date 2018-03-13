using System;
using System.Net;
using System.Windows.Forms;
using SharedUtils;

namespace MailspotClient
{
    public partial class ClientForm : Form
    {
        private const string ServerName = "MailspotServer";
        private const string ClientName = "MailspotClient";

        private MailslotServer server;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            tbServerName.Text = Dns.GetHostName();

            server = new MailslotServer(ClientName);

            var serverName = Dns.GetHostName();
            var fullServerName = $@"\\{serverName}\mailspot\{ClientName}";
            lbMessages.Items.Add($"Создан mailspot с именем \"{fullServerName}\"");
        }


        private void btnSend_Click(object sender, EventArgs e)
        {
            var message = tbMessage.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show(this, "Сообщение не должно быть пустым.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }

            var machine = tbServerName.Text;
            if (string.IsNullOrWhiteSpace(machine))
            {
                MessageBox.Show(this, "Необходимо указать имя компьютера, на который будет отправлено сообщение.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }

            var client = new MailslotClient(ServerName, machine);
            client.SendMessage(message);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            while (true)
            {
                var message = server.GetNextMessage();
                if (message == null)
                {
                    break;
                }
                message = DateTime.Now.ToString("t") + ": " + message;
                lbMessages.Items.Add(message);
            }
        }

    }
}