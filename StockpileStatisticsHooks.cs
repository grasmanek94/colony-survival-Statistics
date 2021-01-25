using System;
using Pipliz.Collections;
using Pipliz;
using Jobs.Implementations;
using Jobs;
using NPC;
using static Jobs.BlockFarmAreaJobDefinition;
using Jobs.Implementations.Construction.Types;
using Jobs.Implementations.Construction;
using HarmonyLib;
using static ColonyTrading;
using Assets.ColonyPointUpgrades;
using System.Reflection;

namespace grasmanek94.Statistics
{
    class DebugLog
    {
        public static void Write(string s)
        {
#if (DEBUG)
            Log.WriteWarning(s);
#endif
        }

        public static void Write(string s, bool something)
        {
            Write(s + " (" + something.ToString() + ")");
        }

        public static void Write(string s, string something)
        {
            Write(s + ": " + something);
        }

        public static void Write(string s, object something)
        {
            Write(s + " (" + (something == null ? "NULL" : "NOT NULL") + ")");
        }
    }

    //Stockpile

    [HarmonyPatch(typeof(Stockpile))]
    [HarmonyPatch("Add")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(int) })]
    public class StockpileHookAdd
    {
        static void Prefix(Stockpile __instance, ushort type, int amount)
        {
            if (amount <= 0 || type <= 0 || __instance.Owner == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name + ": INVALID");
                return;
            }

            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            InventoryStatistics.AddInventory(__instance.Owner, type, amount);
        }
    }

    [HarmonyPatch(typeof(Stockpile))]
    [HarmonyPatch("TryRemove")]
    [HarmonyPatch(new Type[] { typeof(ushort), typeof(int), typeof(bool) })]
    public class StockpileHookTryRemove
    {
        static void Postfix(Stockpile __instance, bool __result, ushort type, int amount, bool sendUpdate)
        {
            if (amount <= 0 || type <= 0 || __instance.Owner == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name + ": INVALID");
                return;
            }

            if (__result)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, __result);
                InventoryStatistics.RemoveInventory(__instance.Owner, type, amount);
            }
            else
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, __result);
            }
        }
    }

    [HarmonyPatch(typeof(ColonyShopVisitTracker))]
    [HarmonyPatch("OnVisit")]
    public class ColonyShopVisitTrackerHookOnVisit
    {
        public static Stockpile inFunctionStockpile = null;
        public static NPCBase npc = null;

        static void Prefix(NPCBase npc, ref bool gotFood)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, npc);

            inFunctionStockpile = npc.Colony.Stockpile;
            ColonyShopVisitTrackerHookOnVisit.npc = npc;

        }

        static void Postfix(NPCBase npc, ref bool gotFood)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);

            inFunctionStockpile = null;
            ColonyShopVisitTrackerHookOnVisit.npc = null;
        }
    }
    [HarmonyPatch(typeof(SortedList<ushort, int>))]
    [HarmonyPatch("RemoveAt")]
    public class SortedListHookRemoveAt
    {
        public static void Prefix(SortedList<ushort, int> __instance, int index, int amount)
        {
            Stockpile stockpile = ColonyShopVisitTrackerHookOnVisit.inFunctionStockpile;
            if (stockpile == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "INVALID SOTCKPILE");
                return;
            }

            if (stockpile.Owner == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "INVALID OWNER");
                return;
            }

            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            InventoryStatistics.RemoveInventory(stockpile.Owner, __instance.GetKeyAtIndex(index), amount);
        }
    }

    [HarmonyPatch(typeof(SortedList<ushort, int>))]
    [HarmonyPatch("SetValueAtIndex")]
    public class SortedListHookSetValueAtIndex
    {
        static void Prefix(SortedList<ushort, int> __instance, int index, int val)
        {
            Stockpile stockpile = ColonyShopVisitTrackerHookOnVisit.inFunctionStockpile;
            if (stockpile == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "INVALID SOTKCPILE");
                return;
            }

            if (stockpile.Owner == null)
            {
                DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "INVALID OWNER");
                return;
            }

            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);

            int difference = __instance.GetValueAtIndex(index) - val;
            ushort type = __instance.GetKeyAtIndex(index);

            if (difference > 0)
            {
                InventoryStatistics.RemoveInventory(stockpile.Owner, type, difference);
            }
            else
            {
                InventoryStatistics.AddInventory(stockpile.Owner, type, -difference);
            }
        }
    }

    //Jobs

    [HarmonyPatch(typeof(ScientistJobSettings))]
    [HarmonyPatch("OnNPCAtJob")]
    public class ScientistJobSettingsHookOnNPCAtJob
    {
        public static NPCBase npc = null;

        static void Prefix(ScientistJobSettingsHookOnNPCAtJob __instance, BlockJobInstance blockJobInstance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, blockJobInstance.NPC);
            npc = blockJobInstance.NPC;
        }

        static void Postfix(ScientistJobSettingsHookOnNPCAtJob __instance, BlockJobInstance blockJobInstance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    [HarmonyPatch(typeof(GuardJobSettings))]
    [HarmonyPatch("ShootAtTarget")]
    public class GuardJobSettingsHookShootAtTarget
    {
        public static NPCBase npc = null;

        static void Prefix(GuardJobSettings __instance, GuardJobInstance instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, instance.NPC);
            npc = instance.NPC;
        }

        static void Postfix(GuardJobSettings __instance, GuardJobInstance instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    [HarmonyPatch(typeof(BlockFarmAreaJob))]
    [HarmonyPatch("OnNPCAtJob")]
    public class BlockFarmAreaJobHookOnNPCAtJob
    {
        public static NPCBase npc = null;

        static void Prefix(BlockFarmAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, __instance.NPC);
            npc = __instance.NPC;
        }

        static void Postfix(BlockFarmAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    [HarmonyPatch(typeof(FarmAreaJob))]
    [HarmonyPatch("OnNPCAtJob")]
    public class FarmAreaJobHookOnNPCAtJob
    {
        public static NPCBase npc = null;

        static void Prefix(FarmAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, __instance.NPC);
            npc = __instance.NPC;
        }

        static void Postfix(FarmAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    [HarmonyPatch(typeof(BuilderBasic))]
    [HarmonyPatch("DoJob")]
    public class BuilderBasicHookDoJob
    {
        public static NPCBase npc = null;

        static void Prefix(BuilderBasic __instance, IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, job.NPC);
            npc = job.NPC;
        }

        static void Postfix(BuilderBasic __instance, IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    [HarmonyPatch(typeof(AbstractAreaJob))]
    [HarmonyPatch("OnNPCAtStockpile")]
    public class AbstractAreaJobHookOnNPCAtStockpile
    {
        public static NPCBase npc = null;

        static void Prefix(AbstractAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, __instance.NPC);
            npc = __instance.NPC;
        }

        static void Postfix(AbstractAreaJob __instance, ref NPCBase.NPCState state)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            npc = null;
        }
    }

    //Trading

    [HarmonyPatch(typeof(Rule))]
    [HarmonyPatch("TryExecute")]
    public class RuleHookTryExecute
    {
        public static bool trading = false;

        static void Prefix(Rule __instance, ref float tradePower)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            trading = true;
        }

        static void Postfix(Rule __instance, ref float tradePower)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name);
            trading = false;
        }
    }

    public class InventoryStatistics
    {
        public static void AddInventory(Colony colony, ushort type, int amount)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "Type: " + type.ToString() + ", Amount: " + amount.ToString());

            ColonyStatistics stats = Statistics.GetColonyStats(colony);
            TimedItemStatistics itemStats = stats.GetTimedItemStats(type);

            itemStats.AddInventory(amount);

            RemoveConsumerAddProducer(itemStats, ScientistJobSettingsHookOnNPCAtJob.npc, amount);
            RemoveConsumerAddProducer(itemStats, GuardJobSettingsHookShootAtTarget.npc, amount);
            RemoveConsumerAddProducer(itemStats, BlockFarmAreaJobHookOnNPCAtJob.npc, amount);
            RemoveConsumerAddProducer(itemStats, FarmAreaJobHookOnNPCAtJob.npc, amount);
            RemoveConsumerAddProducer(itemStats, BuilderBasicHookDoJob.npc, amount);
            RemoveConsumerAddProducer(itemStats, ColonyShopVisitTrackerHookOnVisit.npc, amount);
            RemoveConsumerAddProducer(itemStats, AbstractAreaJobHookOnNPCAtStockpile.npc, amount);

            if (ItemTypes.GetType(type).ColonyPointsMeal > 0)
            {
                // wonder if this gets ever called ? It shouldn't though
                itemStats.UseAsFood(-amount);
            }

            if (RuleHookTryExecute.trading)
            {
                itemStats.TradeIn(amount);
            }
        }

        public static void RemoveInventory(Colony colony, ushort type, int amount)
        {
            DebugLog.Write(MethodBase.GetCurrentMethod().DeclaringType + "::" + MethodBase.GetCurrentMethod().Name, "Type: " + type.ToString() + ", Amount: " + amount.ToString());

            ColonyStatistics stats = Statistics.GetColonyStats(colony);
            TimedItemStatistics itemStats = stats.GetTimedItemStats(type);

            itemStats.RemoveInventory(amount);

            AddConsumerRemoveProducer(itemStats, ScientistJobSettingsHookOnNPCAtJob.npc, amount);
            AddConsumerRemoveProducer(itemStats, GuardJobSettingsHookShootAtTarget.npc, amount);
            AddConsumerRemoveProducer(itemStats, BlockFarmAreaJobHookOnNPCAtJob.npc, amount);
            AddConsumerRemoveProducer(itemStats, FarmAreaJobHookOnNPCAtJob.npc, amount);
            AddConsumerRemoveProducer(itemStats, BuilderBasicHookDoJob.npc, amount);
            AddConsumerRemoveProducer(itemStats, ColonyShopVisitTrackerHookOnVisit.npc, amount);
            AddConsumerRemoveProducer(itemStats, AbstractAreaJobHookOnNPCAtStockpile.npc, amount);

            if (ItemTypes.GetType(type).ColonyPointsMeal > 0)
            {
                itemStats.UseAsFood(amount);
            }

            if (RuleHookTryExecute.trading)
            {
                itemStats.TradeOut(amount);
            }
        }

        static void AddConsumerRemoveProducer(TimedItemStatistics stats, NPCBase npc, int amount)
        {
            if (stats == null || npc == null || amount == 0)
            {
                return;
            }

            stats.AddConsumer(npc.ID);
            stats.RemoveProducer(npc.ID);
        }

        static void RemoveConsumerAddProducer(TimedItemStatistics stats, NPCBase npc, int amount)
        {
            if (stats == null || npc == null || amount == 0)
            {
                return;
            }

            stats.RemoveConsumer(npc.ID);
            stats.AddProducer(npc.ID);
        }
    }
}
