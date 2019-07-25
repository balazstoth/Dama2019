using Dama.Data.Enums;
using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Category : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Color Color { get; set; }

        public int Priority { get; set; }

        public string UserId { get; set; }

        public Category()
        {
        }

        public Category(string name, string description, Color color, int priority, string userId)
        {
            CheckArguments(name, priority, userId);

            Name = name;
            Description = description;
            Color = color;
            Priority = priority;
            UserId = userId;
        }

        public override string ToString()
        {
            return Name;
        }

        private void CheckArguments(string name, int priority, string userId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("userId");

            if (priority < 0)
                throw new ArgumentOutOfRangeException("priority");
        }
    }
}
