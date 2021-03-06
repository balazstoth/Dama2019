﻿using Dama.Data.Interfaces;
using Dama.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    /// <summary>
    /// Main feature of the software which tries to fill empty slots with the selected acitivities
    /// This class is responsible for finding the best coverage for each free slots
    /// (It uses the optional activities only)
    /// </summary>
    /// <param name="freeTimeList">Collection that contains all the remained free slots during a day</param>
    /// <param name="activities">Collection that contains all the optional activities which needs to be placed</param>
    /// <param name="breakValue">TimeSpan that determines the minimal break value among activities</param>
    public class Generator
    {
        public List<BestResultForMainSearch> BestResultsForFirst { get; set; } //This is a list contains one BestResultFirst element for each freeTime object

        public List<BestResultForSideSearch> BestResultsForLast { get; set; }

        public List<FreeSlot> FreeSlotList { get; set; } //All the freeTime objects

        public List<FreeSlot> FreeSlotsForUndefined { get; set; }

        public List<IDefinedActivity> PriorityListFirst { get; set; } //All the IDefined items

        public List<UndefinedActivity> PriorityListSecond { get; set; } //Only Undefined items

        public TimeSpan Break { get; set; }

        public List<FinalActivityItem> FinalResult { get; set; }

        public Generator(IEnumerable<FreeSlot> freeTimeList, IEnumerable<Activity> activities, TimeSpan breakValue)
        {
            FreeSlotList = freeTimeList.OrderByDescending(x => x.FullTimeSpan).ToList();
            PriorityListFirst = new List<IDefinedActivity>(Enumerable.Union(
                                                        new List<IDefinedActivity>((activities.Where(x => x is FixedActivity).Select(x => x as FixedActivity))),
                                                        new List<IDefinedActivity>((activities.Where(x => x is UnfixedActivity).Select(x => x as UnfixedActivity)))));
            PriorityListFirst = PriorityListFirst.OrderByDescending(x => x.Priority).ToList();
            PriorityListSecond = activities.Where(x => x is UndefinedActivity).Select(x => x as UndefinedActivity).ToList();
            BestResultsForFirst = new List<BestResultForMainSearch>();
            BestResultsForLast = new List<BestResultForSideSearch>();
            FreeSlotsForUndefined = new List<FreeSlot>();
            Break = breakValue;
            FinalResult = Start();
        }

        private List<FinalActivityItem> Start()
        {
            GenerateFirst();
            GenerateFinal();
            return CreateFinalListFromResults();
        }

        #region HelpMethodsFirst
        private List<IDefinedActivity> GetAllBestResults(Tree<FreeSlot> tree)
        {
            List<IDefinedActivity> IDefinedList = new List<IDefinedActivity>();

            for (int i = 0; i < tree.Count; i++)
            {
                if (tree[i].Value.BestResultFirst.ResultList.Any())
                    IDefinedList.AddRange(tree[i].Value.BestResultFirst.ResultList.ToList());
            }

            return IDefinedList;
        }
        private List<IDefinedActivity> GetItemOnPriorityLevel(int priorityLevel)
        {
            return PriorityListFirst
                            .Where(i => i.Priority == priorityLevel)
                            .ToList();
        }
        private TimeSpan GetTimeSpanOfItems(List<IDefinedActivity> activities) //Get the total timeSpan of the specified elements, each item also contains the break value!
        {
            var sum = activities
                            .Where(a => a is UnfixedActivity)
                            .Sum(a => a.TimeSpan.TotalMinutes + Break.TotalMinutes);

            return TimeSpan.FromMinutes(sum);
        }
        private bool Fit(IDefinedActivity activity, FreeSlot frame, List<IDefinedActivity> temporaryStorage)
        {
            //Check if the FixActivity is insertable because of the ufas are in the tmpStorage
            var totalTimeSpan = GetTimeSpanOfItems(temporaryStorage);

            if (activity is FixedActivity)
            {
                var fixedActivity = activity as FixedActivity;

                return  fixedActivity.Start >= frame.Start && 
                        fixedActivity.End <= frame.End && 
                        fixedActivity.Start.Value.TimeOfDay > totalTimeSpan;
            }
            else
            {
                UnfixedActivity unfixedActivity = activity as UnfixedActivity;

                return frame.RemainingTimeSpan >= unfixedActivity.TimeSpan;
            }
        }
        private bool InsertItem(IDefinedActivity activity, FreeSlot freeSlot)
        {
            var isFixed = activity is FixedActivity;
            if (isFixed)
            {
                freeSlot.tmpActivitiesFirst.Add(activity);
            }
            else
            {
                activity.Start = freeSlot.Start + GetTimeSpanOfItems(freeSlot.tmpActivitiesFirst);
                freeSlot.tmpActivitiesFirst.Add(activity);
            }

            return isFixed;
        }
        private void UpdateBestResult(FreeSlot freeSlot)
        {
            BestResultForMainSearch bestResult = new BestResultForMainSearch(new List<IDefinedActivity>(freeSlot.tmpActivitiesFirst), Break);

            if (freeSlot.BestResultFirst.CoverTime < bestResult.CoverTime)
                freeSlot.BestResultFirst = bestResult;
        }
        private void UpdateBestResultAfterDivide(FreeSlot freeSlot, BestResultForMainSearch result)
        {
            BestResultForMainSearch newBestResult = new BestResultForMainSearch(new List<IDefinedActivity>(result.ResultList), Break);

            if (newBestResult.CoverTime > freeSlot.BestResultFirst.CoverTime)
                freeSlot.BestResultFirst = newBestResult;
        }
        private bool IsFinished(FreeSlot freeSlot)
        {
            return freeSlot.BestResultFirst.CoverTime == freeSlot.FullTimeSpan;
        }
        private int GetNextPriority(int priority)
        {
            var activity = PriorityListFirst.Where(a => a.Priority < priority).FirstOrDefault();

            if (activity != null)
                return activity.Priority;

            return -1;
        }
        private void RemoveUsedItems(Tree<FreeSlot> tree)
        {
            List<IDefinedActivity> usedItems = new List<IDefinedActivity>();

            for (int i = 0; i < tree.Count; i++)
                usedItems.AddRange(tree[i].Value.BestResultFirst.ResultList);

            PriorityListFirst = PriorityListFirst.Except(usedItems).ToList();
        }
        private int GetGreatestPriority()
        {
            var activity = PriorityListFirst.FirstOrDefault();

            if (activity == null)
                return -1;

            return activity.Priority;
        }
        private List<FreeSlot> DivideFreeSlotsIntoTwoParts(FreeSlot baseFreeSlots, List<IDefinedActivity> temporaryActivitiesList)
        {
            List<FreeSlot> newFreeSlotList = new List<FreeSlot>();

            var insertedActivity = temporaryActivitiesList
                                                .Where(a => a is FixedActivity)
                                                .FirstOrDefault() as FixedActivity;

            var endOfTemporaryItems = baseFreeSlots.Start + GetTimeSpanOfItems(temporaryActivitiesList);

            if (endOfTemporaryItems < insertedActivity.Start - Break) //There are a freeTime before the fixedActivity
                newFreeSlotList.Add(new FreeSlot(endOfTemporaryItems, insertedActivity.Start.Value - Break, Break));

            if (insertedActivity.End + Break < baseFreeSlots.End)
                newFreeSlotList.Add(new FreeSlot(insertedActivity.End + Break, baseFreeSlots.End, Break));

            return newFreeSlotList;
        }
        private List<FreeSlot> DivideFreeSlot(FreeSlot baseFreeSlot, List<IDefinedActivity> bestResultList, bool first = true)
        {
            if (bestResultList.Count == 0)
                return new List<FreeSlot>() { new FreeSlot(baseFreeSlot.Start, baseFreeSlot.End, Break, first) };

            var unfixedActivities = bestResultList.Where(a => a is UnfixedActivity).Select(a => a as UnfixedActivity);
            var activitiesWithStartValue = bestResultList.Where(a => a is FixedActivity).ToList();
            var dictionary = new Dictionary<DateTime, List<UnfixedActivity>>();

            foreach (var item in unfixedActivities)
            {
                if (!dictionary.ContainsKey(item.Start.GetValueOrDefault()))
                    dictionary[item.Start.GetValueOrDefault()] = new List<UnfixedActivity>();

                dictionary[item.Start.GetValueOrDefault()].Add(item);
            }

            foreach (var key in dictionary.Keys)
            {
                var time = key;
                foreach (var activity in dictionary[key])
                {
                    activity.Start = time;
                    activitiesWithStartValue.Add(activity);
                    time = time + activity.TimeSpan + Break;
                }
            }

            activitiesWithStartValue = activitiesWithStartValue.OrderBy(x => x.Start).ToList();

            var freeTimeList = new List<FreeSlot>();
            DateTime currentTime;

            if (activitiesWithStartValue.First().Start > baseFreeSlot.Start)
                currentTime = baseFreeSlot.Start;
            else
                currentTime = activitiesWithStartValue.First().Start.GetValueOrDefault() + activitiesWithStartValue.First().TimeSpan + Break;

            foreach (var result in activitiesWithStartValue)
            {
                if ((result.Start - Break) > currentTime)
                    freeTimeList.Add(new FreeSlot(currentTime, result.Start.GetValueOrDefault() - Break, Break, first));

                currentTime = result.Start.GetValueOrDefault() + result.TimeSpan + Break;

                if (currentTime >= baseFreeSlot.End)
                    break;
            }

            if (currentTime < baseFreeSlot.End)
                freeTimeList.Add(new FreeSlot(currentTime, baseFreeSlot.End, Break, first));

            return freeTimeList;
        }
        private void SaveBestResults(Tree<FreeSlot> tree)
        {
            for (int i = 0; i < tree.Count; i++)
            {
                if (tree[i].Value.BestResultFirst.ResultList.Any())
                    BestResultsForFirst.Add(tree[i].Value.BestResultFirst);
            }
        }
        private IEnumerable<IDefinedActivity> GetRemainingItems(List<IDefinedActivity> sourceList, List<IDefinedActivity> temporaryList)
        {
            int index = sourceList.IndexOf(temporaryList.Last()) + 1;

            for (int i = index; i < sourceList.Count; i++)
                yield return sourceList[i];
        }
        private List<IDefinedActivity> GetRemainingItemsFix(List<IDefinedActivity> sourceActivities, List<IDefinedActivity> temporaryActivities)
        {
            //Is comparer required?
            var result = sourceActivities.Except(temporaryActivities);
            var list = new List<IDefinedActivity>(result);

            return list;
        }
        #endregion

        #region PreSelection
        private void GenerateFirst() //For fixed and unfixed filling (Pre)
        {
            var greatestPriority = GetGreatestPriority();

            if (greatestPriority == -1)
                return;

            foreach (var freeSlot in FreeSlotList) //Iterate through all the freeTimes, starting with the largest one
            {
                var result = SearchFirst(greatestPriority, freeSlot);
                SaveBestResults(result);

                foreach (var bestResult in BestResultsForFirst)
                    bestResult.ResultList = bestResult.ResultList.OrderBy(a => a.Start).ToList();

                RemoveUsedItems(result);
                FreeSlotsForUndefined.AddRange(DivideFreeSlot(freeSlot, GetAllBestResults(result), false));
            }
        }
        private Tree<FreeSlot> SearchFirst(int priority, FreeSlot freeSlot) //Seach the best option for a freeTime section
        {
            List<IDefinedActivity> selectedActivities;
            var tree = new Tree<FreeSlot>(priority, freeSlot);

            for (int i = 0; i < tree.Count; i++)
            {
                selectedActivities = GetItemOnPriorityLevel(priority); //Contains the items which have the highest priority value
                FirstRecursion(selectedActivities, tree[i].Value); //A specified priority level

                priority = GetNextPriority(priority);

                if (priority == -1)
                    break;

                var dividedFreeTimeList = DivideFreeSlot(tree[i].Value, tree[i].Value.BestResultFirst.ResultList.ToList());
                tree[i].Leaves.AddRange(dividedFreeTimeList.Select(fs => new Leaf<FreeSlot>(priority, fs)));
            }

            return tree;
        }
        private bool FirstRecursion(List<IDefinedActivity> sourceActivities, FreeSlot freeSlot, bool remove = true)
        {
            foreach (var activity in sourceActivities)
            {
                if (Fit(activity, freeSlot, freeSlot.tmpActivitiesFirst))
                {
                    bool isFixed = InsertItem(activity, freeSlot);
                    UpdateBestResult(freeSlot);

                    if (IsFinished(freeSlot))
                        return true;
                    
                    if (isFixed)
                    {
                        var dividedTimes = DivideFreeSlotsIntoTwoParts(freeSlot, freeSlot.tmpActivitiesFirst);
                        var tmpBestResult = new BestResultForMainSearch(new List<IDefinedActivity>(), Break);
                        tmpBestResult.ResultList.AddRange(freeSlot.tmpActivitiesFirst);

                        foreach (var time in dividedTimes)
                        {
                            List<IDefinedActivity> saved = freeSlot.tmpActivitiesFirst;

                            FirstRecursion(GetRemainingItemsFix(sourceActivities, freeSlot.tmpActivitiesFirst).ToList(), time, true);
                            tmpBestResult.ResultList.AddRange(time.BestResultFirst.ResultList);
                            freeSlot.tmpActivitiesFirst = saved.Concat(time.BestResultFirst.ResultList).ToList();
                        }

                        UpdateBestResultAfterDivide(freeSlot, tmpBestResult);
                    }
                    else
                    {
                        var saved = freeSlot.tmpActivitiesFirst;

                        if (FirstRecursion(GetRemainingItems(sourceActivities, freeSlot.tmpActivitiesFirst).ToList(), freeSlot, false))
                            return true;
                        
                        freeSlot.tmpActivitiesFirst = saved;
                    }

                    if (remove)
                        freeSlot.tmpActivitiesFirst.Clear();
                    else
                        freeSlot.tmpActivitiesFirst.RemoveAt(freeSlot.tmpActivitiesFirst.Count - 1);
                }
            }
            return false;
        }
        #endregion

        #region FinalSelection
        private List<FinalActivityItem> CreateFinalListFromResults()
        {
            var finalList = new List<FinalActivityItem>();

            foreach (var result in BestResultsForFirst)
            {
                foreach (var activity in result.ResultList)
                {
                    var finalItem = new FinalActivityItem(activity as Activity, activity.Start.GetValueOrDefault(), activity.TimeSpan);
                    finalList.Add(finalItem);
                }
            }

            foreach (var result in BestResultsForLast)
            {
                foreach (var flexibleItem in result.ResultList)
                {
                    finalList.Add(new FinalActivityItem(flexibleItem.UndefinedActivity, flexibleItem.Start, flexibleItem.TimeSpan));
                }
            }

            return finalList.OrderBy(x => x.Start).ToList();
        }
        private bool Fit(FreeSlot freeSlot, UndefinedActivity undefinedActivity)
        {
            return freeSlot.RemainingTimeSpan >= TimeSpan.FromMinutes(undefinedActivity.MinimumTime);
        }
        private bool IsCoverFull(FreeSlot freeSlot, UndefinedActivity undefinedActivity)
        {
            return Fit(freeSlot, undefinedActivity) && 
                   freeSlot.RemainingTimeSpan <= TimeSpan.FromMinutes(undefinedActivity.MaximumTime);
        }
        private TimeSpan GetUndefinedActivityTimeSpan(FreeSlot freeSlot)
        {
            var sum = freeSlot.BestResultLast.ResultList.Sum(x => x.TimeSpan.TotalMinutes + freeSlot.Break.TotalMinutes);
            return TimeSpan.FromMinutes(sum);
        }
        private void GenerateFinal()
        {
            FreeSlotsForUndefined = FreeSlotsForUndefined.OrderByDescending(x => x.FullTimeSpan).ToList();

            foreach (var freeSlot in FreeSlotsForUndefined)
            {
                SearchLast(freeSlot);

                if (freeSlot.BestResultLast.ResultList.Count() > 0)
                    BestResultsForLast.Add(freeSlot.BestResultLast);
            }
        }
        private void SearchLast(FreeSlot freeSlot)
        {
            PriorityListSecond = PriorityListSecond
                                        .OrderByDescending(u => u.MaximumTime)
                                        .ThenByDescending(u => u.MinimumTime)
                                        .ToList();

            foreach (var undefinedActivity in PriorityListSecond)
            {
                if (IsCoverFull(freeSlot, undefinedActivity))
                {
                    var newItem = new FlexibleActivityItem(undefinedActivity, freeSlot.Start, freeSlot.End);
                    freeSlot.BestResultLast.ResultList.Add(newItem);
                    break;
                }
                else
                {
                    if (Fit(freeSlot, undefinedActivity))
                    {
                        var newStart = freeSlot.Start;
                        var newItem = new FlexibleActivityItem(undefinedActivity, newStart, newStart + TimeSpan.FromMinutes(undefinedActivity.MaximumTime));
                        freeSlot.BestResultLast.ResultList.Add(newItem);
                        freeSlot.Start = newStart + TimeSpan.FromMinutes(undefinedActivity.MaximumTime) + Break;
                    }
                }
            }

            foreach (var result in freeSlot.BestResultLast.ResultList)
            {
                PriorityListSecond.Remove(result.UndefinedActivity);
            }
        }
        #endregion
    }
}
