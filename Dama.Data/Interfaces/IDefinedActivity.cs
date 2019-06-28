using Dama.Data.Models;
using System;

namespace Dama.Data.Interfaces
{
    interface IDefinedActivity
    {
        int Priority { get; set; }
        TimeSpan TimeSpan { get; set; }
        DateTime? Start { get; set; }
        Repeat Repeat { get; set; }
    }
}
