using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.Models
{
    internal class User
    {
        public int Id_F { get; set; }
        public string Nickname_F { get; set; }
        public string Password_F { get; set; }

        public override string ToString()
        {
            return $"{Id_F} {Nickname_F} {Password_F}";
        }
    }
}
