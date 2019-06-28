using Dama.Data.Enums;
using Dama.Data.Interfaces;

namespace Dama.Data.Models
{
    public class Category : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Color Color { get; set; }
        public int Priority { get; set; }
        public string UserId { get; set; }

        public Category(string name, string description, Color color, int priority, string userId)
        {
            Name = name;
            Description = description;
            Color = color;
            Priority = priority;
            UserId = userId;
        }

        public Category()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
