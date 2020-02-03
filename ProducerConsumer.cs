namespace grasmanek94.Statistics
{
    public abstract class ProducerConsumer
    {
        public abstract void Produce(int amount);
        public abstract void Consume(int amount);
        public abstract void AddProducer(int producers = 1);
        public abstract void AddConsumer(int consumers = 1);
        public abstract void RemoveProducer(int producers = 1);
        public abstract void RemoveConsumer(int consumers = 1);
        public abstract void AddInventory(int amount);
        public abstract void RemoveInventory(int amount);
    }
}
