using System.Linq;

namespace Dama.Data
{
    public static class ActivityExtension
    {
        public static bool ContainsLabel(this Activity activity, string label)
        {
            label = label.ToLower();
            return activity.LabelCollection.Any(l => l.Name == label);
        }
    }
}
