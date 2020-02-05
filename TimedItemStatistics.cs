using System.Collections;
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

        private int[] nextAverages;

        private static double GamePeriodLengthInMinutes { get; set; }

        private ItemStatistics[] timedItemStatistics;
        private ItemStatistics CurrentStatistics { get; set; }
        public ItemStatistics AllTimeStatistics { get; private set; }

        public static int PeriodsToGameHours(int periods)
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
            AllTimeStatistics = new ItemStatistics();
            averages = new List<ItemStatistics>();

            CurrentPeriod = GetPeriod();
            for(int i = 0; i < MaxPeriods; ++i)
            {
                timedItemStatistics[i] = new ItemStatistics();
            }
            CurrentStatistics = timedItemStatistics[CurrentPeriod];

            nextAverages = new int[]
            {
                1,
                // 6,
                // 12,
                24
                // 48,
                //MaxPeriods
            };
        }

        private void PerformPeriodUpdate(bool forceDirty = false)
        {
            if(forceDirty)
            {
                Dirty = true;
            }

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
            PerformPeriodUpdate(true);
            CurrentStatistics.Produce(amount);
            AllTimeStatistics.Produce(amount);
        }

        public override void Consume(int amount)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.Consume(amount);
            AllTimeStatistics.Consume(amount);
        }

        public override void AddProducer(int entity)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.AddProducer(entity);
            AllTimeStatistics.AddProducer(entity);
        }

        public override void AddConsumer(int entity)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.AddConsumer(entity);
            AllTimeStatistics.AddConsumer(entity);
        }

        public override void RemoveProducer(int entity)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.RemoveProducer(entity);
            AllTimeStatistics.RemoveProducer(entity);
        }

        public override void RemoveConsumer(int entity)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.RemoveConsumer(entity);
            AllTimeStatistics.RemoveConsumer(entity);
        }

        public override void AddInventory(int amount = 1)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.AddInventory(amount);
            AllTimeStatistics.AddInventory(amount);
        }

        public override void RemoveInventory(int amount = 1)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.RemoveInventory(amount);
            AllTimeStatistics.RemoveInventory(amount);
        }

        public override void UseAsFood(int amount)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.UseAsFood(amount);
            AllTimeStatistics.UseAsFood(amount);
        }

        public override void TradeIn(int amount)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.TradeIn(amount);
            AllTimeStatistics.TradeIn(amount);
        }

        public override void TradeOut(int amount)
        {
            PerformPeriodUpdate(true);
            CurrentStatistics.TradeOut(amount);
            AllTimeStatistics.TradeOut(amount);
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

            int nextAverage = 0;

            while (nextAverage < nextAverages.Length)
            {
                if(stats.Periods >= nextAverages[nextAverage])
                {
                    ++nextAverage;
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
