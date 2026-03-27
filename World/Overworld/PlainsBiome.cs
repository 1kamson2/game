using Godot;
using System;

public partial class PlainsBiome : Biome
{
	public override void GenerateEvents(Vector2 coordinates) {}
	public override void GenerateMobs(Vector2 coordinates) {}
	public override void GenerateLoot(Vector2 coordinates) {}

	public override void _Ready()
	{
		BiomeParts = new() {
			[BiomePartNames.Surface] = new() 
			{
				YLevelBoundaries = new(-100, 45),
				AtlasCoordinates = new(0, 9) 
			},
			[BiomePartNames.SolidGround] = new() 
			{ 
				NoiseBoundaries = new(-0.5f, 0.5f),
				YLevelBoundaries = new(44, 255),
				AtlasCoordinates = new(0, 0) 
			},
			[BiomePartNames.Caves] = new() 
			{ 
				NoiseBoundaries = new(-1.0f, -0.2f), 
				YLevelBoundaries = new(46, 255), 
				AtlasCoordinates = new(-1, -1)
			},
	};
		TileTexturePath = "res://Resources/PlainsBiome.tres";
		// LoadTileTextures();
	}
	
	public override void LoadTileTextures()
	{
		this.TileSet = GD.Load<TileSet>(TileTexturePath) ?? GD.Load<TileSet>("Some fallback resource path");
		if (this.TileSet is null)
		{
			GD.PrintErr("No tilesets found.");
		}
	}
	
	private void GenerateSurface(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomePartNames.Surface);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				// TODO: This can be probably cached
				float surfaceNoise = noiseFunction.GetNoise1D(x);
				int groundLevel = (int)((surfaceNoise + 1) * 30) + 25;
				Vector2I currentCellPosition = new Vector2I(x, groundLevel);
				// Check if cell is taken
				if (GetCellSourceId(currentCellPosition) != -1)
				{
					continue;
				}
				SetCell(currentCellPosition, 1, BiomeParts[BiomePartNames.Surface].AtlasCoordinates);
			}
		}		
	}
	
	private void GenerateSolidGround(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomePartNames.SolidGround);
		// First we iterate through x, because we want to find the grass block and fill up with dirt
		// to the end
		for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
		{
			bool foundGrass = false;
			for (int y = yStart; y < yEnd; y++)
			{
				Vector2I currentCellPosition = new(x, y);
				Vector2I currentAtlasCoordinates = GetCellAtlasCoords(currentCellPosition);
				if (currentAtlasCoordinates == BiomeParts[BiomePartNames.Surface].AtlasCoordinates)
				{
					foundGrass = true;
					continue;
				}
				if (foundGrass)
				{
					if (GetCellSourceId(currentCellPosition) == -1)
					{
						SetCell(currentCellPosition, 1, BiomeParts[BiomePartNames.SolidGround].AtlasCoordinates);
					}
				}
			}
		}
	}
	
	private void GenerateUnderground(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomePartNames.Caves);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				float noise2D = noiseFunction.GetNoise2D(0.25f * x, 0.25f * y);
				if (BiomeParts[BiomePartNames.Caves].MeetsRequirements(y, noise2D)) 
				{
					SetCell(currentCellPosition, -1);
				}
			}
		}
	}

	// TODO: Rename it maybe to "OrchestrateChunkGeneration", since it will be more verbose about what it does.
	public override void OrchestrateChunkGeneration(Vector2 coordinates, int chunkSize, FastNoiseLite noiseFunction)
	{
		// This returns the Vector with width and height of viewport 
		// TODO: Align it to be full chunks
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		ChunkArea areaToBeChunked = new ChunkArea(ref coordinates, ref viewportSize, chunkSize);
		// TODO: Enforce the process generation (if we swapped two functions the world would generate incorrectly)
		GenerateSurface(ref areaToBeChunked, ref noiseFunction);
		GenerateSolidGround(ref areaToBeChunked, ref noiseFunction);
		GenerateUnderground(ref areaToBeChunked, ref noiseFunction);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
