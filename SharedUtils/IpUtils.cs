using System.Net;
using System.Net.Sockets;

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

        public static bool ValidateParams(string serverIp, string serverName, out string errorMessage)
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

            errorMessage = "";
            return true;
        }
    }
}
