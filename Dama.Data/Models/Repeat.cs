using Dama.Data.Enums;
using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Repeat : IEntity
    {
        public int Id { get; set; }
        public RepeatPeriod RepeatPeriod { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDay { get; set; }

        public Repeat(DateTime startDay, DateTime endDay, RepeatPeriod repeatPeriod)
        {
            StartDay = startDay;
            EndDate = endDay;
            RepeatPeriod = repeatPeriod;
        }

        public Repeat(RepeatPeriod repeatPeriod, DateTime endDay)
        {
            EndDate = endDay;
            RepeatPeriod = repeatPeriod;
        }

        public Repeat(Repeat repeat)
        {
            StartDay = repeat.StartDay;
            EndDate = repeat.EndDate;
            RepeatPeriod = repeat.RepeatPeriod;
        }

        public bool IsEventInInterval(Interval interval)
        {
            if (RepeatPeriod == RepeatPeriod.Single)
                return IsDayInInterval(StartDay, interval);

            DateTime startDay = StartDay;

            do
            {
                if (IsDayInInterval(startDay, interval))
                    return true;

                startDay = startDay.AddDays((int)RepeatPeriod);
            } while (startDay <= interval.End && startDay <= EndDate);

            return false;
        }

        bool IsDayInInterval(DateTime day, Interval interval)
        {
            return day >= interval.Start && day <= interval.End;
        }
    }
}
