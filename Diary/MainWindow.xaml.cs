using Dapper;
using Diary.ExtraWindows;
using Diary.Logics;
using Diary.Models;
using Diary.Sql;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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

namespace Diary
{
    enum ListView_SortedBy
    { 
        Date,
        Type, 
        Description
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListView_SortedBy SortFlag { get; set; }

        private SqliteConnection _db;

        private User? _currentUser;
        internal User? CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                if (value == null)
                {
                    MenuItem_User.Header = "Аварийный режим";
                    ErrorDbShow();
                }
                else
                {
                    MenuItem_User.Header = _currentUser?.Nickname_F;
                    if (value.Nickname_F == "Гость")
                    {
                        MemuItem_UserProperties.IsEnabled = false;
                    }
                    else
                    {
                        MemuItem_UserProperties.IsEnabled = true;
                    }
                }

            }
        }
        public MainWindow()
        {
            InitializeComponent();
            WindowInitializeHelper();
        }

        /// <summary>
        /// выполняет действия которые необходимо сделать при старте приложения
        /// </summary>
        private void WindowInitializeHelper()
        {
            CurrentUser = UserLogics.GetCurrentUser();
            _db = DbLogics.Db;


            //создаю список MenuItem-ов для существующих пользователей для смены аккаунта
            var userList = UserLogics.GetAllUsers();
            if (userList != null)
            {
                foreach (User user in userList)
                {
                    if (user.Nickname_F == CurrentUser?.Nickname_F)
                    {
                        AddItemToMenuItem(user.Nickname_F, MenuItem_ChangeUser, Visibility.Collapsed);
                        continue;
                    }

                    AddItemToMenuItem(user.Nickname_F, MenuItem_ChangeUser);
                }
            }
            calendar.SelectedDate = DateTime.Now;
            calendar.Focus();
        }

        /// <summary>
        /// заглушка для обработки ошибки если база не открылась либо запрос не прошел
        /// короче если запрос вернул null
        /// </summary>
        private void ErrorDbShow()
        {
            MessageBox.Show("База пользователей не загрузилась!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        /// <summary>
        /// добавляет в MenuItem подменю с именем nickname и создает обработчик нажатия
        /// </summary>
        /// <param name="nickname"> nickname_F добавляемого пользователя</param>
        /// <param name="menuItem">пункт меню в который добавляем пользователя</param>
        private void AddItemToMenuItem(string nickname, MenuItem menuItem, Visibility menuitemVisibility = Visibility.Visible)
        {
            MenuItem m = new MenuItem();
            m.Header = nickname;
            m.Visibility = menuitemVisibility;
            m.Click += new RoutedEventHandler(MenuChangeUserHelper);
            menuItem.Items.Add(m);
        }

        /// <summary>
        /// обработчик смены пользователя на выбранный
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChangeUserHelper(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).Header.ToString()!="Гость")
            {
                LoginUserWindow loginUserWindow = new LoginUserWindow();
                loginUserWindow.TextBox_UserName.IsEnabled = false;
                loginUserWindow.TextBox_Password.Focus();
                loginUserWindow.Owner = this;
                loginUserWindow.TextBox_UserName.Text = ((MenuItem)sender).Header.ToString();
                var result = loginUserWindow.ShowDialog();
                if (result == false)
                {
                    return;
                }
            }
            //сделать видимыми текущего пользователя MenuItems
            ShowCurrentUserMenuItem();

            //меняю текущего пользователя
            string userName = ((MenuItem)sender).Header.ToString();
            CurrentUser = UserLogics.GetUser(userName);

            //делаю меню текущего пользователя невидимым
            HideCurrentUserMenuItem();

            //записать в базу текущего юзера
            UserLogics.SetCurrentUser(CurrentUser);

            calendar.SelectedDate = DateTime.Now;
            calendar.Focus();
            UpdateListView(new UsRecViewDateComparer());

            
        }

       

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            //DateTime? selectedDate = calendar.SelectedDate;
            //textbox_test.Text += selectedDate.ToString()+"\n";
            //textbox_test.Text += "---------------------\n";
            UpdateListView(new UsRecViewDateComparer());
            
            
            
        }

        //Обработка меню "создать новый аккаунт"
        private void MenuItem_NewUser_Click(object sender, RoutedEventArgs e)
        {
            NewUserWindow newUser = new NewUserWindow();
            newUser.Owner = this;
            var result = newUser.ShowDialog();
            if (result == true) 
            {
                //добавляю "созданного" пользователя в список меню-доступных пользователей
                //делаю его сразу скрытым в меню пользователей
                AddItemToMenuItem(newUser.TextBox_UserName.Text, MenuItem_ChangeUser, Visibility.Collapsed);
                // отображаю "текущего пользователя" в списке меню-доступных пользователей
                ShowCurrentUserMenuItem();

                //делаю текущим пользователем созданного нового пользователя
                int lastMenuItem = MenuItem_ChangeUser.Items.Count - 1;
                CurrentUser = UserLogics.GetUser(((MenuItem)MenuItem_ChangeUser.Items[lastMenuItem]).Header.ToString());


                //сохраняю текущего пользователя в бд
                UserLogics.SetCurrentUser(CurrentUser);

                UpdateListView(new UsRecViewDateComparer());
            }
            
            
        }

        /// <summary>
        /// нахождение одного скрытого меню в MenuItem_ChangeUser.Items и 
        /// присвоение ему Visibility.Visible
        /// </summary>
        private void ShowCurrentUserMenuItem()
        {
            for (int i = 0; i < MenuItem_ChangeUser.Items.Count; i++)
            {
                if (((MenuItem)MenuItem_ChangeUser.Items[i]).Visibility == Visibility.Collapsed)
                {
                    ((MenuItem)MenuItem_ChangeUser.Items[i]).Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        /// <summary>
        /// прячет меню выбора текущего пользователя из  MenuItem_ChangeUser.Items
        /// и присвоение ему Visibility.Collapsed
        /// </summary>
        private void HideCurrentUserMenuItem()
        {
            for (int i = 0; i < MenuItem_ChangeUser.Items.Count; i++)
            {
                if (((MenuItem)MenuItem_ChangeUser.Items[i]).Header.ToString() == CurrentUser.Nickname_F)
                {
                    ((MenuItem)MenuItem_ChangeUser.Items[i]).Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }

        private void MemuItem_UserProperties_Click(object sender, RoutedEventArgs e)
        {
            //создаю окно изменения свойств юзера
            ChangeUserProperties propertyWindow = new ChangeUserProperties(CurrentUser.Id_F);
            propertyWindow.Owner = this;
            propertyWindow.TextBox_UserName.Text = CurrentUser.Nickname_F;
            propertyWindow.TextBox_Password.Text = CurrentUser.Password_F;
            //отображаю окно
            var result = propertyWindow.ShowDialog();
            if (result == true) 
            {
                //обновляю CurrentUser после изменения
                CurrentUser = UserLogics.GetCurrentUser();
                //Обновляю имя пользователя в меню
                for (int i = 0; i < MenuItem_ChangeUser.Items.Count; i++)
                {
                    if (((MenuItem)MenuItem_ChangeUser.Items[i]).Visibility == Visibility.Collapsed)
                    {
                        ((MenuItem)MenuItem_ChangeUser.Items[i]).Header = CurrentUser.Nickname_F;
                        break;
                    }
                }
                UpdateListView(new UsRecViewDateComparer());
            }

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool MainWindowClose()
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?",
                "Выход из приложения",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information,
                MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel=!MainWindowClose();
        }

        private void MenuItem_NewEvent_Click(object sender, RoutedEventArgs e)
        {

            AddNewEvent newEventWindow = new AddNewEvent(this);//calendar.SelectedDate.Value);
            newEventWindow.Owner = this;
            newEventWindow.ShowDialog();
            UpdateListView(new UsRecViewDateComparer());
           
        }   

        /// <summary>
        /// обновляет ListView_Records в соответствии с текущим пользователем
        /// </summary>
        private void UpdateListView(IComparer<UserRecordView> comparer)
        {
            //обновляю записи в ListView после добавления события
            var eventList = UserLogics.GetUserRecordsPerDay(CurrentUser, (DateTime)calendar.SelectedDate);
            //if (eventList != null)
            //{
            //    ListView_Records.ItemsSource = eventList;

            //}

            if (eventList != null)
            {
                List<UserRecordView> ls = eventList.ToList<UserRecordView>();
                ls.Sort(comparer);
                ListView_Records.ItemsSource = ls;
            }
            TextBlock_ListViewDescription.Text =
                $"{(string)TryFindResource("string2")} \"{CurrentUser.Nickname_F}\" на {DateOnly.FromDateTime(Convert.ToDateTime(calendar.SelectedDate))}";
        }

        
        /// <summary>
        /// выводит в ListView все записи текущаго пользователя отсортированые по дате
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ShowAllRecords_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_ShowAllRecords_Helper(new UsRecViewDateComparer());
                
        }

        /// <summary>
        /// обработчик вывода всех записей пользователя отсортированных в зависимости от переданного сортировщика
        /// </summary>
        /// <param name="comparer"> сортировщик, может быть 3х видов : UsRecViewDateComparer, UsRecViewEventComparer, UsRecViewDescriptionComparer</param>
        private void MenuItem_ShowAllRecords_Helper(IComparer<UserRecordView> comparer)
        {
            var eventList = UserLogics.GetUserRecords(CurrentUser);
            if (eventList != null)
            {
                List<UserRecordView> ls = eventList.ToList<UserRecordView>();
                ls.Sort(comparer);
                ListView_Records.ItemsSource = ls;
            }

            TextBlock_ListViewDescription.Text = $"{(string)TryFindResource("string1")} \"{CurrentUser.Nickname_F}\"";
            if (eventList != null)
            {
                ListView_Records.Focus();
            }
            else
            {
                calendar.Focus();
            }
        }

        private void ListView_Records_HeaderClick(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked?.Content?.ToString() == (string)TryFindResource("ListView_HeaderName1"))
            {
                SortFlag = ListView_SortedBy.Date;
            }
            else if (headerClicked?.Content?.ToString() == (string)TryFindResource("ListView_HeaderName2"))
            {
                SortFlag = ListView_SortedBy.Type;
            }
            else if (headerClicked?.Content?.ToString() == (string)TryFindResource("ListView_HeaderName3"))
            {
                SortFlag = ListView_SortedBy.Description;
            }
            ListView_Records_HeaderClickHelper();


            

        }

        private void ListView_Records_HeaderClickHelper()
        {
            if (TextBlock_ListViewDescription.Text == $"{(string)TryFindResource("string1")} \"{CurrentUser?.Nickname_F}\"")
            {
                if (SortFlag == ListView_SortedBy.Date)
                {
                    MenuItem_ShowAllRecords_Helper(new UsRecViewDateComparer());
                    return;
                }
                if (SortFlag == ListView_SortedBy.Type)
                {
                    MenuItem_ShowAllRecords_Helper(new UsRecViewEventComparer());
                    return;
                }
                if (SortFlag == ListView_SortedBy.Description)
                {
                    MenuItem_ShowAllRecords_Helper(new UsRecViewDescriptionComparer());
                    return;
                }
            }
            if (TextBlock_ListViewDescription.Text == $"{(string)TryFindResource("string2")} \"{CurrentUser.Nickname_F}\" на {DateOnly.FromDateTime(Convert.ToDateTime(calendar.SelectedDate))}")
            {
                if (SortFlag == ListView_SortedBy.Date)
                {
                    UpdateListView(new UsRecViewDateComparer());
                    return;
                }
                if (SortFlag == ListView_SortedBy.Type)
                {
                    UpdateListView(new UsRecViewEventComparer());
                    return;
                }
                if (SortFlag == ListView_SortedBy.Description)
                {
                    UpdateListView(new UsRecViewDescriptionComparer());
                    return;
                }
            }
        }

        private void Button_Today_Click(object sender, RoutedEventArgs e)
        {
            calendar.SelectedDate= DateTime.Today;
            calendar.Focus();
        }

        private void MenuItem_DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            var result = UserLogics.RemoveRecord(((UserRecordView)ListView_Records.Items[ListView_Records.SelectedIndex]).RecordId);
            if (result)
            {
                ListView_Records_HeaderClickHelper();
            }
            else
            {
                ErrorDbShow();
            }
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"приложение {(string)TryFindResource("ApplicationName")}\n\n" +
                $"Написал Шульга Ю.Ю.\n\n" +
                $"Ни одно из прав не защищено!",
                $"О приложении",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
