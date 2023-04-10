using chat.Model;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VipMessageBox.MessageBox;

namespace chat
{

    public partial class MessageCloud : UserControl
    {
        string TableName;
        string Id;

        public MessageCloud(string tableName, string id)
        {
            InitializeComponent();
            TableName = tableName;
            Id = id;
        }

        private void DeleteMsg_Click(object sender, RoutedEventArgs e)
        {
            var conn = GetConnection();
            conn.Open();

            string CommandText = "delete from " + TableName + " where id ='" + Id + "'";
            MySqlCommand command = new MySqlCommand(CommandText, conn);
            command.ExecuteNonQuery();
            conn.Close();
            this.Visibility = Visibility.Collapsed;
        }

        private MySqlConnection GetConnection()
        {
            string host = "37.140.192.192";
            int port = 3306;
            string database = "u1957149_messagerdb";
            string username = "u1957149_iseecol";
            string password = "IseeCollapse090801";

            string connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User id=" + username + ";password=" + password + ";CharSet=utf8";

            MySqlConnection connection = new MySqlConnection(connString);
            return connection;
        }

        private void OpenContextMenu(object sender, MouseButtonEventArgs e)
        {
            DeleteMCG.Visibility = Visibility.Visible;
            EditMSG.Visibility = Visibility.Visible;
        }

        private void EditMsg_Click(object sender, RoutedEventArgs e)
        {
            string editMessage = InputBoxVIP.Show(null,"Введите изменения");

            var conn = GetConnection();
            conn.Open();
            string CommandText = "update `" + TableName + "` set Message = '" + editMessage + "'" +  " where id = '" + Id + "'";
            MySqlCommand command = new MySqlCommand(CommandText, conn);
            command.ExecuteNonQuery();
            conn.Close();
            MessageTextBlock.Text = editMessage;
        }
    }
}
