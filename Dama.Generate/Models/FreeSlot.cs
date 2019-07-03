using Dama.Data.Interfaces;
using Dama.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class FreeSlot
    {
        private bool isFirst;

        #region Properties
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan FullTimeSpan { get { return End.TimeOfDay - Start.TimeOfDay; } }
        public TimeSpan Break { get; set; }
        public TimeSpan RemainingTimeSpan
        {
            get
            {
                return GetRemainingTimeSpan();
            }
        }

        //First search
        public List<IDefinedActivity> tmpActivitiesFirst { get; set; }
        public BestResultFirst BestResultFirst { get; set; }

        //Last search
        public List<FlexibleItem> tmpActivitiesLast { get; set; }
        public BestResultLast BestResultLast { get; set; }
        #endregion

        //Constructor
        public FreeSlot(DateTime start, DateTime end, TimeSpan breakValue, bool first = true)
        {
            isFirst = first;
            Start = start;
            End = end;
            Break = breakValue;

            if (isFirst)
            {
                tmpActivitiesFirst = new List<IDefinedActivity>();
                BestResultFirst = new BestResultFirst(new List<IDefinedActivity>(), Break);
            }
            else
            {
                tmpActivitiesLast = new List<FlexibleItem>();
                BestResultLast = new BestResultLast(new List<FlexibleItem>(), Break);
            }
        }

        public override string ToString()
        {
            return $"{Start.TimeOfDay} - {End.TimeOfDay}";
        }

        private TimeSpan GetRemainingTimeSpan()
        {
            if (isFirst)
            {
                var sum = tmpActivitiesFirst
                                    .Where(a => a is UnfixedActivity)
                                    .Sum(a => a.TimeSpan.TotalMinutes + Break.TotalMinutes);

                return FullTimeSpan - TimeSpan.FromMinutes(sum);
            }
            else
            {
                var sum = tmpActivitiesLast
                                    .Sum(a => a.TimeSpan.TotalMinutes + Break.TotalMinutes);

                return FullTimeSpan - TimeSpan.FromMinutes(sum);
            }
        }
    }
}
