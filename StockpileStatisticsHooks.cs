using Harmony;
using System;
using Pipliz.Collections;
using Pipliz;
using Jobs.Implementations;
using Jobs;
using NPC;
using static Jobs.BlockFarmAreaJobDefinition;
using Happiness;
using Jobs.Implementations.Construction.Types;
using Jobs.Implementations.Construction;
using static ColonyTrading;

namespace grasmanek94.Statistics
{
	[HarmonyPatch(typeof(Stockpile))]
	[HarmonyPatch("Add")]
	[HarmonyPatch(new Type[] { typeof(ushort), typeof(int) })]
	public class StockpileHookAdd
	{
		static void Prefix(Stockpile __instance, ushort type, int amount)
		{
			if (amount <= 0 || type <= 0 || __instance.Owner == null)
			{
				return;
			}

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
				return;
			}

			if (__result)
			{
				InventoryStatistics.RemoveInventory(__instance.Owner, type, amount);
			}
		}
	}

	[HarmonyPatch(typeof(Stockpile))]
	[HarmonyPatch("TryRemoveFood")]
	public class StockpileHookTryRemoveFood
	{
		public  static Stockpile inFunctionStockpile = null;
		public static bool isFood;

		static void Prefix(Stockpile __instance, ref float currentFood, float desiredFoodAddition)
		{
			inFunctionStockpile = __instance;
			isFood = true;
		}

		static void Postfix(Stockpile __instance, bool __result, ref float currentFood, float desiredFoodAddition)
		{
			inFunctionStockpile = null;
			isFood = false;
		}
	}

	[HarmonyPatch(typeof(SortedList<ushort, int>))]
	[HarmonyPatch("RemoveAt")]
	public class SortedListHookRemoveAt
	{
		static void Prefix(SortedList<ushort, int> __instance, int index, int amount)
		{
			Stockpile stockpile = StockpileHookTryRemoveFood.inFunctionStockpile;
			if (stockpile == null)
			{
				return;
			}

			Log.Write("RemoveAtHook Run");

			if (stockpile.Owner == null)
			{
				return;
			}

			InventoryStatistics.RemoveInventory(stockpile.Owner, __instance.GetKeyAtIndex(index), amount);
		}
	}

	[HarmonyPatch(typeof(SortedList<ushort, int>))]
	[HarmonyPatch("SetValueAtIndex")]
	public class SortedListHookSetValueAtIndex
	{
		static void Prefix(SortedList<ushort, int> __instance, int index, int val)
		{
			Stockpile stockpile = StockpileHookTryRemoveFood.inFunctionStockpile;
			if (stockpile == null)
			{
				return;
			}

			Log.Write("SetValueAtIndex Run");

			if(stockpile.Owner == null)
			{
				return;
			}

			int difference = __instance.GetValueAtIndex(index) - val;
			ushort type = __instance.GetKeyAtIndex(index);

			if (difference > 0)
			{
				InventoryStatistics.RemoveInventory(stockpile.Owner, type, val);
			}
			else
			{
				InventoryStatistics.AddInventory(stockpile.Owner, type, -val);
			}
		}
	}

	[HarmonyPatch(typeof(ScientistJobSettings))]
	[HarmonyPatch("OnNPCAtJob")]
	public class ScientistJobSettingsHookOnNPCAtJob
	{
		public static NPCBase npc = null;

		static void Prefix(BlockJobInstance __instance, ref NPCBase.NPCState state)
		{
			npc = __instance.NPC;
		}

		static void Postfix(BlockJobInstance __instance, NPCBase.NPCState state)
		{
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
			npc = instance.NPC;
		}

		static void Postfix(GuardJobSettings __instance, GuardJobInstance instance, ref NPCBase.NPCState state)
		{
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
			npc = __instance.NPC;
		}

		static void Postfix(BlockFarmAreaJob __instance, ref NPCBase.NPCState state)
		{
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
			npc = __instance.NPC;
		}

		static void Postfix(FarmAreaJob __instance, ref NPCBase.NPCState state)
		{
			npc = null;
		}
	}

	[HarmonyPatch(typeof(ItemConfig))]
	[HarmonyPatch("TryFetchMore")]
	public class ItemConfigHookTryFetchMore
	{
		public static bool isFood;

		static void Prefix(ItemConfig __instance, Colony colony, float multiplier)
		{
			isFood = true;
		}

		static void Postfix(ItemConfig __instance, Colony colony, float multiplier)
		{
			isFood = false;
		}
	}

	[HarmonyPatch(typeof(BuilderBasic))]
	[HarmonyPatch("DoJob")]
	public class BuilderBasicHookDoJob
	{
		public static NPCBase npc = null;

		static void Prefix(ItemConfig __instance, IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
		{
			npc = job.NPC;
		}

		static void Postfix(ItemConfig __instance, IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
		{
			npc = null;
		}
	}

	[HarmonyPatch(typeof(Rule))]
	[HarmonyPatch("TryExecute")]
	public class RuleHookTryExecute
	{
		public static bool trading = false;

		static void Prefix(Rule __instance, ref float tradePower)
		{
			trading = true;
		}

		static void Postfix(Rule __instance, ref float tradePower)
		{
			trading = false;
		}
	}

	public class InventoryStatistics
	{
		public static void AddInventory(Colony colony, ushort type, int amount)
		{
			ColonyStatistics stats = Statistics.GetColonyStats(colony);
			TimedItemStatistics itemStats = stats.GetTimedItemStats(type);

			itemStats.AddInventory(amount);

			RemoveConsumerAddProducer(itemStats, ScientistJobSettingsHookOnNPCAtJob.npc, amount);
			RemoveConsumerAddProducer(itemStats, GuardJobSettingsHookShootAtTarget.npc, amount);
			RemoveConsumerAddProducer(itemStats, BlockFarmAreaJobHookOnNPCAtJob.npc, amount);
			RemoveConsumerAddProducer(itemStats, FarmAreaJobHookOnNPCAtJob.npc, amount);
			RemoveConsumerAddProducer(itemStats, BuilderBasicHookDoJob.npc, amount);

			if (RuleHookTryExecute.trading)
			{
				itemStats.TradeIn(amount);
			}
		}

		public static void RemoveInventory(Colony colony, ushort type, int amount)
		{
			ColonyStatistics stats = Statistics.GetColonyStats(colony);
			TimedItemStatistics itemStats = stats.GetTimedItemStats(type);

			itemStats.RemoveInventory(amount);

			AddConsumerRemoveProducer(itemStats, ScientistJobSettingsHookOnNPCAtJob.npc, amount);
			AddConsumerRemoveProducer(itemStats, GuardJobSettingsHookShootAtTarget.npc, amount);
			AddConsumerRemoveProducer(itemStats, BlockFarmAreaJobHookOnNPCAtJob.npc, amount);
			AddConsumerRemoveProducer(itemStats, FarmAreaJobHookOnNPCAtJob.npc, amount);
			AddConsumerRemoveProducer(itemStats, BuilderBasicHookDoJob.npc, amount);

			if (StockpileHookTryRemoveFood.isFood || ItemConfigHookTryFetchMore.isFood)
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
