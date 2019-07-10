using Dama.Data.Enums;
using Dama.Data.Interfaces;

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

        public override string ToString()
        {
            return Name;
        }
    }
}
