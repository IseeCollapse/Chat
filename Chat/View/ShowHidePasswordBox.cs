using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace chat.View
{
    class ShowHidePasswordBox
    {
        public bool ShowHidePassword(TextBox unmaskedPass, PasswordBox maskedPass, bool passView)
        {
            if (passView == false)
            {
                unmaskedPass.Visibility = Visibility.Visible;
                maskedPass.Visibility = Visibility.Hidden;
                unmaskedPass.Text = maskedPass.Password;
                unmaskedPass.Focus();
                unmaskedPass.SelectionStart = unmaskedPass.Text.Length;
                return true;
            }
            else
            {
                maskedPass.Visibility = Visibility.Visible;
                unmaskedPass.Visibility = Visibility.Hidden;
                maskedPass.Password = unmaskedPass.Text;
                maskedPass.Focus();
                SetSelection(maskedPass, maskedPass.Password.Length, 0);
                return false;
            }
        }
        private void SetSelection(PasswordBox passwordBox, int length, int start)
        {
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance |
                BindingFlags.NonPublic).Invoke(passwordBox, new object[] { length, start });
        }
    }
}
