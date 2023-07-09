using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.Sql
{
    internal static class DbLogics
    {
        public  static SqliteConnection Db = 
            new SqliteConnection(@"Data Source=Sql\diary.db");
    }
    
}
