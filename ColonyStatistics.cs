using System.Collections.Generic;

namespace grasmanek94.Statistics
{
    public class ColonyStatistics
    {
        Dictionary<ushort, TimedItemStatistics> itemStatistics;
        TimedItemStatistics npcStatistics;

        public ColonyStatistics()
        {
            itemStatistics = new Dictionary<ushort, TimedItemStatistics>();
            npcStatistics = new TimedItemStatistics();
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

        // died
        public void NPCConsume()
        {
            npcStatistics.Consume(1);
        }

        // born
        public void NPCProduce()
        {
            npcStatistics.Produce(1);
        }

        public List<ItemStatistics> GetNPCAverages()
        {
            return npcStatistics.Averages();
        }
    }
}
