using Dama.Data.Models;
using Dama.Organizer.Enums;
using System.Collections.Generic;

namespace Dama.Organizer.Others
{
    public class NameComparer<T> : IComparer<T> where T : Activity
    {
        public ComparableProperties ComparableProperties { get; set; }
        public ComparableSequence ComparableSequence { get; set; }

        public NameComparer(ComparableSequence comparableSequence, ComparableProperties comparableProperties)
        {
            ComparableProperties = comparableProperties;
            ComparableSequence = comparableSequence;
        }

        public int Compare(T x, T y)
        {
            if (ComparableProperties == ComparableProperties.Name)
            {
                if (ComparableSequence == ComparableSequence.Asc)
                {
                    if (x.Name.CompareTo(y.Name) == -1)
                        return -1;
                    else
                    {
                        if (x.Name.CompareTo(y.Name) == 1)
                            return 1;

                        return 0;
                    }
                }

                if (ComparableSequence == ComparableSequence.Desc)
                {
                    if (x.Name.CompareTo(y.Name) == -1)
                        return 1;
                    else
                    {
                        if (x.Name.CompareTo(y.Name) == 1)
                            return -1;

                        return 0;
                    }
                }
            }

            return x.Name.CompareTo(y.Name);
        }
    }
}
