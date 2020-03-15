using Dama.Data.Models;
using System;
using System.Linq;

namespace Dama.Data
{
    public static class ActivityExtension
    {
        public static bool ContainsLabel(this Activity activity, string label)
        {
            if (string.IsNullOrEmpty(label))
                throw new ArgumentNullException("label");

            return activity.Labels.Any(l => l.Name == label);
        }
    }
}
