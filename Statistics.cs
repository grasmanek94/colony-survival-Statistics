using NetworkUI;
using NetworkUI.Items;
using UnityEngine;
using Shared;
using static ModLoader;

namespace grasmanek94.Statistics
{
    [ModManager]
    public static class Statistics
    {
        [ModCallback(EModCallbackType.OnConstructTooltipUI, "OnConstructTooltipUI")]
        static void OnConstructTooltipUI(ConstructTooltipUIData data)
        {
            if(data.hoverType != ETooltipHoverType.Item)
            {
                return;
            }

            var sources = ServerManager.RecipeStorage.SourceBlockTypesPerProductionItem;
            //var other_sources = BlockFarmAreaJobDefinition / PlacedBlockType

            ushort valueAtIndex = data.hoverItem;

            /*
                Stats:
                    - Production (Output)
                    - Consumption (Input)
                    - Worker amount
                    - Amount of farms currently producing
                    - Amount of crafting places currently producing
            */

            // OnNPCCraftedRecipe
            // OnNPCGathered
            // OnNPCJobChanged
            // OnNPCDied
            // OnNPCLoaded
            // OnNPCRecruited
            // OnNPCSaved
            // Stockpile class (Harmony)
            //  public bool TryRemove(ushort type, int amount = 1, bool sendUpdate = true)
            //  public bool TryRemove(IList<InventoryItem> toRemoveItems)
            //  public bool TryRemove(InventoryItem item)
            //  public void Add(ushort type, int amount = 1)
            //  public void Add(InventoryItem item)
            //  public void Add(IList<InventoryItem> list)
            //  public void AddEnumerable<T>(T toAddItems) where T : IEnumerable<InventoryItem>
            //  public bool TryRemoveFood(ref float currentFood, float desiredFoodAddition)
            //  public void Clear()

            data.menu.Items.Add(new Line(Color.white, 2, -1, 10, 2));
            data.menu.Items.Add(new Label(new LabelData("Blablabla", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
        }
    }
}
