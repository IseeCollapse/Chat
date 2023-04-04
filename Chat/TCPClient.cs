using Google.Protobuf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace chat
{
    class TCPClient
    {
        //public void Main()
        //{
        //    Thread NewConnectionThread = new Thread(ConnectToServer);
        //    NewConnectionThread.Start();
        //}
        Socket socket = null;
        public void ConnectToServer(string login)
        {
            try
            {
                //string host = "37.140.192.192";
                //int port = 3306;
                //string database = "u1957149_messagerdb";
                //string username = "u1957149_iseecol";
                //string password = "IseeCollapse090801";

                //string connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User id=" + username + ";password=" + password + ";CharSet=utf8";

                //MySqlConnection conn = new MySqlConnection(connString);
                //conn.Open();

                //string CommandText = "SELECT * FROM IpServer";

                //MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                //MySqlDataReader dataReader = cmd.ExecuteReader();
                //string IpAdress = "";
                //while (dataReader.Read())
                //{
                //    IpAdress = dataReader["IpAdress"].ToString();
                //}
                //conn.Close();
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13400);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                byte[] data = Encoding.Unicode.GetBytes("@" + login);
                socket.Send(data);
                Thread ReceiveDataThread = new Thread(ReceiveData);
                ReceiveDataThread.Start();

            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        public void ReceiveData()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[256];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);
                    if (builder.ToString() == "STOP")
                    {
                        CloseConnection();
                    }
                    TriggerValue = builder.ToString();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            
            
        }
        public void SentData(string message, string from, string to)
        {
            byte[] data = Encoding.Unicode.GetBytes(message + "#" + from + "#" + to);
            socket.Send(data);
        }

        public void CloseConnection()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public event EventHandler TriggerValueChanged;

        protected virtual void OnIpChanged()
        {
            TriggerValueChanged.Invoke(this, EventArgs.Empty);
        }
        private string _TriggerValue;
        public string TriggerValue
        {
            get { return _TriggerValue; }
            protected set
            {
                if (_TriggerValue == value) return;
                _TriggerValue = value;
                OnIpChanged();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
