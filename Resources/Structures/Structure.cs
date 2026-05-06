using Godot;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public abstract partial class Structure : TileMapLayer, IRegistrable
{
	[Export] public string Id { get; set; }
	[Export] public float SpawnChance { get; set; }
	[Export] public int SpaceInBetween { get; set; }
	public Vector2I[] TilesPositions { get; set; }
	public Vector2I[] TileAtlasCoords { get; set; }

	public override void _Ready()
	{
		Load();
	}
	
	/// <summary>
	/// Load loads the class data. 
	/// This function should be called whenever we don't add this Structure as a child node.
	/// </summary>
	public virtual void Load()
	{
		TilesPositions = GetUsedCells().ToArray();
		TileAtlasCoords = TilesPositions.Select(pos => GetCellAtlasCoords(pos)).ToArray();
	}

	public virtual Vector2I[] TilesGlobalPositions(Vector2I currentCellPosition) =>  TilesPositions.Select(pos => pos + currentCellPosition).ToArray();
}
