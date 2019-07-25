using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Calendar : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public Activity Activity { get; set; }

        public Calendar(DateTime date, Activity activity)
        {
            Date = date;
            Activity = activity ?? throw new ArgumentNullException("activity");
        }
    }
}
