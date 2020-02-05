using NetworkUI;
using NetworkUI.Items;
using UnityEngine;
using Shared;
using Jobs;
using static ModLoader;
using System.Collections.Generic;
using Recipes;
using NPC;
using System;
using Pipliz.JSON;
using Harmony;
using Pipliz;
using Jobs.Implementations;
using Jobs.Implementations.Construction;
using static Jobs.BlockFarmAreaJobDefinition;
using static Jobs.Implementations.TemperateForesterDefinition;

namespace grasmanek94.Statistics
{
    [ModManager]
    public static class Statistics
    {
        static Dictionary<Colony, ColonyStatistics> colonyStats;

        [ModCallback(EModCallbackType.OnAssemblyLoaded, "OnAssemblyLoaded", float.MaxValue)]
        static void OnAssemblyLoaded(string assemblyPath)
        {
            colonyStats = new Dictionary<Colony, ColonyStatistics>();

            var harmony = HarmonyInstance.Create("grasmanek94.Statistics");
            harmony.PatchAll();
        }

        static void PrintStatistic(ConstructTooltipUIData data, ItemStatistics stat, bool allTime)
        {
            string span = "";

            if (!allTime)
            {
                span = "Last " + new TimeSpan(TimedItemStatistics.PeriodsToGameHours(stat.Periods), 0, 0).ToHumanReadableString() + " average:";
            }
            else
            {
                span = "This session average:";
            }

            data.menu.Items.Add(new Line(Color.white, 2, -1, 10, 2));

            data.menu.Items.Add(new Label(new LabelData(span, TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData("Created " + stat.AverageProduced.ToString(".0#") + ", Used " + stat.AverageConsumed.ToString(".0#"), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData(stat.AverageProducers.ToString(".0#") + " producers, " + stat.AverageConsumers.ToString(".0#") + " consumers", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData("Stock " + stat.AverageInventoryAdded.ToString(".0#") + " added, " + stat.AverageInventoryRemoved.ToString(".0#") + " removed", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            
            if(stat.TradedIn != 0 || stat.TradedOut != 0)
            {
                data.menu.Items.Add(new Label(new LabelData("Trade +" + stat.AverageTradedIn.ToString(".0#") + " / -" + stat.AverageTradedOut.ToString(".0#"), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            }

            if(stat.UsedForFood != 0)
            {
                data.menu.Items.Add(new Label(new LabelData("Food Use: " + stat.AverageUsedForFood.ToString(".0#"), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            }
        }

        [ModCallback(EModCallbackType.OnConstructTooltipUI, "OnConstructTooltipUI", float.MaxValue)]
        static void OnConstructTooltipUI(ConstructTooltipUIData data)
        {
            if(data.hoverType != ETooltipHoverType.Item)
            {
                return;
            }

            TimedItemStatistics stats = GetColonyStats(data.player.ActiveColony).GetTimedItemStats(data.hoverItem);
      
            var statlist = stats.Averages();

            foreach (var stat in statlist)
            {
                PrintStatistic(data, stat, false);
            }

            PrintStatistic(data, stats.AllTimeStatistics, false);
        }

        // TODO
        static void AddProducerConsumer(NPCBase npc, IJob job)
        {
            INPCTypeSettings npcSettings;

            if (npc == null || job == null || 
                ServerManager.RecipeStorage == null || 
                NPCType.NPCTypes == null ||
                !NPCType.NPCTypes.TryGetValue(npc.NPCType, out npcSettings))
            {
                return;
            }

            List<Recipe> recipes;

            Log.Write("AddProducerconsumer: KeyName = {0}", npcSettings.KeyName);

            bool found = ServerManager.RecipeStorage.TryGetRecipes(npcSettings.KeyName, out recipes);
            
            Log.Write("AddProducerconsumer: Found = {0}", found);

            if(found)
            {
                foreach(var recipe in recipes)
                {
                    Log.Write("AddProducerconsumer: Requirements:");
                    foreach(var requirement in recipe.Requirements)
                    {
                        Log.Write("{0}: {1}", requirement.Type, requirement.Amount);
                    }
                    Log.Write("AddProducerconsumer: Results:");
                    foreach (var result in recipe.Results)
                    {
                        Log.Write("{0}: {1}", result.Type, result.Amount);
                    }
                }
            }
        }

        // TODO
        static void RemoveProducerConsumer(NPCBase npc, IJob job)
        {
            INPCTypeSettings npcSettings;

            if (npc == null || job == null ||
                ServerManager.RecipeStorage == null ||
                NPCType.NPCTypes == null ||
                !NPCType.NPCTypes.TryGetValue(npc.NPCType, out npcSettings))
            {
                return;
            }

            List<Recipe> recipes;

            Log.Write("RemoveProducerConsumer: KeyName = {0}", npcSettings.KeyName);

            bool found = ServerManager.RecipeStorage.TryGetRecipes(npcSettings.KeyName, out recipes);

            Log.Write("RemoveProducerConsumer: Found = {0}", found);

            if (found)
            {
                foreach (var recipe in recipes)
                {
                    Log.Write("RemoveProducerConsumer: Requirements:");
                    foreach (var requirement in recipe.Requirements)
                    {
                        Log.Write("{0}: {1}", requirement.Type, requirement.Amount);
                    }
                    Log.Write("RemoveProducerConsumer: Results:");
                    foreach (var result in recipe.Results)
                    {
                        Log.Write("{0}: {1}", result.Type, result.Amount);
                    }
                }
            }
        }

        [ModCallback(EModCallbackType.OnNPCDied, "OnNPCDied", float.MaxValue)]
        static void OnNPCDied(NPCBase npc)
        {
            if (npc == null || npc.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);
            stats.NPCConsume();

            RemoveProducerConsumer(npc, npc.Job);
        }

        [ModCallback(EModCallbackType.OnNPCLoaded, "OnNPCLoaded", float.MaxValue)]
        static void OnNPCLoaded(NPCBase npc, JSONNode json)
        {
            if (npc == null || npc.Colony == null)
            {
                return;
            }

            AddProducerConsumer(npc, npc.Job);
        }

        [ModCallback(EModCallbackType.OnNPCRecruited, "OnNPCRecruited", float.MaxValue)]
        static void OnNPCRecruited(NPCBase npc)
        {
            if (npc == null || npc.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);
            stats.NPCProduce();
        }

        [ModCallback(EModCallbackType.OnNPCJobChanged, "OnNPCJobChanged", float.MaxValue)]
        static void OnNPCJobChanged(ValueTuple<NPCBase, IJob, IJob> data)
        {
            NPCBase npc = data.Item1;
            IJob oldJob = data.Item2;
            IJob newJob = data.Item3;

            if (npc == null || npc.Colony == null)
            {
                return;
            }

            RemoveProducerConsumer(npc, oldJob);
            AddProducerConsumer(npc, newJob);
        }

        [ModCallback(EModCallbackType.OnNPCGathered, "OnNPCGathered", float.MaxValue)]
        static void OnNPCGathered(IJob job, Pipliz.Vector3Int pos, List<ItemTypes.ItemTypeDrops> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Log.Write("OnNPCGathered {0} {1}", item.Type, item.Amount);
                }
            }

            if (job == null || job.NPC == null || job.NPC.Colony == null || items == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(job.NPC.Colony);

            foreach (var item in items)
            {
                var itemStat = stats.GetTimedItemStats(item.Type);
                itemStat.Produce(item.Amount);
                itemStat.AddProducer(job.NPC.ID);
            }
        }

        [ModCallback(EModCallbackType.OnNPCCraftedRecipe, "OnNPCCraftedRecipe", float.MaxValue)]
        static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> result)
        {
            if (result != null)
            {
                foreach (var item in result)
                {
                    Log.Write("OnNPCCraftedRecipe {0} {1}", item.Type, item.Amount);
                }
            }

            if (job == null || job.NPC == null || job.NPC.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(job.NPC.Colony);

            if(recipe != null)
            {
                foreach(var item in recipe.Requirements)
                {
                    var itemStat = stats.GetTimedItemStats(item.Type);
                    itemStat.Consume(item.Amount);
                    itemStat.AddConsumer(job.NPC.ID);
                }
            }

            if (result != null)
            {
                foreach (var item in result)
                {
                    var itemStat = stats.GetTimedItemStats(item.Type);
                    itemStat.Produce(item.Amount);
                    itemStat.AddProducer(job.NPC.ID);
                }
            }
        }

        // Maybe add these too: (?)
        // JobFinder

        // Maybe I should wait until NPC is at stockpile and hook into the NPC.Inventory.Dump function? (often found in OnNPCAtStockpile)
        // That would be actualy more accurate than Crafted/Gathered, because these callbacks are called before the items are in the inventory
        // .. which means that if an NPC dies it's not accounted for correctly? but... is is much easier to keep track of recipes this way..
        // or process the NPC inventory on death.. but then how do we know if the NPC got it from the stockpile (Consumer) or produced it (Producer)? 
        // is the Job still valid?

        public static ColonyStatistics GetColonyStats(Colony colony)
        {
            if(colony == null)
            {
                return null;
            }

            ColonyStatistics stats;
            if(!colonyStats.TryGetValue(colony, out stats))
            {
                stats = new ColonyStatistics();
                colonyStats.Add(colony, stats);
            }

            return stats;
        }
    }
}
