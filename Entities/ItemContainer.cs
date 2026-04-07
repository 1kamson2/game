using Godot;

/// <summary>
/// ItemContainer is used to store and chunk items in the player's inventory.
/// </summary>
public partial class ItemContainer : Resource
{
    [Export] public BiomeElementNames ItemName { get; set; }
    [Export] public int MaxStackSize { get; set; } = 9999;
	[Export] public int CurrentStackSize { get; set; } = 1;

    public bool IsEmpty()
    {
        return CurrentStackSize <= 0;
    }
}