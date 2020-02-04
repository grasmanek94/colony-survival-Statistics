using Harmony;
using System;
using Pipliz.Collections;
using Pipliz;

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

			ColonyStatistics stats = Statistics.GetColonyStats(__instance.Owner);
			stats.GetTimedItemStats(type).AddInventory(amount);
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
				ColonyStatistics stats = Statistics.GetColonyStats(__instance.Owner);
				stats.GetTimedItemStats(type).RemoveInventory(amount);
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
	public class StockpileHookRemoveAt
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

			ColonyStatistics stats = Statistics.GetColonyStats(stockpile.Owner);
			stats.GetTimedItemStats(__instance.GetKeyAtIndex(index)).RemoveInventory(amount);
		}
	}

	[HarmonyPatch(typeof(SortedList<ushort, int>))]
	[HarmonyPatch("SetValueAtIndex")]
	public class StockpileHookSpots
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

			ColonyStatistics stats = Statistics.GetColonyStats(stockpile.Owner);

			int difference = __instance.GetValueAtIndex(index) - val;
			ushort type = __instance.GetKeyAtIndex(index);
			if (difference > 0)
			{
				stats.GetTimedItemStats(type).RemoveInventory(val);
			}
			else
			{
				stats.GetTimedItemStats(type).AddInventory(-val);
			}
		}
	}
}
