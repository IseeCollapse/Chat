using DocumentFormat.OpenXml.Office.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace chat.View
{
    class MyAnimation
    {
        public void ShowHidePassword(TextBox unmaskedPass, PasswordBox maskedPass, bool passView)
        {
            if (passView == false)
            {
                unmaskedPass.Visibility = Visibility.Visible;
                maskedPass.Visibility = Visibility.Hidden;
                unmaskedPass.Text = maskedPass.Password;
                unmaskedPass.Focus();
                unmaskedPass.SelectionStart = unmaskedPass.Text.Length;
            }
            else
            {
                maskedPass.Visibility = Visibility.Visible;
                unmaskedPass.Visibility = Visibility.Hidden;
                maskedPass.Password = unmaskedPass.Text;
                maskedPass.Focus();
                SetSelection(maskedPass, maskedPass.Password.Length, 0);
            }
        }
        private void SetSelection(PasswordBox passwordBox, int length, int start)
        {
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance |
                BindingFlags.NonPublic).Invoke(passwordBox, new object[] { length, start });
        }

        public void SwapPlaceAnimation(Grid first, Grid second, double width)
        {
            TranslateTransform trans = new TranslateTransform();
            first.RenderTransform = trans;
            DoubleAnimation aninX = new DoubleAnimation(0, width * 2, TimeSpan.FromSeconds(1));
            trans.BeginAnimation(TranslateTransform.XProperty, aninX);

            second.Visibility = Visibility.Visible;
            TranslateTransform translate2 = new TranslateTransform();
            second.RenderTransform = translate2;
            DoubleAnimation aninmationX = new DoubleAnimation(width * 2, 0, TimeSpan.FromSeconds(0.5));
            translate2.BeginAnimation(TranslateTransform.XProperty, aninmationX);
        }
    }

}
