using System.Collections;

namespace grasmanek94.Statistics
{
    public abstract class ProducerConsumer
    {
        public abstract void Produce(int amount);
        public abstract void Consume(int amount);
        public abstract void AddProducer(int entity);
        public abstract void AddConsumer(int entity);
        public abstract void RemoveProducer(int entity);
        public abstract void RemoveConsumer(int entity);
        public abstract void AddInventory(int amount);
        public abstract void RemoveInventory(int amount);
    }
}
