using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace chat
{
    /// <summary>
    /// Логика взаимодействия для LeftUserPanel.xaml
    /// </summary>
    public partial class LeftUserPanel : UserControl
    {
        public LeftUserPanel()
        {
            InitializeComponent();
        }
        public event EventHandler TriggerValueChanged;

        protected virtual void OnlyChanged()
        {
            TriggerValueChanged.Invoke(this, EventArgs.Empty);
        }
        private string _TriggerValue;
        public string TriggerValue
        {
            get { return _TriggerValue; }
            protected set 
            {
                if(_TriggerValue ==  value) return;
                _TriggerValue = value;
                OnlyChanged();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TriggerValue = "";
           TriggerValue = ChatUserId.ToString();
        }
    }


}
