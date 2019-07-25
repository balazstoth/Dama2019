using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Label : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }

        public Label(string name, string userId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("userId");

            Name = name;
            UserId = userId;
        }

        public Label(Label label)
        {
            if (label == null)
                throw new ArgumentNullException("label");

            Name = label.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
