using Dama.Data.Interfaces;

namespace Dama.Data.Models
{
    public class Label : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }

        public Label(string name, string userId)
        {
            Name = name;
            UserId = userId;
        }

        public Label(Label label)
        {
            Name = label.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
