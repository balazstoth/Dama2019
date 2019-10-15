using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dama.Organizer
{
    class EventSelector
    {
        //Events are selected for the past and the future 3 months

        private readonly string _userId;
        private readonly DateTime _today;

        public EventSelector(string userId, DateTime today)
        {
            _userId = userId;
            _today = today;
        }

        public void QueryActivities()
        {
            //DateTime.Now.Date.DayOfYear;
        }
    }
}
