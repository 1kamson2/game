using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

public partial class ItemManager : ResourceManager<UsableItem>
{
    private string _itemFilesDirectory = "res://Resources/Items/";
    /// <summary>
	/// ItemLookup translates id to UsableItem
	/// </summary>
	protected Dictionary<string, UsableItem> ItemLookup { get; set; } = new();

    public ItemManager()
    {
        {
            string[] files = DirAccess.GetFilesAt(_itemFilesDirectory);
            foreach (string file in files)
            {
                string fullPath = $"{_itemFilesDirectory}{file}";
                GD.Print($"Loading the item file: {fullPath}");
                UsableItem item = GD.Load<UsableItem>(fullPath);
                GD.Print($"Trying to register: {item.Id}");
                if (RegisterResource(item))
                {
                    if (!ItemLookup.TryGetValue(item.Id, out _))
                    {
                        ItemLookup[item.Id] = item;
                    }
                }
            }
        }
    }

    public override bool RegisterResource(UsableItem resource)
    {
        if (resource is null)
        {
            GD.PrintErr("Block resource is null");
            return false;
        }

        if (string.IsNullOrEmpty(resource.Id))
        {
            GD.PrintErr($"{resource.Id} is not a valid ID for Block instance.");
            return false;
        }

        if (Registry.ContainsKey(resource.Id))
        {
            GD.PrintErr($"Resource with {resource.Id} already exists in the Registry.");
            GD.PrintErr("If you want to reassign it, then UnregisterResource the already existing resource and Register it again.");
            return false;
        }

        Registry.Add(resource.Id, resource);
        return true;
    }

    public override bool UnregisterResource(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            GD.PrintErr($"`{id}` is not a valid ID.");
        }
        return Registry.Remove(id);
    }

    public override UsableItem GetResource(string id)
    {
        bool result = Registry.TryGetValue(id, out UsableItem item);
        if (!result)
        {
            GD.PrintErr($"Couldn't find the block with the id: `{id}`");
            return null;
        }
        return item;
    }

    public override U GetResourceAs<U>(string id)
    {
        UsableItem item = GetResource(id);
        if (item is null)
        {
            GD.PrintErr($"Couldn't find the block with the id: `{id}`");
            return null;
        }
        return item as U;
    }

    public void FindAllCraftableItems(Inventory playerInventory)
    {
        foreach (var (k, v) in Registry)
        {
            int index = -1;
            if (CanBeCrafted(k, playerInventory))
            {
                
                index = playerInventory.Recipes.FindIndex(recipe => recipe is not null && recipe.Id == k);
                if (index != -1)
                {
                    // it exists already
                    continue;
                }

                index = playerInventory.Recipes.FindIndex(recipe => recipe is null);
                if (index != -1)
                {
                    playerInventory.Recipes[index] = v;
                }
            }
            else
            {
                index = playerInventory.Recipes.FindIndex(recipe => recipe is not null && recipe.Id == k);
                if (index != -1)
                {
                    playerInventory.Recipes[index] = null;
                }
            }
        }
    }
    public bool CanBeCrafted(string item_id, Inventory playerInventory)
    {
        if (!Registry.TryGetValue(item_id, out UsableItem registredItem))
        {
            return false;
        }

        var ingredients = playerInventory.Items
            .Where(pItem => pItem is not null && registredItem.RequiredItems
                .Any(rItem => rItem is not null && pItem.Id == rItem.Id)
        );

        foreach (var requiredItem in registredItem.RequiredItems)
        {
            int counter = requiredItem.Amount;
            foreach (var ingredient in ingredients)
            {
                if (ingredient.Id == requiredItem.Id)
                {
                    counter -= Math.Min(ingredient.CurrentStackSize, counter);
                }
            }

            if (counter != 0)
            {
                return false;
            }
        }
        return true;
    }
}
