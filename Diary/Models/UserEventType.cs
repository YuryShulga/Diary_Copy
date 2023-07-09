using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.Models
{
    class UserEventType
    {
        public int Id_F { get; set; }
        public string Name_F { get; set; }
        public int UserId_F {get; set;}
        public override string ToString()
        {
            return Name_F;
        }
    }
}
