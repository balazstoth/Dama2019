using Dama.Data.Enums;
using Dama.Data.Interfaces;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    public abstract class Activity : IActivity
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

        public ActivityType ActivityType { get; set; }
        #endregion

        #region Constructors
        public Activity(string name, string description, Color color, CreationType creationType, IEnumerable<Label> labels, Category category, string userId, ActivityType activityType)
        {
            Name = name;
            Description = description;
            Color = color;
            CreationType = creationType;
            LabelCollection = labels;
            Category = category;
            UserId = userId;
            ActivityType = activityType;
        }

        public Activity()
        {
        } 
        #endregion

        public override string ToString()
        {
            return $"Name: {Name}";
        }
    }
}
