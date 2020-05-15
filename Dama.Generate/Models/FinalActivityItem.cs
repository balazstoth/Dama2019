using Dama.Data.Models;
using System;

namespace Dama.Generate
{
    public class FinalActivityItem
    {
        public Activity Activity { get; set; }

        public DateTime Start { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public FinalActivityItem(Activity activity, DateTime start, TimeSpan timeSpan)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            TimeSpan = timeSpan;
            Activity = activity;
            Start = start;
        }

        public override string ToString()
        {
            return $"{Start.ToShortTimeString()} - {(Start + TimeSpan).ToShortTimeString()}";
        }
    }
}
