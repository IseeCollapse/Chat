using chat.Model;
using chat.View;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace chat
{
    public partial class SettingsWindow : Window
    {
        UserLogic user = new UserLogic();
        private string Username { get; set; }
        private string Email { get; set; }
        
        public SettingsWindow(int MyId)
        {
            InitializeComponent();
            user.myId = MyId;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Files|*.jpeg;*.jpg;";
            if(OFD.ShowDialog() == true)
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

                string CommandText = "Update `Users` set Image = '" + sb + "' where id = '" + user.myId + "'";

                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            user.ChangePassword(ChangePassMasked.Password);
        }

        private void ShowSignInPassword(object sender, RoutedEventArgs e)
        {
            MyAnimation myAnimation= new MyAnimation();
            myAnimation.ShowHidePassword(ChangePassUnmasked, ChangePassMasked, !ChangePassTglBtn.IsChecked.Value);
        }
    }
}
