using Diary.Logics;
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
using System.Windows.Shapes;

namespace Diary.ExtraWindows
{
    /// <summary>
    /// Interaction logic for ChangeUserProperties.xaml
    /// </summary>
    public partial class ChangeUserProperties : Window
    {
        private int UserId { get; set; }
        public ChangeUserProperties(int userId)
        {

            InitializeComponent();
            UserId = userId;
        }

        private void Button_UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            var result = UserLogics.UpdatetUser(TextBox_UserName.Text, TextBox_Password.Text, UserId);
            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show($"Пользователь с именем \"{this.TextBox_UserName.Text}\" уже зарегистрирован");
                this.TextBox_UserName.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
