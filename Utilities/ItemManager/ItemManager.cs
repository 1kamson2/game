using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
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

    public List<UsableItem> FindAllCraftableItems(List<UsableItem> oldRecipes, List<IInventoryContainer> items)
    {
        List<UsableItem> newRecipes = new(oldRecipes.Capacity);
        foreach (var (k, v) in Registry)
        {
            if (CanBeCrafted(k, items))
            {
                newRecipes.Add(v);
            }
        }
        return newRecipes;
    }

    public bool CanBeCrafted(string item_id, List<IInventoryContainer> items)
    {
        if (!Registry.TryGetValue(item_id, out UsableItem registredItem))
        {
            return false;
        }

        var ingredients = items
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

    public string GetRandomItemId() => Registry.Keys.ElementAt(Random.Shared.Next(Registry.Count));
    
    /// <summary>
    /// Applies the modifiers based on the provided field.
    /// </summary>
    /// <param name="baseFieldValue">Field that will be modified.</param>
    /// <param name="statId">Id of the modifier.</param>
    /// <returns>Returns a modified value based on the ModifierType</returns>
    public double ApplyModifiersToField(double baseFieldValue, UsableItem usableItem, string statId)
    {
        var selectedStats = usableItem.Stats.Where(stat => stat.Id == statId);
        double modified = 0;
        foreach (Stat stat in selectedStats)
        {
            switch (stat.ModifierType)
            {
                case StatModifierType.Multiplier:
                    modified += baseFieldValue * (1 + stat.Value);
                    break;
                case StatModifierType.Add:
                    modified += stat.Value;
                    break;
            }
        }
        return modified;
    }
}
