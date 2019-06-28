using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Milestone : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }

        public Milestone(string name, DateTime time)
        {
            Name = name;
            Time = time;
        }

        public Milestone()
        {
        }

        public override string ToString()
        {
            return $"{Name}: {Time}";
        }
    }
}