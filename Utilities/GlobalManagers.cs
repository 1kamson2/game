using System;
using Godot;

public partial class GlobalManagers : Node2D
{
	public static GlobalManagers Instance { get; private set; }
	public readonly BlockManager BlockManagerSingleton = GD.Load<PackedScene>("res://Utilities/BlockManager/BlockManager.tscn").Instantiate<BlockManager>();
	public readonly TilesetManager TilesetManagerSingleton = GD.Load<PackedScene>("res://Utilities/TilesetManager/TilesetManager.tscn").Instantiate<TilesetManager>();
	public readonly WorldManager WorldManagerSingleton = GD.Load<PackedScene>("res://Utilities/WorldManager/WorldManager.tscn").Instantiate<WorldManager>();
	public readonly ItemManager ItemManagerSingleton = GD.Load<PackedScene>("res://Utilities/ItemManager/ItemManager.tscn").Instantiate<ItemManager>();

    public override void _Ready()
    {
        base._Ready();
		Instance = this;
		AddChild(TilesetManagerSingleton);
		AddChild(BlockManagerSingleton);
		AddChild(WorldManagerSingleton);
		AddChild(ItemManagerSingleton);
		PostReadyInitialization();
    }

	private void PostReadyInitialization()
	{
		throw new NotImplementedException();
	}

	public T GetManager<T>() where T : class
	{
		if (BlockManagerSingleton is T b)
		{
			return b;
		} 
		if (TilesetManagerSingleton is T t)
		{
			return t;
		} 
		if (WorldManagerSingleton is T w)
		{
			return w;
		}
		if (ItemManagerSingleton is T i)
		{
			return i;
		}
		return null;
	}
}