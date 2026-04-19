using System.Collections.Generic;
using Godot;

[GlobalClass] public partial class UsableItem : Resource, IInventoryContainer, ICraftable, IStatsModifier
{
    [Export] public int MaxStackSize { get; set; }
    [Export] public string Name { get; set; }
    [Export] public bool IsStackable { get; set; }
    [Export] public Vector2I TilesetCoordinates { get; set; }
    public int CurrentStackSize { get; set; }
    [Export] public CraftingInformation[] RequiredItems { get; set; }
    [Export] public string Id { get; set; }
}