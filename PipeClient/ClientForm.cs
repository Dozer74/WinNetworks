using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace PipeClient
{
    public partial class ClientForm : Form
    {
        private const string PipeName = "MyPipe";

        private NamedPipeClientStream pipeClient;
        private StreamWriter pipeWriter;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            tbServerName.Text = Dns.GetHostName();
            Thread.Sleep(500);
            btnConnect_Click(null, null);
        }


        private async void btnSend_Click(object sender, EventArgs e)
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

            try
            {
                await pipeWriter.WriteLineAsync(message);
                await pipeWriter.FlushAsync();
            }
            catch
            {
                MessageBox.Show(this, "Не удалось доставить сообщение.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var serverName = tbServerName.Text;

            if (string.IsNullOrEmpty(serverName))
            {
                MessageBox.Show(this, "Необходимо указать имя сервера.",
                    "Не указан сервер",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }

            pipeClient = new NamedPipeClientStream(serverName, PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            try
            {
                pipeClient.Connect(4);
                pipeWriter = new StreamWriter(pipeClient);

                var thread = new Thread(StartServer);
                thread.Start();
            }
            catch
            {
                MessageBox.Show(this, $"Не удалось подключиться к серверу с именем {serverName}. " +
                                      "Проверьте имя сервера и повторите попытку.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }

            btnSend.Enabled = true;
            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;
        }

        private async void StartServer()
        {
            while (true)
            {
                var reader = new StreamReader(pipeClient);

                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        return;
                    }

                    var message = DateTime.Now.ToString("t") + ": " + line;
                    Invoke(new Action(() => lbMessages.Items.Add(message)));
                }
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            if (pipeClient.IsConnected)
            {
                pipeWriter.Close();
                pipeWriter.Dispose();

                pipeClient.Close();
            }

            btnSend.Enabled = btnDisconnect.Enabled = false;
            btnConnect.Enabled = true;
        }
    }
}