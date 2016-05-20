using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerSettings
    {
        private static ServerSettings _current;

        private string _serverAddress;

        public string ServerAddress
        {
            get { return _serverAddress; }
        }



        public static ServerSettings Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ServerSettings(); 
                }
                return _current;
            }
        }

        private ServerSettings()
        {
            SetServerAdress();
        }

        private void SetServerAdress()
        {
            string host = Properties.Settings.Default.Host;
            string port = Properties.Settings.Default.Port;
            string serverName = Properties.Settings.Default.ServerName;

            _serverAddress = string.Format("opc.tcp://{0}:{1}/{2}", host, port, serverName);
        }


    }
}
