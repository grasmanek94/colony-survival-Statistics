using System.Collections.Generic;

namespace grasmanek94.Statistics
{
    public class ColonyStatistics
    {
        Dictionary<ushort, TimedItemStatistics> itemStatistics;

        public ColonyStatistics()
        {
            itemStatistics = new Dictionary<ushort, TimedItemStatistics>();
        }

        public TimedItemStatistics GetTimedItemStats(ushort item)
        {
            TimedItemStatistics stats;
            if(!itemStatistics.TryGetValue(item, out stats))
            {
                stats = new TimedItemStatistics();
                itemStatistics.Add(item, stats);
            }
            return stats;
        }
    }
}
