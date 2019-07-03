using Dama.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class AutoFill
    {
        private Generate _generate;

        #region Properties
        public IEnumerable<Activity> OptionalActivities { get; set; }
        public List<FixedActivity> SortedFixedActivities { get; set; }
        public DateTime TimeFrameStart { get; set; }
        public DateTime TimeFrameEnd { get; set; }
        public TimeSpan Break { get; set; }
        public List<FreeTime> FreeTimeList { get; set; } 
        #endregion

        public AutoFill(IEnumerable<FixedActivity> fixedActivities, IEnumerable<Activity> optionalActivities, DateTime start, DateTime end, TimeSpan timeSpan)
        {
            OptionalActivities = optionalActivities;
            TimeFrameStart = start;
            TimeFrameEnd = end;
            Break = timeSpan;
            SortedFixedActivities = SortFixedActivities(fixedActivities);
            Start();
        }

        #region Methods
        public void Start()
        {
            FreeTimeList = GetFreeTimeList();
            _generate = new Generate(FreeTimeList, OptionalActivities, Break);
        }

        private List<FixedActivity> SortFixedActivities(IEnumerable<FixedActivity> fixedActivities)
        {
            List<FixedActivity> resultList = new List<FixedActivity>();
            fixedActivities = fixedActivities.OrderBy(x => x.Start).ToList();

            foreach (var fixedActivity in fixedActivities)
            {
                bool result;

                try
                {
                    result = IsInFrame(fixedActivity);
                }
                catch (ArgumentException)
                {
                    return null;
                }

                if (result)
                    resultList.Add(fixedActivity);
            }

            resultList = MergeItems(resultList);
            return resultList;
        }

        private List<FixedActivity> MergeItems(List<FixedActivity> resultList)
        {
            List<FixedActivity> mergedList = new List<FixedActivity>();

            foreach (var currentActivity in resultList)
            {
                if (mergedList.Count == 0 || mergedList.Last().End < currentActivity.Start)
                {
                    var latest = GetLongestIntersection(currentActivity, resultList);
                    if (latest != null)
                    {
                        FixedActivity merged = currentActivity;
                        merged.End = latest.End;
                        mergedList.Add(merged);
                    }
                    else
                    {
                        if (mergedList.Count == 0 || !(Intersect(mergedList.Last(), currentActivity)))
                            mergedList.Add(currentActivity);
                    }
                }
            }

            return mergedList;
        }

        private void RemoveRange(List<FixedActivity> fixedActivities, List<FixedActivity> list)
        {
            foreach (var fixedActivity in fixedActivities)
                list.Remove(fixedActivity);
        }

        private FixedActivity GetLongestIntersection(FixedActivity fixedActivity, List<FixedActivity> list)
        {
            FixedActivity latest = null;

            foreach (var currentItem in list)
            {
                if (currentItem != fixedActivity && Intersect(fixedActivity, currentItem))
                {
                    if (latest == null)
                    {
                        latest = currentItem;
                    }
                    else
                    {
                        if (latest.End < currentItem.End)
                            latest = currentItem;
                    }
                }
            }

            if (latest != null && latest.End > fixedActivity.End)
            {
                FixedActivity back = GetLongestIntersection(latest, list);
                if (back == null)
                    return latest;

                return back;
            }
            else
            {
                return null;
            }
        }

        private bool IsInFrame(FixedActivity activity)
        {
            if (activity.Start <= TimeFrameStart && activity.End >= TimeFrameEnd)
                throw new ArgumentException(activity.Name);

            return !(activity.End <= TimeFrameStart || activity.Start >= TimeFrameEnd);
        }

        private bool Intersect(FixedActivity first, FixedActivity second)
        {
            return !(first.End < second.Start);
        }

        private List<FreeTime> GetFreeTimeList()
        {
            if (SortedFixedActivities == null)
                return new List<FreeTime>();

            if (SortedFixedActivities.Count == 0)
                return new List<FreeTime>() { new FreeTime(TimeFrameStart, TimeFrameEnd, Break) };

            List<FreeTime> freeTimeList = new List<FreeTime>();
            DateTime currentTime;

            if (SortedFixedActivities.First().Start > TimeFrameStart)
                currentTime = TimeFrameStart;
            else
                currentTime = SortedFixedActivities.First().End + Break;

            foreach (FixedActivity fixedActivity in SortedFixedActivities)
            {
                if ((fixedActivity.Start - Break) > currentTime)
                    freeTimeList.Add(new FreeTime(currentTime, fixedActivity.Start.GetValueOrDefault() - Break, Break));

                currentTime = fixedActivity.End + Break;

                if (currentTime >= TimeFrameEnd)
                    break;
            }

            if (currentTime < TimeFrameEnd)
                freeTimeList.Add(new FreeTime(currentTime, TimeFrameEnd, Break));

            return freeTimeList;
        } 
        #endregion
    }
}
