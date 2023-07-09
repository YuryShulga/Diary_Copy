using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace Diary.Models
{
    internal class UserRecordView 
    {
        public int RecordId { get; set; }
        public string EventType { get; set; }
        public string EventDescription { get; set; }

        private string eventDate;
        public string EventDate 
        {
            get => eventDate;
            set => eventDate=DateOnly.FromDateTime(Convert.ToDateTime(value)).ToString();
        }

        public override string ToString()
        {
            return $"RecordId={RecordId} EventType={EventType} EventDescription={EventDescription} EventDate={EventDate}";
        }
    }

    internal class UsRecViewDateComparer : IComparer<UserRecordView>
    {
        public int Compare(UserRecordView? u1, UserRecordView? u2)
        {
            if (u1 is null || u2 is null)
                throw new ArgumentException("Некорректное значение переданного параметра в UsRecViewDataComparer");
            DateTime date = Convert.ToDateTime(((UserRecordView)u1).EventDate);
            DateTime date1 = Convert.ToDateTime(((UserRecordView)u2).EventDate);

            if (date == date1)
            {
                return 0;
            }
            if (date > date1)
            {
                return 1;
            }
            return -1;
        }
    }

    internal class UsRecViewEventComparer : IComparer<UserRecordView>
    {
        public int Compare(UserRecordView? u1, UserRecordView? u2)
        {
            if (u1 is null || u2 is null)
                throw new ArgumentException("Некорректное значение переданного параметра в UsRecViewDataComparer");

            return string.Compare(u1.EventType, u2.EventType);
        }
    }

    internal class UsRecViewDescriptionComparer   : IComparer<UserRecordView>
    {
        public int Compare(UserRecordView? u1, UserRecordView? u2)
        {
            if (u1 is null || u2 is null)
                throw new ArgumentException("Некорректное значение переданного параметра в UsRecViewDataComparer");

            return string.Compare(u1.EventDescription, u2.EventDescription);
        }
    }
}
