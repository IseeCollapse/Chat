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
        int myId;
        public SettingsWindow(int MyId)
        {
            InitializeComponent();
            myId = MyId;
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

                string host = "37.140.192.192";
                int port = 3306;
                string database = "u1957149_messagerdb";
                string username = "u1957149_iseecol";
                string password = "IseeCollapse090801";

                string connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User id=" + username + ";password=" + password + ";CharSet=utf8";

                MySqlConnection conn = new MySqlConnection(connString);
                conn.Open();

                string CommandText = "Update `Users` set Image = '" + sb + "' where id = '" + myId + "'";

                MySqlCommand cmd = new MySqlCommand(CommandText, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
