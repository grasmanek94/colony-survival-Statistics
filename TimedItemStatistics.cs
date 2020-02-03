using System.Collections.Generic;

namespace grasmanek94.Statistics
{
    public class TimedItemStatistics : ProducerConsumer
    {
        private int PeriodDays { get { return 7; } }
        private int RealLifePeriodLengthInSeconds { get { return 60; } } // 1 period = 1 real minute
        private int MaxPeriods { get; set; }
        private bool Dirty { get; set; }
        private List<ItemStatistics> averages;

        private int _currentPeriod;
        private int CurrentPeriod 
        { 
            get { return _currentPeriod; } 
            set 
            { 
                _currentPeriod = value % MaxPeriods;
            } 
        }

        private int[] nextHoursTable;

        private double GamePeriodLengthInMinutes { get; set; }

        private ItemStatistics[] timedItemStatistics;
        private ItemStatistics CurrentStatistics { get; set; }

        public int PeriodsToGameHours(int periods)
        {
            return (int)(periods * GamePeriodLengthInMinutes / 60.0);
        }

        public int GetPeriod()
        {
            return (int)(TimeCycle.TotalTime.Value.TotalMinutes / GamePeriodLengthInMinutes) % MaxPeriods;
        }

        public TimedItemStatistics()
        {
            GamePeriodLengthInMinutes = ServerManager.ServerSettings.Time.GameTimeScale * RealLifePeriodLengthInSeconds / 60;
            MaxPeriods = (int)(PeriodDays * 24 * 60 / GamePeriodLengthInMinutes) + 1;
            timedItemStatistics = new ItemStatistics[MaxPeriods];
            averages = new List<ItemStatistics>();

            CurrentPeriod = GetPeriod();
            for(int i = 0; i < MaxPeriods; ++i)
            {
                timedItemStatistics[i] = new ItemStatistics();
            }
            CurrentStatistics = timedItemStatistics[CurrentPeriod];

            double period_hours = GamePeriodLengthInMinutes / 60.0;
            nextHoursTable = new int[]
            {
                (int)(1 * period_hours),
                // (int)(6 * period_hours),
                // (int)(12 * period_hours),
                (int)(24 * period_hours),
                // (int)(48 * period_hours),
                (int)(MaxPeriods * period_hours)
            };
        }

        private void PerformPeriodUpdate()
        {
            int period = GetPeriod();
            if (CurrentPeriod != period)
            {
                while (CurrentPeriod != period)
                {
                    timedItemStatistics[++CurrentPeriod].Reset();
                }
                Dirty = true;
                CurrentStatistics = timedItemStatistics[CurrentPeriod];
            }
        }

        public override void Produce(int amount)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.Produce(amount);
        }

        public override void Consume(int amount)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.Consume(amount);
        }

        public override void AddProducer(int producers = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.AddProducer(producers);
        }

        public override void AddConsumer(int consumers = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.AddConsumer(consumers);
        }

        public override void RemoveProducer(int producers = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.RemoveProducer(producers);
        }

        public override void RemoveConsumer(int consumers = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.RemoveConsumer(consumers);
        }

        public override void AddInventory(int amount = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.AddInventory(amount);
        }

        public override void RemoveInventory(int amount = 1)
        {
            PerformPeriodUpdate();
            Dirty = true;
            CurrentStatistics.RemoveInventory(amount);
        }

        public ItemStatistics Average(int lastPeriods)
        {
            if(lastPeriods <= 0)
            {
                return new ItemStatistics();
            }

            if(lastPeriods > MaxPeriods)
            {
                lastPeriods = MaxPeriods;
            }

            PerformPeriodUpdate();

            ItemStatistics stats = new ItemStatistics(CurrentStatistics);

            while (--lastPeriods > 0)
            {
                int adjusted_period = _currentPeriod - lastPeriods;
                if(adjusted_period < 0)
                {
                    adjusted_period = MaxPeriods + adjusted_period;
                }

                stats += timedItemStatistics[adjusted_period];
            }

            return stats;
        }

        public List<ItemStatistics> Averages()
        {
            PerformPeriodUpdate();

            if(!Dirty && averages != null)
            {
                return averages;
            }

            averages = new List<ItemStatistics>();

            ItemStatistics stats = new ItemStatistics(CurrentStatistics);

            int lastPeriods = 0;

            int nextHours = 0;

            while (nextHours < nextHoursTable.Length)
            {
                int hours = PeriodsToGameHours(stats.Periods);
                if(nextHoursTable[nextHours] >= hours)
                {
                    nextHours++;
                    averages.Add(new ItemStatistics(stats));
                }

                int adjusted_period = _currentPeriod - (++lastPeriods);
                if (adjusted_period < 0)
                {
                    adjusted_period = MaxPeriods + adjusted_period;
                }

                stats += timedItemStatistics[adjusted_period];
            }

            Dirty = false;
            return averages;
        }
    }
}
