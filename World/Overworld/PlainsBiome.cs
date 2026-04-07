using Godot;
using System;
using System.Linq;

public partial class PlainsBiome : Biome
{
	public override void OrchestrateEventGeneration(Vector2 coordinates) {}
	public override bool OrchestrateMobGeneration(Vector2 coordinates)
	{
		// Spawn a mob
		if (GD.Randf() <= 0.5)
		{
			Mob mobInstance = GD.Load<PackedScene>("res://Entities/Mob.tscn").Instantiate<Mob>();
			int distFromPlayer = GD.RandRange(-1000, 1000);
			distFromPlayer = Mathf.Abs(distFromPlayer) <= 200 ? distFromPlayer + 300 : distFromPlayer;
			coordinates.X = coordinates.X + distFromPlayer;
			mobInstance.GlobalPosition = coordinates;
			AddChild(mobInstance);
			return true;
		}
		return false;
	}
	public override void OrchestrateLootGeneration(Vector2 coordinates) {}

	public override void _Ready()
	{
		BiomeElementLookup = new() {
			[BiomeElementNames.Grass] =  new BiomeElement().WithYLevelBoundaries(-100, 45).WithAtlasCoordinates(1, 29),
			[BiomeElementNames.Dirt] = new BiomeElement().WithNoiseBoundaries(-0.5f, 0.5f).WithYLevelBoundaries(44, 255).WithAtlasCoordinates(0, 29),
			[BiomeElementNames.Air] = new BiomeElement().WithNoiseBoundaries(-1.0f, -0.2f).WithYLevelBoundaries(46, 255).WithAtlasCoordinates(-1, -1),
			[BiomeElementNames.Coal] = new BiomeElement().WithNoiseBoundaries(-0.3f, -0.26f).WithYLevelBoundaries(100, 255).WithAtlasCoordinates(9, 5),
			[BiomeElementNames.Iron] = new BiomeElement().WithNoiseBoundaries(0.0f, 0.02f).WithYLevelBoundaries(160, 255).WithAtlasCoordinates(10, 5),
			[BiomeElementNames.Diamond] = new BiomeElement().WithNoiseBoundaries(0.2f, 0.21f).WithYLevelBoundaries(200, 255).WithAtlasCoordinates(8, 5),
	};
		BiomeElementNamesLookup = BiomeElementLookup.ToDictionary(
			pair => pair.Value.AtlasCoordinates,
			pair => pair.Key
	);
		TileTexturePath = "res://Resources/PlainsBiome.tres";
	}
	
	public override void LoadTextures()
	{
		this.TileSet = GD.Load<TileSet>(TileTexturePath) ?? GD.Load<TileSet>("Some fallback resource path");
		if (this.TileSet is null)
		{
			
			GD.PrintErr("No tilesets found.");
		}
	}
	
	private void GenerateSurface(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Grass);
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
				SetCell(currentCellPosition, 1, BiomeElementLookup[BiomeElementNames.Grass].AtlasCoordinates);
			}
		}		
	}
	
	private void GenerateSolidGround(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Dirt);
		// First we iterate through x, because we want to find the grass block and fill up with dirt
		// to the end
		for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
		{
			bool foundGrass = false;
			for (int y = yStart; y < yEnd; y++)
			{
				Vector2I currentCellPosition = new(x, y);
				Vector2I currentAtlasCoordinates = GetCellAtlasCoords(currentCellPosition);
				if (currentAtlasCoordinates == BiomeElementLookup[BiomeElementNames.Grass].AtlasCoordinates)
				{
					foundGrass = true;
					continue;
				}
				if (foundGrass)
				{
					if (GetCellSourceId(currentCellPosition) == -1)
					{
						SetCell(currentCellPosition, 1, BiomeElementLookup[BiomeElementNames.Dirt].AtlasCoordinates);
					}
				}
			}
		}
	}
	
	private void GenerateUnderground(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Air);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				float noise2D = noiseFunction.GetNoise2D(0.25f * x, 0.25f * y);
				if (BiomeElementLookup[BiomeElementNames.Air].CanSpawnAt(y, noise2D)) 
				{
					SetCell(currentCellPosition, -1);
				}
			}
		}
	}

	private void GenerateCoalOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Coal);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				float noise2D = noiseFunction.GetNoise2D(x, y);
				if (BiomeElementLookup[BiomeElementNames.Coal].CanSpawnAt(y, noise2D)) 
				{
					SetCell(currentCellPosition, 1, BiomeElementLookup[BiomeElementNames.Coal].AtlasCoordinates);
				}
			}
		}
	}

	private void GenerateIronOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Iron);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				float noise2D = noiseFunction.GetNoise2D(x, y);
				if (BiomeElementLookup[BiomeElementNames.Iron].CanSpawnAt(y, noise2D)) 
				{
					SetCell(currentCellPosition, 1, BiomeElementLookup[BiomeElementNames.Iron].AtlasCoordinates);
				}
			}
		}
	}

	private void GenerateDiamondOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBiomePartYBoundaries(ref areaToBeChunked, BiomeElementNames.Diamond);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				float noise2D = noiseFunction.GetNoise2D(x,  y);
				if (BiomeElementLookup[BiomeElementNames.Diamond].CanSpawnAt(y, noise2D)) 
				{
					SetCell(currentCellPosition, 1, BiomeElementLookup[BiomeElementNames.Diamond].AtlasCoordinates);
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
		GenerateCoalOre(ref areaToBeChunked, ref noiseFunction);
		GenerateIronOre(ref areaToBeChunked, ref noiseFunction);
		GenerateDiamondOre(ref areaToBeChunked, ref noiseFunction);
	}

	public override void TryDestroyBlock(Vector2I mouseCoordinates, ref Inventory playersInventory)
	{
		Vector2I atlasCoords = GetCellAtlasCoords(mouseCoordinates);
		BiomeElementNames blockName = BiomeElementNamesLookup[atlasCoords];
		GD.Print($"Destroyed: {atlasCoords} = {blockName}");
		if (blockName == BiomeElementNames.Air)
		{
			return;
		}
		// TODO: Add before if we can destroy block or no.
		EraseCell(mouseCoordinates);
		playersInventory.AddNewItem(blockName);
	}

	public override void TryPlaceBlock(Vector2I mouseCoordinates, ref Inventory playersInventory)
	{
		// This cell is occupied
		if (GetCellSourceId(mouseCoordinates) != -1) 
		{ 
			return;
		}

		BiomeElementNames? blockName = playersInventory.UseCurrentItem();
		if (blockName is null)
		{
			return;
		}
		BiomeElement block = BiomeElementLookup[blockName.Value];
		GD.Print($"Got: {blockName} = {block}");
		SetCell(mouseCoordinates, 1, block.AtlasCoordinates);
	}

	public override void _Process(double delta)
	{
	}
}
