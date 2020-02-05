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
                string span = new TimeSpan(stats.PeriodsToGameHours(stat.Periods), 0, 0).ToHumanReadableString();

                data.menu.Items.Add(new Line(Color.white, 2, -1, 10, 2));

                data.menu.Items.Add(new Label(new LabelData("Last " + span + " average:", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                data.menu.Items.Add(new Label(new LabelData("Created " + stat.AverageProduced.ToString() + ", Used " + stat.AverageConsumed.ToString(), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                data.menu.Items.Add(new Label(new LabelData(stat.AverageProducers.ToString() + " producers, " + stat.AverageConsumers.ToString() + " consumers" , TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                data.menu.Items.Add(new Label(new LabelData("Stock " + stat.AverageInventoryAdded.ToString() + " added, " + stat.AverageInventoryRemoved.ToString() + " removed", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            }
        }

        static void AddProducerConsumer(NPCBase npc, IJob job)
        {
            if (npc == null || job == null || ServerManager.RecipeStorage == null)
            {
                return;
            }

            List<Recipe> recipes;

            Log.Write("AddProducerconsumer: KeyName = {0}", NPCType.NPCTypes[npc.NPCType].KeyName);

            bool found = ServerManager.RecipeStorage.TryGetRecipes(NPCType.NPCTypes[npc.NPCType].KeyName, out recipes);
            
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

            switch (job)
            {
                case BlockFarmAreaJob blockFarm:
                    Log.Write("BlockFarmAreaJob");
                    break;
                case FarmAreaJob farm:
                    Log.Write("FarmAreaJob");
                    break;
                case OliveFarmerAreaJob oliveFarm:
                    Log.Write("OliveFarmerAreaJob");
                    break;
                case ForesterJob forestFarm:
                    Log.Write("ForesterJob");
                    break;
                case CraftingJobInstance crafting:
                    Log.Write("CraftingJobInstance");
                    break;
                case CraftingJobWaterInstance water:
                    Log.Write("CraftingJobWaterInstance");
                    break;
                case GuardJobInstance guard:
                    Log.Write("GuardJobInstance");
                    break;
                case MinerJobInstance miner:
                    Log.Write("MinerJobInstance {0}", miner.BlockType.Name);
                    break;
                case ScientistJobInstance scientist:
                    Log.Write("ScientistJobInstance");
                    break;
                case TraderJobInstance trader:
                    Log.Write("TraderJobInstance");
                    break;
                case ConstructionJobInstance construction:
                    Log.Write("ConstructionJobInstance");
                    break;
                case BlockJobInstance block:
                    Log.Write("BlockJobInstance");
                    break;
                case AbstractAreaJob abstractArea:
                    Log.Write("AbstractAreaJob");
                    break;
                case AbstractFarmAreaJobDefinition abstractFarm:
                    Log.Write("AbstractFarmAreaJobDefinition");
                    break;
                case LaborerJob laborer:
                    Log.Write("LaborerJob");
                    break;
            }
        }

        static void RemoveProducerConsumer(NPCBase npc, IJob job)
        {
            if (npc == null || job == null || ServerManager.RecipeStorage == null)
            {
                return;
            }

            List<Recipe> recipes;

            Log.Write("RemoveProducerConsumer: KeyName = {0}", NPCType.NPCTypes[npc.NPCType].KeyName);

            bool found = ServerManager.RecipeStorage.TryGetRecipes(NPCType.NPCTypes[npc.NPCType].KeyName, out recipes);

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

            switch (job)
            {
                case BlockFarmAreaJob blockFarm:
                    Log.Write("BlockFarmAreaJob");
                    break;
                case FarmAreaJob farm:
                    Log.Write("FarmAreaJob");
                    break;
                case OliveFarmerAreaJob oliveFarm:
                    Log.Write("OliveFarmerAreaJob");
                    break;
                case ForesterJob forestFarm:
                    Log.Write("ForesterJob");
                    break;
                case CraftingJobInstance crafting:
                    Log.Write("CraftingJobInstance");
                    break;
                case CraftingJobWaterInstance water:
                    Log.Write("CraftingJobWaterInstance");
                    break;
                case GuardJobInstance guard:
                    Log.Write("GuardJobInstance");
                    break;
                case MinerJobInstance miner:
                    Log.Write("MinerJobInstance");
                    break;
                case ScientistJobInstance scientist:
                    Log.Write("ScientistJobInstance");
                    break;
                case TraderJobInstance trader:
                    Log.Write("TraderJobInstance");
                    break;
                case ConstructionJobInstance construction:
                    Log.Write("ConstructionJobInstance");
                    break;
                case BlockJobInstance block:
                    Log.Write("BlockJobInstance");
                    break;
                case AbstractAreaJob abstractArea:
                    Log.Write("AbstractAreaJob");
                    break;
                case AbstractFarmAreaJobDefinition abstractFarm:
                    Log.Write("AbstractFarmAreaJobDefinition");
                    break;
                case LaborerJob laborer:
                    Log.Write("LaborerJob");
                    break;
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

        // var other_sources = BlockFarmAreaJobDefinition / PlacedBlockType
        // var sources = ServerManager.RecipeStorage.SourceBlockTypesPerProductionItem;

        /*
            Stats:
                - Production (Output)
                - Consumption (Input)
                - Requirement
                - Worker amount
                - Amount of farms currently producing
                - Amount of crafting places currently producing
        */

        // STILL TODO:
        // Stockpile class (Harmony)
        //  public bool TryRemoveFood(ref float currentFood, float desiredFoodAddition)
        //  public void Clear()
        // AreaJobTracker
        // BlockJobInstance??

        /*[ModCallback(EModCallbackType.OnLoadingColony, "OnLoadingColony")]
        static void OnLoadingColony(Colony colony, JSONNode node)
        {
            GetColonyStats(colony);
        }

        [ModCallback(EModCallbackType.OnCreatedColony, "OnCreatedColony")]
        static void OnCreatedColony(Colony colony)
        {
            GetColonyStats(colony);
        }

        [ModCallback(EModCallbackType.OnActiveColonyChanges, "OnActiveColonyChanges")]
        static void OnActiveColonyChanges(Players.Player player, Colony oldColony, Colony newColony)
        {
            GetColonyStats(oldColony);
            GetColonyStats(newColony);
        }

        [ModCallback(EModCallbackType.OnPlayerRespawn, "OnPlayerRespawn")]
        static void OnPlayerRespawn(Players.Player player)
        {
            GetColonyStats(player.ActiveColony);
        }*/

        // Maybe add ColonyTrading too?

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
