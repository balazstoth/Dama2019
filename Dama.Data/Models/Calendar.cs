using System;

namespace Dama.Data.Models
{
    public class Calendar
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public Activity Activity { get; set; }

        public Calendar(DateTime date, Activity activity)
        {
            Date = date;
            Activity = activity;
        }
    }
}
