using Dama.Data.Enums;
using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Data.Interfaces
{
    public interface IActivity
    {
        int Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        Color Color { get; set; }

        CreationType CreationType { get; set; }

        IEnumerable<Label> Labels { get; set; }

        Category Category { get; set; }

        string UserId { get; set; }

        ActivityType ActivityType { get; set; }
    }
}
