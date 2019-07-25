using Dama.Data.Models;
using System;

namespace Dama.Generate
{
    public class FinalItem
    {
        public Activity Activity { get; set; }

        public DateTime Start { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public FinalItem(Activity activity, DateTime start, TimeSpan timeSpan)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            TimeSpan = timeSpan;
            Activity = activity;
            Start = start;
        }
    }
}
