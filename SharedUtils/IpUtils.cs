using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharedUtils
{
    public static class IpUtils
    {
        public static IPAddress GetLocalIp(string hostName)
        {
            IPHostEntry host;
            try
            {
                host = Dns.GetHostEntry(hostName);
            }
            catch (SocketException)
            {
                return null;
            }

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }

        public static bool ValidateParams(string serverIp, string serverName, string message, out string errorMessage)
        {
            if (serverIp == "" && serverName == "")
            {
                errorMessage = "Необходимо указать имя или ip-адрес сервера";
                return false;
            }

            if (serverIp != "")
            {
                if (!IPAddress.TryParse(serverIp, out var _))
                {
                    errorMessage = "Указан некорректный ip-адрес сервера";
                    return false;
                }
            }

            if (serverName != "")
            {
                if (GetLocalIp(serverName) == null)
                {
                    errorMessage = $"Не удалось получить ip-адрес компьютера с именем {serverName}";
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                errorMessage = "Сообщение не может быть пустым";
                return false;
            }

            errorMessage = "";
            return true;
        }

        public static bool ValidateParams(string serverIp, string serverName, out string errorMessage)
        {
            return ValidateParams(serverIp, serverName, "ignore", out errorMessage);
        }
    }

    public static class UdpUtils
    {
        public static async Task<string> SendMessage(IPEndPoint remoteEndPoint, int localPort, string message)
        {
            var updClient = new UdpClient(localPort);

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await updClient.SendAsync(data, data.Length, remoteEndPoint);
            }
            catch (SocketException)
            {
                return
                    "Не удалось подключиться к удаленному серверу. Проверьте правильность ip-адреса или имени сервера.";
            }
            catch (Exception ex)
            {
                return "Во время отправки сообщения возникла непредвиденная ошибка:\n" + ex.Message;
            }
            finally
            {
                updClient.Close();
            }

            return "";
        }
    }

    public static class TcpUtils
    {
        public static string ReadMessage(NetworkStream stream, int bufferSize = 256)
        {
            byte[] data = new byte[bufferSize];
            var response = new StringBuilder();

            try
            {
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (stream.DataAvailable); // пока данные есть в потоке

                return response.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static bool SendMessage(string message, NetworkStream stream)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                stream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}