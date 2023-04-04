using chat.View;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace chat
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Settings1.Default.Remember == true)
            {
                RememberMeToggleButton.IsChecked = true;
                LoginTextBox.Text = Settings1.Default.Login;
                PassMasked.Password = Settings1.Default.Password;
            }
        }

        bool passView = false;
        bool passView1 = false;
        bool passView2 = false;
        private void ShowHidePassword_Click(object sender, RoutedEventArgs e)
        {
            ShowHidePasswordBox showOrHide = new ShowHidePasswordBox();
            passView = showOrHide.ShowHidePassword(PassUnmasked, PassMasked, passView);

        }
        private void RegShowHidePassword_Click(object sender, RoutedEventArgs e)
        {
            ShowHidePasswordBox showOrHide = new ShowHidePasswordBox();
            passView1 = showOrHide.ShowHidePassword(RegPassUnmasked1, RegPassMasked1, passView1);
        }
        private void RegShowHidePassword2_Click(object sender, RoutedEventArgs e)
        {
            ShowHidePasswordBox showOrHide = new ShowHidePasswordBox();
            passView2 = showOrHide.ShowHidePassword(RegPassUnmasked2, RegPassMasked2, passView2);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TranslateTransform trans = new TranslateTransform();  
            RegGrid.RenderTransform = trans;
            DoubleAnimation aninX = new DoubleAnimation(0,this.Width * 2, TimeSpan.FromSeconds(1));
            trans.BeginAnimation(TranslateTransform.XProperty, aninX);

            TranslateTransform translate2 = new TranslateTransform();
            AuthGrid.RenderTransform = translate2;
            DoubleAnimation aninmationX = new DoubleAnimation(this.Width * 2, 0, TimeSpan.FromSeconds(0.5));
            translate2.BeginAnimation(TranslateTransform.XProperty, aninmationX);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            TranslateTransform translation = new TranslateTransform();
            AuthGrid.RenderTransform = translation;
            DoubleAnimation animationX = new DoubleAnimation(0, this.Width * 2, TimeSpan.FromSeconds(1));
            translation.BeginAnimation(TranslateTransform.XProperty, animationX);

            RegGrid.Visibility= Visibility.Visible;
            TranslateTransform translation2 = new TranslateTransform();
            RegGrid.RenderTransform = translation2;
            DoubleAnimation animationX2 = new DoubleAnimation(this.Width * 2, 0, TimeSpan.FromSeconds(0.5));
            translation2.BeginAnimation(TranslateTransform.XProperty, animationX2);
        }

        private void RegisterUserButton_Click(object sender, RoutedEventArgs e)
        {
            var conn = GetConnection();

            try
            {
                if (RegPassMasked1.Password == RegPassMasked2.Password)
                {
                    conn.Open();

                    bool Flag = false;
                    string CommandText = "select Email from Users";
                    MySqlCommand command = new MySqlCommand(CommandText, conn);
                    MySqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        if (RegEmail.Text == dataReader["Email"].ToString())
                        {
                            MessageBox.Show("На данную почту уже зарегистрирован аккаунт");
                            Flag= true;
                            break;
                        }
                    }
                    dataReader.Close();

                    CommandText = "select Username from Users";
                    command = new MySqlCommand(CommandText, conn);
                    MySqlDataReader dataReader2 = command.ExecuteReader();
                    while (dataReader2.Read())
                    {
                        if (RegUsername.Text == dataReader2["Username"].ToString())
                        {
                            MessageBox.Show("Пользователь с таким именем уже существует");
                            Flag = true;
                            break;
                        }
                    }
                    dataReader2.Close();

                    if (!Flag)
                    {
                        CommandText = $"INSERT INTO Users (`Email`, `Username`, `Password`) VALUES ('{RegEmail.Text}','{RegUsername.Text}','{RegPassMasked1.Password}')";
                        command = new MySqlCommand(CommandText, conn);
                        command.ExecuteNonQuery();
                        TranslateTransform trans = new TranslateTransform();
                        RegGrid.RenderTransform = trans;
                        DoubleAnimation aninX = new DoubleAnimation(0, this.Width * 2, TimeSpan.FromSeconds(1));
                        trans.BeginAnimation(TranslateTransform.XProperty, aninX);

                        TranslateTransform translate2 = new TranslateTransform();
                        AuthGrid.RenderTransform = translate2;
                        DoubleAnimation aninmationX = new DoubleAnimation(this.Width * 2, 0, TimeSpan.FromSeconds(0.5));
                        translate2.BeginAnimation(TranslateTransform.XProperty, aninmationX);
                        MessageBox.Show("Учетная запись успешно создана!");
                    }
                }
                else
                {
                    MessageBox.Show("Пароли не совпадают!");
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            var conn = GetConnection();

            try
            {
                string loginName = LoginTextBox.Text;
                string loginPassword = PassMasked.Password;
                DataTable table = new DataTable();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                conn.Open();
                MySqlCommand command = new MySqlCommand("SELECT * FROM `Users` WHERE `Username` = @uL AND `Password` = @uP", conn);
                command.Parameters.Add("@uL", MySqlDbType.VarChar).Value = loginName;
                command.Parameters.Add("@uP", MySqlDbType.VarChar).Value = loginPassword;
                adapter.SelectCommand= command;
                adapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    Thread thread = new Thread(OpentClient);
                    thread.Start();
                    LoginGrid.Visibility = Visibility.Hidden;
                    CreateUsersPanels();
                    if (RememberMeToggleButton.IsChecked == true)
                    {
                        Settings1.Default.Remember = true;
                        Settings1.Default.Login = LoginTextBox.Text;
                        Settings1.Default.Password = PassMasked.Password;
                    }
                    else
                    {
                        Settings1.Default.Remember = false;
                        Settings1.Default.Login = "";
                        Settings1.Default.Password = "";
                    }
                    Settings1.Default.Save();
                }
                else
                {
                    MessageBox.Show("Пользователя с таким именем не существует");
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }
        }
        TCPClient client;
        public void OpentClient()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                client = new TCPClient();
                client.ConnectToServer(LoginTextBox.Text);
                client.TriggerValueChanged += (obj, ev) => NewMsgTCP(client.TriggerValue);
            }));
        }
        int MyId;
        public void CreateUsersPanels()
        {
            var conn = GetConnection();

            string loginName = LoginTextBox.Text;
            string loginPassword = PassMasked.Password;
            try
            {
                conn.Open();
                //MySqlCommand command = new MySqlCommand("SELECT * FROM `Users` WHERE `Username` != @uL AND `Password` != @uP", conn);
                //command.Parameters.Add("@uL", MySqlDbType.VarChar).Value = loginName;
                //command.Parameters.Add("@uP", MySqlDbType.VarChar).Value = loginPassword;
                string CommandText = "select * from Users";
                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                string[] array = new string[2];
                while (dataReader.Read())
                {
                    if (dataReader["Username"].ToString() == loginName)
                    {
                        MyId = Convert.ToInt32(dataReader["id"]);
                        if (dataReader["Image"].ToString() != "")
                        {
                            UserImage.ImageSource = DecodeImage(dataReader["Image"].ToString());
                        }
                        
                    }
                    else
                    {
                        LeftUserPanel LUP = new LeftUserPanel();
                        LUP.ChatUserName.Content = dataReader["Username"];
                        LUP.ChatUserId.Content = dataReader["id"];
                        UserCardStackPanel.Children.Add(LUP);
                        LUP.TriggerValueChanged += (obj, ev) => OpenNewChat(Convert.ToInt32(LUP.ChatUserId.Content), LUP.ChatUserName.Content.ToString());
                        if (dataReader["Image"].ToString() != "")
                        {
                            LUP.UserImage.ImageSource = DecodeImage(dataReader["Image"].ToString());
                        }
                    }

                }
                dataReader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }
        }
        string FriendLogin;
        string TableName;

        public void OpenNewChat(int FriendId, string LoginFriend)
        {
            FriendLogin = LoginFriend;
            ChatStackPanel.Children.Clear();
            var conn = GetConnection();

            int[] tableNameArray = { MyId, FriendId };
            Array.Sort(tableNameArray);
            TableName = "Chat" + tableNameArray[0].ToString() + "AND" + tableNameArray[1].ToString();
            conn.Open();
            try
            {
                string commandText = "Create table if not exists " + TableName + " " + "(id integer primary key AUTO_INCREMENT, SenderId text, Message text, MessageType text, Time text)";
                MySqlCommand cmd = new MySqlCommand(commandText, conn);
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }

            MySqlDataReader sqlite_datareader;
            MySqlCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM `" + TableName + "`";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while(sqlite_datareader.Read())
            {
                if (sqlite_datareader["SenderId"].ToString() == MyId.ToString())
                {
                    MessageCloud MC = new MessageCloud();
                    MC.HorizontalAlignment = HorizontalAlignment.Right;
                    if (sqlite_datareader["MessageType"].ToString() == "JPG")
                    {
                        MC.ImageMessage.Source = DecodeImage(sqlite_datareader["Message"].ToString());
                        MC.ImageMessage.Visibility = Visibility.Visible;
                    }
                    else if(sqlite_datareader["MessageType"].ToString() == "Text")
                    {
                        MC.MessageTextBlock.Text = sqlite_datareader["Message"].ToString();
                    }
                    MC.MsgBorder.Background = Brushes.LightBlue;
                    MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                    MC.MsgCldTImeLabel.Content = sqlite_datareader["Time"].ToString();
                    ChatStackPanel.Children.Add(MC);
                    ChatScrollViewer.ScrollToBottom();
                }
                else
                {
                    MessageCloud MC = new MessageCloud();
                    MC.DeleteMsg.Visibility = Visibility.Collapsed;
                    MC.HorizontalAlignment = HorizontalAlignment.Left;
                    if (sqlite_datareader["MessageType"].ToString() == "JPG")
                    {
                        MC.ImageMessage.Source = DecodeImage(sqlite_datareader["Message"].ToString());
                        MC.ImageMessage.Visibility = Visibility.Visible;
                    }
                    else if (sqlite_datareader["MessageType"].ToString() == "Text")
                    {
                        MC.MessageTextBlock.Text = sqlite_datareader["Message"].ToString();
                    }
                    MC.MsgBorder.Background = Brushes.LightGray;
                    MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Right;
                    MC.MsgCldTImeLabel.Content = sqlite_datareader["Time"].ToString();
                    ChatStackPanel.Children.Add(MC);
                    ChatScrollViewer.ScrollToBottom();
                }
            }
            conn.Close();
        }

        public void NewMsgTCP(string messageText)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                string [] msgArray = messageText.Split(new char[] { '#' });

            if (msgArray[0] == FriendLogin)
            {
                MessageCloud MC = new MessageCloud();
                MC.DeleteMsg.Visibility = Visibility.Collapsed;
                MC.HorizontalAlignment = HorizontalAlignment.Left;
                MC.MessageTextBlock.Text = msgArray[2];
                MC.MsgBorder.Background = Brushes.LightGray;
                MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Right;
                MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ChatStackPanel.Children.Add(MC);
                ChatScrollViewer.ScrollToBottom();
            }
            }));

        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
                Thread socketCliendThread = new Thread(Sending);
                socketCliendThread.Start();
        }
        private void Sending()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                
                MessageCloud MC = new MessageCloud();
                MC.HorizontalAlignment = HorizontalAlignment.Right;
                MC.MessageTextBlock.Text = MessageTextBox.Text;
                MC.MsgBorder.Background = Brushes.LightBlue;
                MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ChatStackPanel.Children.Add(MC);
                ChatScrollViewer.ScrollToBottom();

                var conn = GetConnection();
                conn.Open();

                string CommandText = "INSERT INTO`" + TableName + "` " + "(SenderId, Message, MessageType, Time) values ('" + MyId + "', '" + MessageTextBox.Text + "', 'Text', '" + MC.MsgCldTImeLabel.Content + "')";
                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                client.SentData(MessageTextBox.Text, LoginTextBox.Text, FriendLogin);
            }));
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow(MyId);
            sw.Show();
        }
        private ImageSource DecodeImage(string byteArray)
        {
            byte[] array = byteArray.Split(';').Select(a => byte.Parse(a)).ToArray();   
            MemoryStream stream = new MemoryStream(array);
            Image image = new Image();
            image.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            return image.Source;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Files|*.jpeg;*.jpg;";
            if (OFD.ShowDialog() == true)
            {
                FileStream FS = new FileStream(OFD.FileName, FileMode.Open, FileAccess.Read);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(OFD.FileName, UriKind.Relative);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.DecodePixelHeight = 256;
                bi.EndInit();

                JpegBitmapEncoder JBE = new JpegBitmapEncoder();
                MemoryStream ms = new MemoryStream();
                StringBuilder sb = new StringBuilder();
                JBE.Frames.Add(BitmapFrame.Create(bi));
                JBE.Save(ms);
                byte[] imgBytes = ms.ToArray();
                foreach (byte b in imgBytes)
                {
                    sb.Append(b).Append(";");
                }
                sb.Remove(sb.Length - 1, 1);

                MessageCloud MC = new MessageCloud();
                MC.HorizontalAlignment = HorizontalAlignment.Right;
                MC.ImageMessage.Source = DecodeImage(sb.ToString());
                MC.ImageMessage.Visibility = Visibility.Visible;
                MC.MsgBorder.Background = Brushes.LightBlue;
                MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ChatStackPanel.Children.Add(MC);
                ChatScrollViewer.ScrollToBottom();

                var conn = GetConnection();
                conn.Open();

                string CommandText = "INSERT INTO`" + TableName + "` " + "(SenderId, Message, MessageType, Time) values ('" + MyId + "', '" + sb + "', 'JPG', '" + MC.MsgCldTImeLabel.Content + "')";

                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void SearchUsers(object sender, RoutedEventArgs e)
        {
            string userName = SearchTextBox.Text;
            try
            {
                UserCardStackPanel.Children.RemoveRange(2, 100);
                var conn = GetConnection();
                conn.Open();

                string CommandText = "SELECT * FROM `Users`";

                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["Username"].ToString() == userName)
                    {
                        LeftUserPanel LUP = new LeftUserPanel();
                        LUP.ChatUserName.Content = dataReader["Username"];
                        LUP.ChatUserId.Content = dataReader["id"];
                        UserCardStackPanel.Children.Add(LUP);
                        LUP.TriggerValueChanged += (obj, ev) => OpenNewChat(Convert.ToInt32(LUP.ChatUserId.Content), LUP.ChatUserName.Content.ToString());
                        if (dataReader["Image"].ToString() != "")
                        {
                            LUP.UserImage.ImageSource = DecodeImage(dataReader["Image"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
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
        private void BackFromChangePass_Click(object sender, RoutedEventArgs e)
        {
            TranslateTransform trans = new TranslateTransform();
            ForgotPasswordGrid.RenderTransform = trans;
            DoubleAnimation aninX = new DoubleAnimation(0, this.Width * 2, TimeSpan.FromSeconds(1));
            trans.BeginAnimation(TranslateTransform.XProperty, aninX);

            AuthGrid.Visibility = Visibility.Visible;
            TranslateTransform translate2 = new TranslateTransform();
            AuthGrid.RenderTransform = translate2;
            DoubleAnimation aninmationX = new DoubleAnimation(this.Width * 2, 0, TimeSpan.FromSeconds(0.5));
            translate2.BeginAnimation(TranslateTransform.XProperty, aninmationX);
        }
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            TranslateTransform translation = new TranslateTransform();
            AuthGrid.RenderTransform = translation;
            DoubleAnimation animationX = new DoubleAnimation(0, this.Width * 2, TimeSpan.FromSeconds(1));
            translation.BeginAnimation(TranslateTransform.XProperty, animationX);

            ForgotPasswordGrid.Visibility = Visibility.Visible;
            TranslateTransform translation2 = new TranslateTransform();
            ForgotPasswordGrid.RenderTransform = translation2;
            DoubleAnimation animationX2 = new DoubleAnimation(this.Width * 2, 0, TimeSpan.FromSeconds(0.5));
            translation2.BeginAnimation(TranslateTransform.XProperty, animationX2);
        }

        private void ChangePassword_click(object sender, RoutedEventArgs e)
        {
            var conn = GetConnection();
            conn.Open();

            if (ForgotMaskedPass.Password == ForgotMaskedPass2.Password)
            {
                try
                {
                    string CommandText = "Update `Users` set Password = '" + ForgotMaskedPass.Password + "' where Username = '" + ForgotLogin.Text + "' and Email = '" + ForgotMail.Text + "'";
                    MySqlCommand command = new MySqlCommand(CommandText, conn);
                    command.ExecuteNonQuery();
                    MessageBox.Show("Пароль обновлен");
                }
                catch (Exception) { MessageBox.Show("Пользователь не найдет"); }
            }
            else
            {
                MessageBox.Show("Пароли не совпадают");
            }
            conn.Close();
        }
    }
};
