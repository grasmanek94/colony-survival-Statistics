using System.Collections;
using System.Collections.Generic;

namespace grasmanek94.Statistics
{
    public class ItemStatistics : ProducerConsumer
    {
        private HashSet<int> _producers;
        private HashSet<int> _consumers;

        public int Periods { get; private set; }

        public int Produced { get; private set; }
        public int Consumed { get; private set; }
        public int Producers { get; private set; }
        public int Consumers { get; private set; }
        public int InventoryAdded { get; private set; }
        public int InventoryRemoved { get; private set; }

        public override void Produce(int amount)
        {
            Produced += amount;
        }

        public override void Consume(int amount)
        {
            Consumed += amount;
        }

        public override void AddProducer(int entity)
        {
            if(_producers.Add(entity))
            {
                ++Producers;
            }
        }

        public override void AddConsumer(int entity)
        {
            if(_consumers.Add(entity))
            {
                ++Consumers;
            }
        }

        public override void RemoveProducer(int entity)
        {
            if(_producers.Remove(entity))
            {
                --Producers;
            }
        }

        public override void RemoveConsumer(int entity)
        {
            if(_consumers.Remove(entity))
            {
                --Consumers;
            }
        }

        public override void AddInventory(int amount)
        {
            InventoryAdded += amount;
        }

        public override void RemoveInventory(int amount)
        {
            InventoryRemoved += amount;
        }

        public ItemStatistics()
        {
            Reset();
        }

        public void Reset()
        {
            _producers = new HashSet<int>();
            _consumers = new HashSet<int>();

            Produced = 0;
            Consumed = 0;
            Producers = 0;
            Consumers = 0;
            InventoryAdded = 0;
            InventoryRemoved = 0;

            Periods = 1;
        }

        public ItemStatistics(int produced, int consumed, int producers, int consumers, int inventoryAdded, int inventoryRemoved, int periods)
        {
            _producers = new HashSet<int>();
            _consumers = new HashSet<int>();

            Produced = produced;
            Consumed = consumed;
            Producers = producers;
            Consumers = consumers;
            InventoryAdded = inventoryAdded;
            InventoryRemoved = inventoryRemoved;

            Periods = periods;
        }

        public ItemStatistics(ItemStatistics other)
        {
            _producers = other._producers;
            _consumers = other._consumers;

            Produced = other.Produced;
            Consumed = other.Consumed;
            Producers = other.Producers;
            Consumers = other.Consumers;
            InventoryAdded = other.InventoryAdded;
            InventoryRemoved = other.InventoryRemoved;

            Periods = other.Periods;
        }

        public float NetProduced { get { return Produced - Consumed; } }
        public float NetProducers { get { return Producers - Consumers; } }
        public float NetInventory { get { return InventoryAdded - InventoryRemoved; } }

        public float AverageProduced { get { return Produced / Periods; } }
        public float AverageConsumed { get { return Consumed / Periods; } }
        public float AverageProducers { get { return Producers / Periods; } }
        public float AverageConsumers { get { return Consumers / Periods; } }
        public float AverageInventoryAdded { get { return InventoryAdded / Periods; } }
        public float AverageInventoryRemoved { get { return InventoryRemoved / Periods; } }
        public float AverageNetProduced { get { return NetProduced / Periods; } }
        public float AverageNetProducers { get { return NetProducers / Periods; } }
        public float AverageNetInventory { get { return NetInventory / Periods; } }

        public static ItemStatistics operator +(ItemStatistics a, ItemStatistics b)
        {
            return new ItemStatistics(
                a.Produced + b.Produced,
                a.Consumed + b.Consumed,
                a.Producers + b.Producers,
                a.Consumers + b.Consumers,
                a.InventoryAdded + b.InventoryAdded,
                a.InventoryRemoved + b.InventoryRemoved,
                a.Periods + b.Periods
            );

            //temp._producers.UnionWith(a._producers);
            //temp._producers.UnionWith(b._producers);

            //temp._consumers.UnionWith(a._consumers);
            //temp._consumers.UnionWith(b._consumers);
        }
    }
}
