﻿using Dama.Data.Interfaces;
using System;

namespace Dama.Data.Models
{
    public class Milestone
    {
        public int Id { get; set; }
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