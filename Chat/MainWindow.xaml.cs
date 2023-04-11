using chat.Model;
using chat.View;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace chat
{
    public partial class MainWindow : Window
    {
        TCPClient client;
        string FriendLogin;
        string TableName;
        UserLogic user = new UserLogic();



        public MainWindow()
        {
            InitializeComponent();
            if (Settings1.Default.Remember == true)
            {
                RememberMeToggleButton.IsChecked = true;
                LoginTextBox.Text = Settings1.Default.Login;
                LoginPassMasked.Password = Settings1.Default.Password;
            }
        }



        private void ShowSignInPassword(object sender, RoutedEventArgs e)
        {
            MyAnimation showOrHide = new MyAnimation();
            showOrHide.ShowHidePassword(PassUnmasked, LoginPassMasked, !SignInTglBtn.IsChecked.Value);

        }
        private void ShowRegisterPassword(object sender, RoutedEventArgs e)
        {
            MyAnimation showOrHide = new MyAnimation();
            showOrHide.ShowHidePassword(RegPassUnmasked1, RegPassMasked1, !RegisterTglBtn.IsChecked.Value);
        }
        private void ShowRegisterPassword2(object sender, RoutedEventArgs e)
        {
            MyAnimation showOrHide = new MyAnimation();
            showOrHide.ShowHidePassword(RegPassUnmasked2, RegPassMasked2, !RegisterTglBtn2.IsChecked.Value);
        }
        private void ShowForgotPassword(object sender, RoutedEventArgs e)
        {
            MyAnimation showOrHide = new MyAnimation();
            showOrHide.ShowHidePassword(ForgotUnmaskedPass, ForgotMaskedPass, !ForgotTglBtn.IsChecked.Value);
        }
        private void ShowForgotPassword2(object sender, RoutedEventArgs e)
        {
            MyAnimation showOrHide = new MyAnimation();
            showOrHide.ShowHidePassword(ForgotUnmaskedPass2, ForgotMaskedPass2, !ForgotTglBtn2.IsChecked.Value);
        }
        


        private void OpenRegisterGrid(object sender, RoutedEventArgs e)
        {
            MyAnimation swapAnimation = new MyAnimation();
            swapAnimation.SwapPlaceAnimation(AuthGrid, RegGrid, this.Width);
        }
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MyAnimation swapAnimation = new MyAnimation();
            swapAnimation.SwapPlaceAnimation(AuthGrid, ForgotPasswordGrid, this.Width);
        }
        private void BackFromRegisterGrid(object sender, RoutedEventArgs e)
        {
            MyAnimation swapAnimation = new MyAnimation();
            swapAnimation.SwapPlaceAnimation(RegGrid, AuthGrid, this.Width);
        }
        private void BackFromForgotPasswordGrid(object sender, RoutedEventArgs e)
        {
            MyAnimation swapAnimation = new MyAnimation();
            swapAnimation.SwapPlaceAnimation(ForgotPasswordGrid, AuthGrid, this.Width);
        }



        private void ChangePassword_click(object sender, RoutedEventArgs e)
        {
            if (ForgotMaskedPass.Password == ForgotMaskedPass2.Password)
            {
                user.ChangePassword(ForgotLogin.Text, ForgotMail.Text, ForgotMaskedPass.Password);
            }
            else
            {
                MessageBox.Show("Пароли не совпадают");
            }
        }
        private void RegisterUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (RegPassMasked1.Password == RegPassMasked2.Password)
            {
                bool regSuccessful;
                regSuccessful = user.RegisterUser(RegEmail.Text, RegUsername.Text, RegPassMasked1.Password);

                if (regSuccessful)
                {
                    MyAnimation enterAnimation = new MyAnimation();
                    enterAnimation.SwapPlaceAnimation(RegGrid, AuthGrid, this.Width);
                }
            }
            else
            {
                MessageBox.Show("Пароли не совпадают!");
            }
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginTextBox.Text;
            string password = LoginPassMasked.Password;
            bool rememberMe = RememberMeToggleButton.IsChecked.Value;

            bool isSignIn = user.SignIn(username, password);
            if (isSignIn)
            {
                LoginGrid.Visibility = Visibility.Hidden;
                user.RememberMe(username, password, rememberMe);
                user.GetId(username);
                UserImage.ImageSource = user.GetMyImage(username);
                CreateUsersPanels(username);



                Thread thread = new Thread(OpentClient);
                thread.Start();
            }
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Thread socketCliendThread = new Thread(Sending);
            socketCliendThread.Start();
        }



        public void CreateUsersPanels(string username)
        {
            var conn = user.GetConnection();
            try
            {
                conn.Open();
                string CommandText = "select * from Users";
                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (dataReader["Username"].ToString() != username)
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
                conn.Close();
                MessageBox.Show("Error" + ex);
            }
        }
        public void OpenNewChat(int FriendId, string LoginFriend)
        {
            FriendLogin = LoginFriend;
            ChatStackPanel.Children.Clear();
            var conn = user.GetConnection();

            int[] tableNameArray = { user.myId, FriendId };
            Array.Sort(tableNameArray);
            TableName = "Chat" + tableNameArray[0].ToString() + "AND" + tableNameArray[1].ToString();
            conn.Open();
            try
            {
                string commandText = "Create table if not exists " + TableName + " " + "(id integer primary key AUTO_INCREMENT, SenderId text, Message text, MessageType text, Time text)";
                MySqlCommand cmd = new MySqlCommand(commandText, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }

            MySqlDataReader sqlite_datareader;
            MySqlCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM `" + TableName + "`";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                if (sqlite_datareader["SenderId"].ToString() == user.myId.ToString())
                {
                    MessageCloud MC = new MessageCloud(TableName, sqlite_datareader["id"].ToString());
                    MC.HorizontalAlignment = HorizontalAlignment.Right;
                    if (sqlite_datareader["MessageType"].ToString() == "JPG")
                    {
                        MC.ImageMessage.Source = DecodeImage(sqlite_datareader["Message"].ToString());
                        MC.ImageMessage.Visibility = Visibility.Visible;
                    }
                    else if (sqlite_datareader["MessageType"].ToString() == "Text")
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
                    MessageCloud MC = new MessageCloud(TableName, sqlite_datareader["id"].ToString());
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
        public void OpentClient()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                client = new TCPClient();
                client.ConnectToServer(LoginTextBox.Text);
                client.TriggerValueChanged += (obj, ev) => NewMsgTCP(client.TriggerValue);
            }));
        }
        private void NewMsgTCP(string messageText)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                string[] msgArray = messageText.Split(new char[] { '#' });

                if (msgArray[0] == FriendLogin)
                {
                    var conn = user.GetConnection();
                    conn.Open();
                    string commandText = "SELECT id FROM " + TableName + " ORDER BY id DESC LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(commandText, conn);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    dataReader.Read();
                    MessageCloud MC = new MessageCloud(TableName, dataReader["id"].ToString());
                    conn.Close();
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
        
        private void Sending()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var conn = user.GetConnection();
                    conn.Open();
                    string commandText = "SELECT count(*) FROM" + TableName;
                    MySqlCommand cmd = new MySqlCommand(commandText, conn);
                    int count = cmd.ExecuteNonQuery();
                    if(count > 0)
                    {
                        commandText = "SELECT id FROM " + TableName + " ORDER BY id DESC LIMIT 1";
                        cmd = new MySqlCommand(commandText, conn);
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        dataReader.Read();
                        MessageCloud MC = new MessageCloud(TableName, dataReader["id"].ToString());
                        dataReader.Close();
                        MC.HorizontalAlignment = HorizontalAlignment.Right;
                        MC.MessageTextBlock.Text = MessageTextBox.Text;
                        MC.MsgBorder.Background = Brushes.LightBlue;
                        MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                        ChatStackPanel.Children.Add(MC);
                        ChatScrollViewer.ScrollToBottom();
                        commandText = "INSERT INTO`" + TableName + "` " + "(SenderId, Message, MessageType, Time) values ('" + user.myId + "', '" + MessageTextBox.Text + "', 'Text', '" + MC.MsgCldTImeLabel.Content + "')";
                        cmd = new MySqlCommand(commandText, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        client.SentData(MessageTextBox.Text, LoginTextBox.Text, FriendLogin);
                        MessageTextBox.Text = "";
                    }
                    else
                    {
                        MessageCloud MC = new MessageCloud(TableName, "1");
                        MC.HorizontalAlignment = HorizontalAlignment.Right;
                        MC.MessageTextBlock.Text = MessageTextBox.Text;
                        MC.MsgBorder.Background = Brushes.LightBlue;
                        MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                        ChatStackPanel.Children.Add(MC);
                        ChatScrollViewer.ScrollToBottom();
                        commandText = "INSERT INTO`" + TableName + "` " + "(SenderId, Message, MessageType, Time) values ('" + user.myId + "', '" + MessageTextBox.Text + "', 'Text', '" + MC.MsgCldTImeLabel.Content + "')";
                        cmd = new MySqlCommand(commandText, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        client.SentData(MessageTextBox.Text, LoginTextBox.Text, FriendLogin);
                        MessageTextBox.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error" + ex);
                }
                
            }));
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow(user.myId);
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

                var conn = user.GetConnection();
                conn.Open();
                string commandText = "SELECT id FROM " + TableName + " ORDER BY id DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(commandText, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                dataReader.Read();
                MessageCloud MC = new MessageCloud(TableName, dataReader["id"].ToString());
                dataReader.Close();
                MC.HorizontalAlignment = HorizontalAlignment.Right;
                MC.ImageMessage.Source = DecodeImage(sb.ToString());
                MC.ImageMessage.Visibility = Visibility.Visible;
                MC.MsgBorder.Background = Brushes.LightBlue;
                MC.MsgCldTImeLabel.HorizontalAlignment = HorizontalAlignment.Left;
                MC.MsgCldTImeLabel.Content = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                ChatStackPanel.Children.Add(MC);
                ChatScrollViewer.ScrollToBottom();

                commandText = "INSERT INTO`" + TableName + "` " + "(SenderId, Message, MessageType, Time) values ('" + user.myId + "', '" + sb + "', 'JPG', '" + MC.MsgCldTImeLabel.Content + "')";

                cmd = new MySqlCommand(commandText, conn);
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
                var conn = user.GetConnection();
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
    }
};
