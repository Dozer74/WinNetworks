using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.IO.Pipes;

namespace PipeServer
{
    public partial class ServerForm : Form
    {
        private const string PipeName = "MyPipe";

        public ServerForm()
        {
            InitializeComponent();
        }

        private StreamWriter senderWriter;

        private void ServerForm_Load(object sender, EventArgs e)
        {
            var thread = new Thread(StartServer);
            thread.Start();

            var serverName = Dns.GetHostName();
            var fullPipeName = $@"\\{serverName}\pipe\{PipeName}";
            lbMessages.Items.Add($"Создан pipe с именем \"{fullPipeName}\"");
        }

        private async void StartServer()
        {
            while (true)
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                server.WaitForConnection();
                var reader = new StreamReader(server);
                
                senderWriter = new StreamWriter(server);

                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        server.Disconnect();
                        server.Dispose();
                        break;
                    }

                    var message = DateTime.Now.ToString("t") + ": " + line;
                    Invoke(new Action(() => lbMessages.Items.Add(message)));
                }
            }
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

            await senderWriter.WriteLineAsync(message);
            await senderWriter.FlushAsync();
        }

    }
}