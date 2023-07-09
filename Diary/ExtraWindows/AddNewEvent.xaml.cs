using Diary.Logics;
using Diary.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for AddNewEvent.xaml
    /// </summary>
    public partial class AddNewEvent : Window
    {
        private MainWindow Owner { get; set; }

        public AddNewEvent(MainWindow owner )//DateTime curentDate)
        {
            Owner = owner;
            //this.SelectedDate = curentDate;
            InitializeComponent();
            MainWindowInitHelper();

        }

        private void MainWindowInitHelper()
        {
            if (Owner.calendar.SelectedDate == null)
            {
                Owner.calendar.SelectedDate = DateTime.Now;
            }
            DateTime selectedDate = Owner.calendar.SelectedDate.Value;
            TextBlock_Date.Text = $"{selectedDate.Day}.{selectedDate.Month}.{selectedDate.Year}";
            var eventTypesList = UserLogics.GetUserEventTypesList(Owner.CurrentUser);
            if (eventTypesList != null)
            {
                ComboBox_EventTypes.ItemsSource= eventTypesList;
                //foreach (UserEventType item in eventTypesList)
                //{
                //    ComboBox_EventTypes.Items.Add(item);
                //}
            }

            
        }

        private void Button_AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_EventTypes.Items != null)
            {
                //проверяю есть ли выбранный-введенный тип события в базе
                bool eventIsExist = false;
                foreach (UserEventType item in ComboBox_EventTypes.Items)
                {
                    if (item.Name_F == ComboBox_EventTypes.Text)
                    {
                        eventIsExist = true;
                        break;
                    }
                }
                if (!eventIsExist)//если введенного типа события нет в базе создаю новую запись в базе
                {
                    var result1 = UserLogics.NewEventType(Owner.CurrentUser, ComboBox_EventTypes.Text);
                    if (!result1)
                    {
                        MessageBox.Show("Новый тип события не добавлен. База пользователей не загрузилась!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                //добавляю запись в базу
                UserEventType tempTipe = UserLogics.GetCurrentUserEventType(ComboBox_EventTypes.Text);
                if (tempTipe==null)
                {
                    MessageBox.Show("База типов событий не загрузилась!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result =UserLogics.NewRecord(tempTipe, ((DateTime)Owner.calendar.SelectedDate).Date.ToString(), TextBox_EventComment.Text);
                if (!result)
                {
                    MessageBox.Show("База записей не загрузилась!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
               
                Close();
            }

        }
    }
}
