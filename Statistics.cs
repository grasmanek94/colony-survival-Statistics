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

namespace grasmanek94.Statistics
{
    [ModManager]
    public static class Statistics
    {
        static Dictionary<Colony, ColonyStatistics> colonyStats;

        [ModCallback(EModCallbackType.OnAssemblyLoaded, "grasmanek94.Statistics.OnAssemblyLoaded", float.MaxValue)]
        static void OnAssemblyLoaded(string assemblyPath)
        {
            colonyStats = new Dictionary<Colony, ColonyStatistics>();

            var harmony = HarmonyInstance.Create("grasmanek94.Statistics");
            harmony.PatchAll();
        }

        static string PrintSingleStat(float value)
        {
            float absValue = Mathf.Abs(value);
            if(absValue < 1.0f)
            {
                return value.ToString("0.00");
            }
            if (absValue < 10.0f)
            {
                return value.ToString("0.0");
            }
            return value.ToString("0");
        }

        static void PrintStatistic(ConstructTooltipUIData data, ItemStatistics stat, bool allTime)
        {
            string span;

            if (!allTime)
            {
                span = "Last " + new TimeSpan(TimedItemStatistics.PeriodsToGameHours(stat.Periods), 0, 0).ToHumanReadableString() + " average:";
            }
            else
            {
                span = "Since world load:";
            }

            data.menu.Items.Add(new Line(Color.white, 2, -1, 10, 2));

            data.menu.Items.Add(new Label(new LabelData(span, TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData("Created " + PrintSingleStat(stat.AverageProduced) + ", Used " + PrintSingleStat(stat.AverageConsumed), TextAnchor.MiddleLeft, 13, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData(PrintSingleStat(stat.AverageProducers) + " producers, " + PrintSingleStat(stat.AverageConsumers) + " consumers", TextAnchor.MiddleLeft, 13, LabelData.ELocalizationType.Sentence), -1));
            data.menu.Items.Add(new Label(new LabelData("Stock " + PrintSingleStat(stat.AverageInventoryAdded) + " added, " + PrintSingleStat(stat.AverageInventoryRemoved) + " removed", TextAnchor.MiddleLeft, 13, LabelData.ELocalizationType.Sentence), -1));
            
            if(stat.TradedIn != 0 || stat.TradedOut != 0)
            {
                data.menu.Items.Add(new Label(new LabelData("Trade +" + PrintSingleStat(stat.AverageTradedIn) + " / -" + PrintSingleStat(stat.AverageTradedOut), TextAnchor.MiddleLeft, 13, LabelData.ELocalizationType.Sentence), -1));
            }

            if(stat.UsedForFood != 0)
            {
                data.menu.Items.Add(new Label(new LabelData("Food Use: " + PrintSingleStat(stat.AverageUsedForFood), TextAnchor.MiddleLeft, 13, LabelData.ELocalizationType.Sentence), -1));
            }
        }

        [ModCallback(EModCallbackType.OnConstructTooltipUI, "grasmanek94.Statistics.OnConstructTooltipUI", float.MaxValue)]
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

            PrintStatistic(data, stats.AllTimeStatistics, true);
        }

        static void AddProducerConsumer(NPCBase npc, IJob job)
        {
            INPCTypeSettings npcSettings;

            if (npc == null || job == null || 
                ServerManager.RecipeStorage == null || 
                NPCType.NPCTypes == null || npc.Colony == null ||
                !NPCType.NPCTypes.TryGetValue(job.NPCType, out npcSettings))
            {
                return;
            }
            
            List<Recipe> recipes;

            bool found = ServerManager.RecipeStorage.TryGetRecipes(npcSettings.KeyName, out recipes);

            if(found)
            {
                var colonyStats = GetColonyStats(npc.Colony);
                foreach (var recipe in recipes)
                {
                    foreach(var requirement in recipe.Requirements)
                    {
                        colonyStats.GetTimedItemStats(requirement.Type).AddConsumer(npc.ID);
                    }
                    foreach (var result in recipe.Results)
                    {
                        colonyStats.GetTimedItemStats(result.Type).AddProducer(npc.ID);
                    }
                }
            }
        }

        static void RemoveProducerConsumer(NPCBase npc, IJob job)
        {
            INPCTypeSettings npcSettings;

            if (npc == null || job == null ||
                ServerManager.RecipeStorage == null ||
                NPCType.NPCTypes == null || npc.Colony == null ||
                !NPCType.NPCTypes.TryGetValue(job.NPCType, out npcSettings))
            {
                return;
            }

            List<Recipe> recipes;

            bool found = ServerManager.RecipeStorage.TryGetRecipes(npcSettings.KeyName, out recipes);

            if (found)
            {
                var colonyStats = GetColonyStats(npc.Colony);
                foreach (var recipe in recipes)
                {
                    foreach (var requirement in recipe.Requirements)
                    {
                        colonyStats.GetTimedItemStats(requirement.Type).RemoveConsumer(npc.ID);
                    }
                    foreach (var result in recipe.Results)
                    {
                        colonyStats.GetTimedItemStats(result.Type).RemoveProducer(npc.ID);
                    }
                }
            }
        }

        [ModCallback(EModCallbackType.OnNPCDied, "grasmanek94.Statistics.OnNPCDied", float.MaxValue)]
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

        [ModCallback(EModCallbackType.OnNPCLoaded, "grasmanek94.Statistics.OnNPCLoaded", float.MaxValue)]
        static void OnNPCLoaded(NPCBase npc, JSONNode json)
        {
            if (npc == null || npc.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);
            stats.NPCProduce();

            AddProducerConsumer(npc, npc.Job);
        }

        [ModCallback(EModCallbackType.OnNPCRecruited, "grasmanek94.Statistics.OnNPCRecruited", float.MaxValue)]
        static void OnNPCRecruited(NPCBase npc)
        {
            if (npc == null || npc.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);
            stats.NPCProduce();

            AddProducerConsumer(npc, npc.Job);
        }

        [ModCallback(EModCallbackType.OnNPCJobChanged, "grasmanek94.Statistics.OnNPCJobChanged", float.MaxValue)]
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

        [ModCallback(EModCallbackType.OnNPCGathered, "grasmanek94.Statistics.OnNPCGathered", float.MaxValue)]
        static void OnNPCGathered(IJob job, Pipliz.Vector3Int pos, List<ItemTypes.ItemTypeDrops> items)
        {
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

        [ModCallback(EModCallbackType.OnNPCCraftedRecipe, "grasmanek94.Statistics.OnNPCCraftedRecipe", float.MaxValue)]
        static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> result)
        {
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
        // is the Job still valid? and looping al items to check the hashSets would be kinda inneficient.. maybe use the global statistic? But that's per item haha

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
