using Godot;
using System;
using System.Collections.Generic;


public abstract partial class Biome : TileMapLayer
{
	public Dictionary<BiomePartNames, BiomePart> BiomeParts { get; set; }
	// Define how big a biome will be
	public Vector2I SpanningBoundaries { get; protected set; } = new(int.MinValue, int.MinValue);
	// Define the path for tile textures
	public string TileTexturePath { get; set; } = "";
	// Generate chunks base on the given coordinates. It will render chunks to fillup the entire viewport.
	// Then renders the chunks in the up, right, down, left directions.
	public abstract void OrchestrateChunkGeneration(Vector2 coordinates, int chunkSize, FastNoiseLite noiseFunction);
	public abstract void GenerateMobs(Vector2 coordinates);
	public abstract void GenerateLoot(Vector2 coordinates);
	public abstract void GenerateEvents(Vector2 coordinates);
	public abstract void LoadTileTextures();

	public virtual (int, int) FindBiomePartYBoundaries(ref ChunkArea areaToBeChunked, BiomePartNames biomePart)
	{
		return ((int)Mathf.Max(
			areaToBeChunked.UpperLeftCorner.Y, 
			BiomeParts[biomePart].YLevelBoundaries.X
		), (int)Mathf.Min(
			areaToBeChunked.LowerRightCorner.Y, 
			BiomeParts[biomePart].YLevelBoundaries.Y
		));
	}
}
public enum BiomeNames
{
	Overworld,
}

public struct BiomePart
{
	// The boundary is inclusive
	public Vector2 NoiseBoundaries{ get;  set; }
	// The boundary is inclusive
	public Vector2 YLevelBoundaries { get;  set; }
	// Coordinates in the tileset
	public Vector2I AtlasCoordinates { get; set; }
	
	public bool MeetsRequirements(int y, float currentNoise)
	{
		bool yRequirement = (y >= YLevelBoundaries.X && 
							 y <= YLevelBoundaries.Y);
		bool noiseRequirement = (currentNoise >= NoiseBoundaries.X && 
								 currentNoise <= NoiseBoundaries.Y);
		return yRequirement && noiseRequirement;
	}
}

public enum BiomePartNames
{
	Surface,
	SolidGround,
	Caves,
}

public class ChunkArea
{
	// (x, y) coordinates of the upper left corner
	public Vector2I UpperLeftCorner { get; set; }
	// (x, y) coordinates of the lower right corner
	public Vector2I LowerRightCorner { get; set; }
	public ChunkArea(ref Vector2 coordinates, ref Vector2 viewportSize, int chunkSize)
	{
		Vector2 viewportMidPoint = new Vector2(viewportSize.X / 2, viewportSize.Y / 2);
		UpperLeftCorner = new Vector2I(
			(int) (coordinates.X - viewportMidPoint.X - chunkSize),
			(int) (coordinates.Y - viewportMidPoint.X - chunkSize)
		);
		LowerRightCorner = new Vector2I(
			(int) (coordinates.X + viewportMidPoint.X + chunkSize),
			(int) (coordinates.Y + viewportMidPoint.Y + chunkSize)
		);
	}
}
