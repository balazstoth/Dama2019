using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class BestResultLast
    {
        public IEnumerable<FlexibleItem> ResultList { get; set; }
        public TimeSpan Break { get; set; }
        public TimeSpan CoverTime
        {
            get
            {
                return GetCoverTime();
            }
        }

        public BestResultLast(IEnumerable<FlexibleItem> results, TimeSpan breakValue)
        {
            Break = breakValue;
            ResultList = results;
        }

        private TimeSpan GetCoverTime()
        {
            if (ResultList == null || ResultList.Count() == 0)
                return new TimeSpan(0,0,0);

            var sum = ResultList.Sum(fi => fi.TimeSpan.TotalMinutes + Break.TotalMinutes);
            return TimeSpan.FromMinutes(sum) - Break;
        }
    }
}
