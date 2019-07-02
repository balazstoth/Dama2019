using Dama.Data.Models;
using System.Collections.Generic;

namespace Dama.Organizer.Comparers
{
    class LabelComparer : IEqualityComparer<Label>
    {
        public bool Equals(Label x, Label y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Label obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
