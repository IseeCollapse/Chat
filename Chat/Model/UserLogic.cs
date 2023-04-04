using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace chat.Model
{
    internal class UserLogic
    {
        TCPClient client;
        public int myId { get; private set; }
        string FriendLogin;
        string TableName;

        public MySqlConnection GetConnection()
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

        public void GetId(string username)
        {
            var conn = GetConnection();
            try
            {
                conn.Open();
                string commandText = "SELECT * FROM `Users` WHERE `Username` = " + username;
                MySqlCommand command = new MySqlCommand(commandText, conn);
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    if (dataReader["Username"].ToString() == username)
                    {
                        myId = Convert.ToInt32(dataReader["id"]);
                    }
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show("Error" + ex);
            }
        }
        
        public ImageSource GetMyImage(string username)
        {
            var conn = GetConnection();
            try
            {
                conn.Open();
                string commandText = "SELECT `Image` FROM `Users` WHERE `Username` = " + username;
                MySqlCommand command = new MySqlCommand(commandText, conn);
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    if (dataReader["Image"].ToString() != "")
                    {
                        return DecodeImage(dataReader["Image"].ToString());
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show("Error" + ex);
                return null;
            }
        }

        public void ChangePassword(string login, string email, string newPassword)
        {
            var conn = GetConnection();
            conn.Open();

            string commandText = "SELECT * FROM `Users` WHERE `Username` = " + login + " AND `Email` = " + email;
            MySqlCommand command = new MySqlCommand(commandText, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            DataTable table = new DataTable();
            adapter.SelectCommand = command;
            adapter.Fill(table);
            if (table.Rows.Count == 1)
            {
                commandText = "Update `Users` set Password = '" + newPassword + "' where Username = '" + login + "' and Email = '" + email + "'";
                command = new MySqlCommand(commandText, conn);
                command.ExecuteNonQuery();
                MessageBox.Show("Пароль обновлен");
            }
            else
            {
                MessageBox.Show("Данный пользователь не существует");
            }
            conn.Close();
        }

        public bool RegisterUser(string regMail, string regUsername, string regPassword)
        {
            var conn = GetConnection();
            try
            {
                conn.Open();
                bool Flag = false;
                string CommandText = "select Email from Users";
                MySqlCommand command = new MySqlCommand(CommandText, conn);
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    if (regMail == dataReader["Email"].ToString())
                    {
                        MessageBox.Show("На данную почту уже зарегистрирован аккаунт");
                        Flag = true;
                        break;
                    }
                }
                dataReader.Close();

                CommandText = "select Username from Users";
                command = new MySqlCommand(CommandText, conn);
                MySqlDataReader dataReader2 = command.ExecuteReader();
                while (dataReader2.Read())
                {
                    if (regUsername == dataReader2["Username"].ToString())
                    {
                        MessageBox.Show("Пользователь с таким именем уже существует");
                        Flag = true;
                        break;
                    }
                }
                dataReader2.Close();

                if (!Flag)
                {
                    CommandText = $"INSERT INTO Users (`Email`, `Username`, `Password`) VALUES ('{regMail}','{regUsername}','{regPassword}')";
                    command = new MySqlCommand(CommandText, conn);
                    command.ExecuteNonQuery();
                    TranslateTransform trans = new TranslateTransform();
                    MessageBox.Show("Учетная запись успешно создана!");
                    conn.Close();
                    return true;
                }
                conn.Close();
                return false;
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show("Error" + ex);
                return false;
            }
        }

        public bool SignIn(string username, string password)
        {
            var conn = GetConnection();
            try
            {
                conn.Open();
                DataTable table = new DataTable();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand("SELECT * FROM `Users` WHERE `Username` = @uL AND `Password` = @uP", conn);
                command.Parameters.Add("@uL", MySqlDbType.VarChar).Value = username;
                command.Parameters.Add("@uP", MySqlDbType.VarChar).Value = password;
                adapter.SelectCommand = command;
                adapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    conn.Close();
                    return true;
                }
                else
                {
                    MessageBox.Show("Пользователя с таким именем не существует");
                    conn.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show("Error" + ex);
                return false;
            }
        }

        public void RememberMe(string username, string password ,bool rememberMe)
        {
            if (rememberMe == true)
            {
                Settings1.Default.Remember = true;
                Settings1.Default.Login = username;
                Settings1.Default.Password = password;
            }
            else
            {
                Settings1.Default.Remember = false;
                Settings1.Default.Login = "";
                Settings1.Default.Password = "";
            }
            Settings1.Default.Save();
        }

        private ImageSource DecodeImage(string byteArray)
        {
            byte[] array = byteArray.Split(';').Select(a => byte.Parse(a)).ToArray();
            MemoryStream stream = new MemoryStream(array);
            Image image = new Image();
            image.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            return image.Source;
        }

    }
}
