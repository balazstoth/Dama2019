using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Milestone : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }

        public Milestone()
        {
        }

        public Milestone(string name, DateTime time)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Name = name;
            Time = time;
        }

        public override string ToString()
        {
            return $"{Name}: {Time}";
        }
    }
}