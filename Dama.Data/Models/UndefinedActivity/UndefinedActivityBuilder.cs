namespace Dama.Data.Models
{
    class UndefinedActivityBuilder : ActivityBuilder
    {
        private int minimumTime;
        private int maximumTime;

        public UndefinedActivityBuilder WithMinTime(int minTime)
        {
            minimumTime = minTime;
            return this;
        }

        public UndefinedActivityBuilder WithMaxTime(int maxTime)
        {
            maximumTime = maxTime;
            return this;
        }

        public static implicit operator UndefinedActivity(UndefinedActivityBuilder ufb)
        {
            return new UndefinedActivity(ufb.name,
                                     ufb.description,
                                     ufb.color,
                                     ufb.creationType,
                                     ufb.labels,
                                     ufb.category,
                                     ufb.userId,
                                     ufb.minimumTime,
                                     ufb.maximumTime);
        }
    }
}
