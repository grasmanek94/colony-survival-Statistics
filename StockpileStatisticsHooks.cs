using Harmony;
using System;
using Pipliz.Collections;
using Pipliz;
using Jobs.Implementations;
using Jobs;
using NPC;

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

		static void Prefix(Stockpile __instance, ref float currentFood, float desiredFoodAddition)
		{
			inFunctionStockpile = __instance;
		}

		static void Postfix(Stockpile __instance, bool __result, ref float currentFood, float desiredFoodAddition)
		{
			inFunctionStockpile = null;
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

	
	[HarmonyPatch(typeof(BlockFarmAreaJobDefinition))]
	[HarmonyPatch("OnNPCAtJob")]
	public class BlockFarmAreaJobDefinitionHookOnNPCAtJob
	{
		public static NPCBase npc = null;

		static void Prefix(BlockFarmAreaJobDefinition __instance, ref NPCBase.NPCState state)
		{
			npc = null;
		}

		static void Postfix(BlockFarmAreaJobDefinition __instance, ref NPCBase.NPCState state)
		{
			npc = null;
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
		}

		public static void RemoveInventory(Colony colony, ushort type, int amount)
		{
			ColonyStatistics stats = Statistics.GetColonyStats(colony);
			TimedItemStatistics itemStats = stats.GetTimedItemStats(type);

			itemStats.RemoveInventory(amount);

			AddConsumerRemoveProducer(itemStats, ScientistJobSettingsHookOnNPCAtJob.npc, amount);
			AddConsumerRemoveProducer(itemStats, GuardJobSettingsHookShootAtTarget.npc, amount);
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
