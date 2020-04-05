using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class BestResultForSideSearch
    {
        public List<FlexibleActivityItem> ResultList { get; set; }
        public TimeSpan Break { get; set; }
        public TimeSpan CoverTime => GetCoverTime();

        public BestResultForSideSearch(IEnumerable<FlexibleActivityItem> results, TimeSpan breakValue)
        {
            if (results == null)
                throw new ArgumentNullException("results");

            Break = breakValue;
            ResultList = results.ToList();
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
