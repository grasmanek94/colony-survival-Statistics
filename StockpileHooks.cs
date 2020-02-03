using Harmony;
using System;

namespace grasmanek94.Statistics
{
	[HarmonyPatch(typeof(Stockpile))]
	[HarmonyPatch("Add")]
	[HarmonyPatch(new Type[] { typeof(ushort), typeof(int) })]
	class StockpileHookAdd
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
	class StockpileHookTryRemove
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
	class StockpileHookTryRemoveFood
	{
		static void Prefix(Stockpile __instance, ref float currentFood, float desiredFoodAddition)
		{
			// TODO
		}

		static void Postfix(Stockpile __instance, bool __result, ref float currentFood, float desiredFoodAddition)
		{
			// TODO
		}
	}
}
