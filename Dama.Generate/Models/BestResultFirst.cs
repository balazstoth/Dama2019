using Dama.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class BestResultFirst
    {
        public List<IDefinedActivity> ResultList { get; set; }

        public TimeSpan Break { get; set; }

        public TimeSpan CoverTime => GetCoverTime();
        
        
        public BestResultFirst(IEnumerable<IDefinedActivity> results, TimeSpan breakValue)
        {
            if (results == null)
                throw new ArgumentNullException("result");

            Break = breakValue;
            ResultList = results.ToList();
        }

        private TimeSpan GetCoverTime()
        {
            int count = ResultList.Count();

            if (ResultList == null || count == 0)
                return new TimeSpan(0,0,0);

            var resultCoverage = ResultList.Sum(r => r.TimeSpan.TotalMinutes);
            var breakCoverage = (count - 1) * Break.TotalMinutes;

            return TimeSpan.FromMinutes(resultCoverage) + TimeSpan.FromMinutes(breakCoverage);
        }
    }
}
