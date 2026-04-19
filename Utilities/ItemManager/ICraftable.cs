using Godot;
using System.Collections.Generic;
using System.Linq;


public interface ICraftable
{
    /// <summary>
    /// RequiredItems stores the item ID and the amount needed to craft this new item
    /// </summary>
    [Export] public CraftingInformation[] RequiredItems { get; set; }
    public bool CanBeCrafted(ref Inventory playerInventory)
    {
        int itemRemaining = RequiredItems.Select(item => item.Amount).Sum();
        foreach (var item in playerInventory.Items)
        {
            foreach (var requiredItem in RequiredItems)
            {
                if ((item as Block).Id != requiredItem.Id)
                {
                    continue;
                }

                if (item.CurrentStackSize >= requiredItem.Amount)
                {
                    itemRemaining -= requiredItem.Amount;
                }
                else
                {
                    // This item cannot be crafted anyways
                    return false;
                }
            }
        }
        return itemRemaining == 0;
    }
}