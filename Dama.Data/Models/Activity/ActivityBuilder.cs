using Dama.Data.Enums;
using System.Collections.Generic;

namespace Dama.Data.Models
{
    public abstract class ActivityBuilder<T> where T : class
    {
        protected string name;
        protected string description;
        protected Color color;
        protected CreationType creationType;
        protected IEnumerable<Label> labels;
        protected Category category;
        protected string userId;
        protected ActivityType activityType;

        public T CreateActivity(string name)
        {
            this.name = name;
            return this as T;
        }
        public T WithDescription(string description)
        {
            this.description = description;
            return this as T;
        }
        public T WithColor(Color color)
        {
            this.color = color;
            return this as T;
        }
        public T WithCreationType(CreationType creationType)
        {
            this.creationType = creationType;
            return this as T;
        }
        public T WithLabels(IEnumerable<Label> labels)
        {
            this.labels = labels;
            return this as T;
        }
        public T WithCategory(Category category)
        {
            this.category = category;
            return this as T;
        }
        public T WithUserId(string userId)
        {
            this.userId = userId;
            return this as T;
        }
        public T WithActivityType(ActivityType activityType)
        {
            this.activityType = activityType;
            return this as T;
        }
    }
}
