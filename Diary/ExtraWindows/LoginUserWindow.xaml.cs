using Diary.Logics;
using Diary.Models;
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
    /// Interaction logic for LoginUserWindow.xaml
    /// </summary>
    public partial class LoginUserWindow : Window
    {
        public LoginUserWindow()
        {
            InitializeComponent();
        }

        private void Button_LoginUser_Click(object sender, RoutedEventArgs e)
        {
            User requiredUser = UserLogics.GetUser(TextBox_UserName.Text);
            if (requiredUser == null)
            {
                MessageBox.Show("Ошибка при подключении к базе данных",
                    "Ошибка", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                if (requiredUser.Password_F == TextBox_Password.Text)
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Неверный пароль",
                        "Ошибка авторизации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    this.TextBox_Password.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
            
        }
    }
}
