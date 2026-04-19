using Godot;

/// <summary>
/// ItemContainer is used to store and chunk items in the player's inventory.
/// </summary>
public interface IInventoryContainer : IRegistrable
{
    [Export] public int MaxStackSize { get; set; }
    [Export] public string Name { get; set; }
    [Export] public bool IsStackable { get; set; }
    [Export] public Vector2I TilesetCoordinates { get; set; }
	public int CurrentStackSize { get; set; }

    public bool IsEmpty() => CurrentStackSize <= 0;
    public bool CanBeStacked()
    {
        return (
            CurrentStackSize < MaxStackSize &&
            IsStackable
        );
    }
}