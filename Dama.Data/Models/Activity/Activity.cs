using Dama.Data.Enums;
using Dama.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    public abstract class Activity : IActivity, IEntity
    {
        #region Properties
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Color Color { get; set; }

        public CreationType CreationType { get; set; }

        public IEnumerable<Label> LabelCollection { get; set; }

        public Category Category { get; set; }

        public string UserId { get; set; }

        public bool BaseActivity { get; set; }

        public ActivityType ActivityType { get; set; }
        #endregion

        #region Constructors
        public Activity(string name, string description, Color color, CreationType creationType, IEnumerable<Label> labels, Category category, string userId, ActivityType activityType, bool baseActivity)
        {
            CheckArguments(name, userId);

            Name = name;
            Description = description;
            Color = color;
            CreationType = creationType;
            LabelCollection = labels ?? new List<Label>();
            Category = category;
            UserId = userId;
            ActivityType = activityType;
            BaseActivity = baseActivity;
        }

        public Activity()
        {
        }
        #endregion

        public override string ToString()
        {
            return $"Name: {Name}";
        }

        private void CheckArguments(string name, string userId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("userId");
        }
    }
}
