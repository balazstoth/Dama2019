using Dama.Data.Models;
using System;

namespace Dama.Data.Interfaces
{
    public interface IDefinedActivity
    {
        int Priority { get; set; }
        TimeSpan TimeSpan { get; set; }
        DateTime? Start { get; set; }
        Repeat Repeat { get; set; }
    }
}
