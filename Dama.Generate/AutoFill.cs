using Dama.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    /// <summary>
    /// Main feature of the software which tries to fill empty slots with the selected acitivities
    /// This class calculates and merges the remaining free slots which can be filled be Generator class
    /// </summary>
    /// <param name="fixedActivities">Collection that contains all the mandatory activities which are important to be inculded to the current day</param>
    /// <param name="optionalActivities">Collection that contains all the optional activities which are less important to be inculded to the current day</param>
    /// <param name="start">DateTime that determines when the current day 'starts'</param>
    /// <param name="end">DateTime that determines when the current day 'ends'</param>
    /// <param name="timeSpan">TimeSpan that determines the minimal break value among activities</param>
    public class AutoFill
    {
        private Generator _generate;

        public IEnumerable<Activity> OptionalActivities { get; set; }
        public List<FixedActivity> SortedFixedActivities { get; set; }
        public DateTime TimeFrameStart { get; set; }
        public DateTime TimeFrameEnd { get; set; }
        public TimeSpan Break { get; set; }
        public List<FreeSlot> FreeTimeList { get; set; }
        public List<FinalActivityItem> FinalResult { get { return _generate.FinalResult; } }

        public AutoFill(IEnumerable<FixedActivity> fixedActivities, IEnumerable<Activity> optionalActivities, DateTime start, DateTime end, TimeSpan timeSpan)
        {
            if (fixedActivities == null)
                throw new ArgumentNullException("fixedActivities");

            if (optionalActivities == null)
                throw new ArgumentNullException("optionalActivities");

            OptionalActivities = optionalActivities;
            TimeFrameStart = start;
            TimeFrameEnd = end;
            Break = timeSpan;
            SortedFixedActivities = SortFixedActivities(SetCommonDateForActivities(start.Date, fixedActivities.ToList()));
            StartGenerating();
        }

        public void StartGenerating()
        {
            FreeTimeList = GetFreeTimeList();
            _generate = new Generator(FreeTimeList, OptionalActivities, Break);
            _generate.FinalResult = SetValidStartTimeForItems();
            SetStartAndEndValues();
        }

        private List<FixedActivity> SortFixedActivities(IEnumerable<FixedActivity> fixedActivities)
        {
            var resultList = new List<FixedActivity>();
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
            var mergedList = new List<FixedActivity>();

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
                var back = GetLongestIntersection(latest, list);
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

        private List<FreeSlot> GetFreeTimeList()
        {
            if (SortedFixedActivities == null)
                return new List<FreeSlot>();

            if (SortedFixedActivities.Count == 0)
                return new List<FreeSlot>() { new FreeSlot(TimeFrameStart, TimeFrameEnd, Break) };

            List<FreeSlot> freeTimeList = new List<FreeSlot>();
            DateTime currentTime;

            if (SortedFixedActivities.First().Start.Value > TimeFrameStart)
                currentTime = TimeFrameStart;
            else
                currentTime = SortedFixedActivities.First().End + Break;

            foreach (var fixedActivity in SortedFixedActivities)
            {
                if ((fixedActivity.Start.Value - Break) > currentTime)
                    freeTimeList.Add(new FreeSlot(currentTime, fixedActivity.Start.GetValueOrDefault() - Break, Break));

                currentTime = fixedActivity.End + Break;

                if (currentTime >= TimeFrameEnd)
                    break;
            }

            if (currentTime < TimeFrameEnd)
                freeTimeList.Add(new FreeSlot(currentTime, TimeFrameEnd, Break));

            return freeTimeList;
        } 

        private List<FixedActivity> SetCommonDateForActivities(DateTime correct, List<FixedActivity> fixedActivities)
        {
            for (int i = 0; i < fixedActivities.Count; i++)
            {
                fixedActivities[i].Start = correct.Date + fixedActivities[i].Start.Value.TimeOfDay;
                fixedActivities[i].End = correct.Date + fixedActivities[i].End.TimeOfDay;
            }

            return fixedActivities;
        }

        private List<FinalActivityItem> SetValidStartTimeForItems()
        {
            var unfixedActivities = FinalResult.Where(a => a.Activity.ActivityType == Data.Enums.ActivityType.UnfixedActivity).ToList();
            var dictionary = new Dictionary<DateTime, List<FinalActivityItem>>();

            foreach (var item in unfixedActivities)
            {
                if(!dictionary.ContainsKey(item.Start))
                    dictionary[item.Start] = new List<FinalActivityItem>();

                dictionary[item.Start].Add(item);
            }

            var activities = FinalResult.Where(a => a.Activity.ActivityType != Data.Enums.ActivityType.UnfixedActivity).ToList();

            foreach (var key in dictionary.Keys)
            {
                var time = key;
                foreach (var activity in dictionary[key])
                {
                    activity.Start = time;
                    activities.Add(activity);
                    time = time + activity.TimeSpan + Break;
                }
            }

            return activities.OrderBy(a => a.Start).ToList();
        }

        private void SetStartAndEndValues()
        {
            foreach (var item in FinalResult)
            {
                switch (item.Activity.ActivityType)
                {
                    case Data.Enums.ActivityType.UnfixedActivity:
                        (item.Activity as UnfixedActivity).Start = item.Start;
                        (item.Activity as UnfixedActivity).End = item.Start + item.TimeSpan;
                        break;

                    case Data.Enums.ActivityType.UndefinedActivity:
                        (item.Activity as UndefinedActivity).Start = item.Start;
                        (item.Activity as UndefinedActivity).End = item.Start + item.TimeSpan;
                        break;
                }
            }
        }
    }
}
