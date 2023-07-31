using Dapper;
using Diary.Models;
using Diary.Sql;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Diary.Logics
{
    internal class UserLogics
    {
        private static readonly SqliteConnection connection = DbLogics.Db;

        /// <summary>
        /// Добавляет в базу данных нового пользователя
        /// </summary>
        /// <param name="nickname">Никнейм добавляемого пользователя</param>
        /// <param name="password">пароль добавляемого пользователя</param>
        /// <returns>пользователь добавлен - true, нет - false</returns>
        public static bool NewUser(string nickname, string password)
        { 
            SqliteParameter nicknameParam = new SqliteParameter();
            nicknameParam.ParameterName = "@nickname";
            nicknameParam.SqliteType = SqliteType.Text;
            nicknameParam.Value = nickname;
            SqliteParameter passwordParam = new SqliteParameter();
            passwordParam.ParameterName = "@password";
            passwordParam.SqliteType = SqliteType.Text;
            passwordParam.Value = password;
            try
            {
                connection.Open();
                SqliteCommand command =
                    new SqliteCommand("INSERT INTO Users_T (Nickname_F, Password_F) VALUES (@nickname, @password);", connection);
                command.Parameters.Add(nicknameParam);
                command.Parameters.Add(passwordParam);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                {
                    return true;
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        /// <summary>
        /// Возвращает текущего пользователя 
        /// </summary>
        /// <returns>Объект класса User</returns>
        public static User? GetCurrentUser()
        {
            try
            {
                connection.Open();
                var result = 
                    connection.Query<User>("SELECT U.Id_F, U.Nickname_F, U.Password_F " +
                    "FROM Users_T AS U " +
                    "JOIN CurrentUser_T AS C ON U.Id_F=C.CurrentUserId_F " +
                    "WHERE C.Id_F=1;");//текущий пользователь находится в таблице CurrentUser_T в ячейке с Id_F=1
                if (result != null)
                {
                    return result.ElementAtOrDefault<User>(0);
                    
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        /// <summary>
        /// Возвращает список всех пользователей из базы данных
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<User>? GetAllUsers()
        {
            IEnumerable<User>? result = null;
            try
            {
                connection.Open();
                result =
                    connection.Query<User>("SELECT * FROM Users_T ");
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// возвращает объект User по имени пользователя 
        /// </summary>
        /// <param name="nickname">имя искомого пользователя</param>
        /// <returns></returns>
        public static User? GetUser(string nickname)
        {
            try
            {
                connection.Open();
                var result =
                    connection.Query<User>("SELECT U.Id_F, U.Nickname_F, U.Password_F " +
                    $"FROM Users_T AS U WHERE Nickname_F='{nickname}';");
                if (result != null)
                {
                    return result.ElementAtOrDefault<User>(0);

                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }


        /// <summary>
        /// Записывает в базу пользователя который является активным в данный момент 
        /// </summary>
        /// <param name="user">Объект User который хотим записать как текущий</param>
        /// <returns></returns>
        public static bool SetCurrentUser(User user)
        {
            int result = 0;
            try
            {
                connection.Open();
                var command = new SqliteCommand($"UPDATE CurrentUser_T SET CurrentUserId_F={user.Id_F} WHERE ID_F=1;", connection);
                result = command.ExecuteNonQuery(); //update data command

            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return result==1;
        }

        /// <summary>
        /// изменяет  User базе 
        /// </summary>
        /// <param name="user">Объект User который хотим записать как текущий</param>
        /// <returns></returns>
        public static bool UpdatetUser(string nickname, string password, int id)
        {
            SqliteParameter nicknameParam = new SqliteParameter();
            nicknameParam.ParameterName = "@nickname";
            nicknameParam.SqliteType = SqliteType.Text;
            nicknameParam.Value = nickname;
            SqliteParameter passwordParam = new SqliteParameter();
            passwordParam.ParameterName = "@password";
            passwordParam.SqliteType = SqliteType.Text;
            passwordParam.Value = password;
            try
            {
                connection.Open();
                SqliteCommand command =
                    new SqliteCommand($"UPDATE Users_T SET Nickname_F=@nickname, Password_F=@password WHERE ID_F ={id};", connection);
                command.Parameters.Add(nicknameParam);
                command.Parameters.Add(passwordParam);
                int result = command.ExecuteNonQuery();
                //MessageBox.Show(result.ToString());
                if (result == 1)
                {
                    //обновляю в базе текущего пользователя
                    SetCurrentUser(new User() { Nickname_F=nickname, Password_F=password, Id_F=id});
                    return true;
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        /// <summary>
        /// возвращает список типов событий для введенного пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<UserEventType>? GetUserEventTypesList(User user)
        {
            try
            {
                connection.Open();
                var result =
                    connection.Query<UserEventType>("SELECT * FROM EventTypes_T AS E JOIN CurrentUser_T AS C ON C.CurrentUserId_F=E.UserId_F;");
                return result;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                
            }
            return null;
        }


        public static bool NewEventType(User user, string eventName)
        {
            SqliteParameter evName = new SqliteParameter();
            evName.ParameterName = "@eventName";
            evName.SqliteType = SqliteType.Text;
            evName.Value = eventName;
            try
            {
                connection.Open();
                SqliteCommand command =
                    new SqliteCommand($"INSERT INTO EventTypes_T (Name_F, UserId_F) VALUES (@eventName, {user.Id_F});", connection);
                command.Parameters.Add(evName);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                {
                    return true;
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        public static UserEventType? GetCurrentUserEventType(string eventTypeName)
        {
            SqliteParameter paramEventTypeName = new SqliteParameter();
            paramEventTypeName.ParameterName = "@EventTypeName";
            paramEventTypeName.SqliteType = SqliteType.Text;
            paramEventTypeName.Value = eventTypeName;
            try
            {
                connection.Open();
                var dictionary = new Dictionary<string, object>
                {
                    { "@myParam", eventTypeName }
                };
                var parameters = new DynamicParameters(dictionary);
                //var parameters = new DynamicParameters({ EventTypeName=eventTypeName })
                var sql = "SELECT E.Id_F, E.Name_F, E.UserId_F FROM EventTypes_T AS E JOIN CurrentUser_T AS C ON C.CurrentUserId_F=E.UserId_F WHERE E.Name_F=@myParam;";
                var result = connection.Query<UserEventType>(sql, parameters);
                //command.Parameters.Add(paramDate);
                //connection.Query<UserEventType>("SELECT E.Id_F, E.Name_F, E.UserId_F FROM EventTypes_T AS E JOIN CurrentUser_T AS C ON C.CurrentUserId_F=E.UserId_F WHERE E.Name_F='Встреча';");

                if (result != null)
                {
                    
                    return result.ElementAtOrDefault<UserEventType>(0);

                }


            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();

            }
            return null;
        }

        public static bool NewRecord(UserEventType eventType, string date, string description)
        {
            SqliteParameter paramDate = new SqliteParameter();
            paramDate.ParameterName = "@date";
            paramDate.SqliteType = SqliteType.Text;
            paramDate.Value = date;
            SqliteParameter paramDescription = new SqliteParameter();
            paramDescription.ParameterName = "@description";
            paramDescription.SqliteType = SqliteType.Text;
            paramDescription.Value = description;
            try
            {
                connection.Open();

                SqliteCommand command =
                    new SqliteCommand($"INSERT INTO Records_T (EventTypeId_F, Date_F, Description_F) VALUES ({eventType.Id_F}, @date, @description);", connection);
                command.Parameters.Add(paramDate);
                command.Parameters.Add(paramDescription);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        /// <summary>
        /// возвращает список всех записей/событий пользователя в указанный день
        /// </summary>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static IEnumerable<UserRecordView>? GetUserRecordsPerDay(User user, DateTime date)
        {

            try
            {
                connection.Open();
                var result =
                    connection.Query<UserRecordView>($"SELECT R.Id_F AS RecordId, E.Name_F AS EventType, R.Description_F AS EventDescription, R.Date_F AS EventDate  FROM Records_T AS R   JOIN EventTypes_T AS E ON E.Id_F=R.EventTypeId_F  WHERE E.UserId_F={user.Id_F} AND R.Date_F='{date.Date.ToString()}';");
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();

            }
            return null;
        }

        public static IEnumerable<UserRecordView>? GetUserRecords(User user)
        {

            try
            {
                connection.Open();
                var result =
                    connection.Query<UserRecordView>($"SELECT R.Id_F AS RecordId, E.Name_F AS EventType, R.Description_F AS EventDescription, R.Date_F AS EventDate  FROM Records_T AS R   JOIN EventTypes_T AS E ON E.Id_F=R.EventTypeId_F  WHERE E.UserId_F={user.Id_F};");
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();

            }
            return null;
        }

        public static bool RemoveRecord(int recordId)
        {
            int result = 0;
            try
            {
                connection.Open();
                var command = new SqliteCommand($"DELETE FROM Records_T AS R WHERE R.Id_F={recordId}", connection);
                result = command.ExecuteNonQuery(); //update data command

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return result == 1;
        }
    }
}
