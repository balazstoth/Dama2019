using Dama.Data.Enums;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    abstract class ActivityBuilder
    {
        protected string name;
        protected string description;
        protected Color color;
        protected CreationType creationType;
        protected IEnumerable<Label> labels;
        protected Category category;
        protected string userId;
        protected ActivityType activityType;

        public ActivityBuilder CreateActivity(string name)
        {
            this.name = name;
            return this;
        }
        public ActivityBuilder WithDescription(string description)
        {
            this.description = description;
            return this;
        }
        public ActivityBuilder WithColor(Color color)
        {
            this.color = color;
            return this;
        }
        public ActivityBuilder WithCreationType(CreationType creationType)
        {
            this.creationType = creationType;
            return this;
        }
        public ActivityBuilder WithLabels(IEnumerable<Label> labels)
        {
            this.labels = labels;
            return this;
        }
        public ActivityBuilder WithCategory(Category category)
        {
            this.category = category;
            return this;
        }
        public ActivityBuilder WithUserId(string userId)
        {
            this.userId = userId;
            return this;
        }
        public ActivityBuilder WithActivityType(ActivityType activityType)
        {
            this.activityType = activityType;
            return this;
        }
    }
}
