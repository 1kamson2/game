using Godot;
using System;
using System.Collections.Generic;


public abstract partial class Biome : TileMapLayer
{
	/// <summary>
	/// BiomeElementLookup translates BiomeElementNames to BiomeElement
	/// </summary>
	public Dictionary<BiomeElementNames, BiomeElement> BiomeElementLookup { get; set; }
	/// <summary>
	/// BiomeElementNamesLookup translates Atlas Coordiantes to BiomeElementNames
	/// </summary>
	public Dictionary<Vector2I, BiomeElementNames> BiomeElementNamesLookup { get; set; }
	// Define how big a biome will be
	public Vector2I SpanningBoundaries { get; protected set; } = new(int.MinValue, int.MinValue);
	// Define the path for tile textures
	public string TileTexturePath { get; set; } = "";
	// Generate chunks base on the given coordinates. It will render chunks to fillup the entire viewport.
	// Then renders the chunks in the up, right, down, left directions.
	public abstract void OrchestrateChunkGeneration(Vector2 coordinates, int chunkSize, FastNoiseLite noiseFunction);
	public abstract bool OrchestrateMobGeneration(Vector2 coordinates);
	public abstract void OrchestrateLootGeneration(Vector2 coordinates);
	public abstract void OrchestrateEventGeneration(Vector2 coordinates);
	public abstract void TryDestroyBlock(Vector2I mouseCoordinates, ref Inventory playersInventory);
	public abstract void TryPlaceBlock(Vector2I mouseCoordinates, ref Inventory playersInventory);
	public abstract void LoadTextures();

	public virtual (int, int) FindBiomePartYBoundaries(ref ChunkArea areaToBeChunked, BiomeElementNames biomePart)
	{
		return ((int)Mathf.Max(
			areaToBeChunked.UpperLeftCorner.Y, 
			BiomeElementLookup[biomePart].YLevelBoundaries.X
		), (int)Mathf.Min(
			areaToBeChunked.LowerRightCorner.Y, 
			BiomeElementLookup[biomePart].YLevelBoundaries.Y
		));
	}
}
public enum BiomeNames
{
	Overworld,
}

public class BiomeElement
{
	// The boundary is inclusive
	public Vector2 NoiseBoundaries{ get;  set; }
	// The boundary is inclusive
	public Vector2 YLevelBoundaries { get;  set; }
	// Coordinates in the tileset
	public Vector2I AtlasCoordinates { get; set; }

	public BiomeElement(ref Vector2 noiseBoundaries, ref Vector2 yLevelBoundaries, ref Vector2I atlasCoordinates)
	{
		// TODO: Add checking of the parameters later.
		NoiseBoundaries = noiseBoundaries;
		YLevelBoundaries = yLevelBoundaries;
		AtlasCoordinates = atlasCoordinates;
	}

	public BiomeElement() { }

	public BiomeElement WithNoiseBoundaries(float lowerNoiseBoundary, float upperNoiseBoundary)
	{
		NoiseBoundaries = new(lowerNoiseBoundary, upperNoiseBoundary);
		return this;
	}

	public BiomeElement WithYLevelBoundaries(int lowerYLevelBoundary, int upperYLevelBoundary)
	{
		YLevelBoundaries = new(lowerYLevelBoundary, upperYLevelBoundary);
		return this;
	}

	public BiomeElement WithAtlasCoordinates(int x, int y)
	{
		AtlasCoordinates = new(x, y);
		return this;
	}
	
	public bool CanSpawnAt(int y, float currentNoise)
	{
		bool yRequirement = (y >= YLevelBoundaries.X && 
							 y <= YLevelBoundaries.Y);
		bool noiseRequirement = (currentNoise >= NoiseBoundaries.X && 
								 currentNoise <= NoiseBoundaries.Y);
		return yRequirement && noiseRequirement;
	}
}

/// <summary>
/// BiomeElementNames enum stores information about what biome can contain, e.g.: structures (Surface, SolidGround, Caves, etc.),
/// ores (Coal, Iron, Diamond) 
/// </summary>
public enum BiomeElementNames
{
	Grass,
	Dirt,
	Air,
	Coal,
	Iron,
	Diamond
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
