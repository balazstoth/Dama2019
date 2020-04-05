using Dama.Data.Interfaces;
using Dama.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class FreeSlot
    {
        private bool _isFirst;

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan FullTimeSpan { get { return End.TimeOfDay - Start.TimeOfDay; } }
        public TimeSpan Break { get; set; }
        public TimeSpan RemainingTimeSpan => GetRemainingTimeSpan();

        /// <summary>
        /// Properties used for the main search 
        /// </summary>
        public List<IDefinedActivity> tmpActivitiesFirst { get; set; }
        public BestResultForMainSearch BestResultFirst { get; set; }

        /// <summary>
        /// Properties used for the side search 
        /// </summary>
        public List<FlexibleActivityItem> tmpActivitiesLast { get; set; }
        public BestResultForSideSearch BestResultLast { get; set; }

        public FreeSlot(DateTime start, DateTime end, TimeSpan breakValue, bool first = true)
        {
            _isFirst = first;
            Start = start;
            End = end;
            Break = breakValue;

            if (_isFirst)
            {
                tmpActivitiesFirst = new List<IDefinedActivity>();
                BestResultFirst = new BestResultForMainSearch(new List<IDefinedActivity>(), Break);
            }
            else
            {
                tmpActivitiesLast = new List<FlexibleActivityItem>();
                BestResultLast = new BestResultForSideSearch(new List<FlexibleActivityItem>(), Break);
            }
        }

        public override string ToString()
        {
            return $"{Start.TimeOfDay} - {End.TimeOfDay}";
        }

        private TimeSpan GetRemainingTimeSpan()
        {
            if (_isFirst)
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
