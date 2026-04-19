using Godot;
using System;
using System.Collections.Generic;


[GlobalClass] public abstract partial class Biome : TileMapLayer, IRegistrable
{
	[Export] public string Id { get; set; }
	// Define how big a biome will be
	[Export] public Vector2I SpanningBoundaries { get; protected set; } = new(int.MinValue, int.MinValue);
	// Generate chunks base on the given coordinates. It will render chunks to fillup the entire viewport.
	// Then renders the chunks in the up, right, down, left directions.
	public abstract void OrchestrateChunkGeneration(Vector2 coordinates, int chunkSize, FastNoiseLite noiseFunction);
	public abstract bool OrchestrateMobGeneration(Vector2 coordinates);
	public abstract void OrchestrateLootGeneration(Vector2 coordinates);
	public abstract void OrchestrateEventGeneration(Vector2 coordinates);
	public abstract void TryDestroyBlock(Vector2I mouseCoordinates, ref Inventory playersInventory);
	public abstract void TryPlaceBlock(Vector2I mouseCoordinates, ref Inventory playersInventory);
	public virtual (int, int) FindBlockHeightBoundaries(ref ChunkArea areaToBeChunked, string blockName)
	{
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResource(blockName);
		return ((int)Mathf.Max(
			areaToBeChunked.UpperLeftCorner.Y, 
			block.HeightBoundaries.X
		), (int)Mathf.Min(
			areaToBeChunked.LowerRightCorner.Y, 
			block.HeightBoundaries.Y
		));
	}
}


/// <summary>
///  ChunkArea defines boundaries that chunking must occur in.
/// </summary>
public struct ChunkArea
{
	/// <summary>
	/// (x, y) coordinates of the upper left corner
	/// </summary>
	public Vector2I UpperLeftCorner { get; set; }
	/// <summary>
	/// (x, y) coordinates of the lower right corner
	/// </summary>
	public Vector2I LowerRightCorner { get; set; }
	/// <summary>
	/// Create area to be chunked.
	/// </summary>
	/// <param name="coordinates"> Coordinates where evaluation should begin, e.g.: Player's location, some other entity location. </param>
	/// <param name="viewportSize"> Current viewport size. </param>
	/// <param name="chunkSize"> Current chunk size. </param>
	public ChunkArea(ref Vector2 coordinates, ref Vector2 viewportSize, int chunkSize)
	{
		Vector2 viewportMidPoint = new Vector2(viewportSize.X / 2, viewportSize.Y / 2);
		UpperLeftCorner = new Vector2I(
			(int) (coordinates.X - viewportMidPoint.X - chunkSize),
			(int) (coordinates.Y - viewportMidPoint.Y - chunkSize)
		);
		LowerRightCorner = new Vector2I(
			(int) (coordinates.X + viewportMidPoint.X + chunkSize),
			(int) (coordinates.Y + viewportMidPoint.Y + chunkSize)
		);
	}
}
