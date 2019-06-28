using Dama.Data.Enums;
using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Data.Interfaces
{
    public interface IActivity : IEntity
    {
        string Name { get; set; }
        string Description { get; set; }
        Color Color { get; set; }
        CreationType CreationType { get; set; }
        IEnumerable<Label> LabelCollection { get; set; }
        Category Category { get; set; }
        string UserId { get; set; }
        ActivityType ActivityType { get; set; }
    }
}
