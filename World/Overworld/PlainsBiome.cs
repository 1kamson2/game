using Godot;
using System;
using System.Linq;

// TODO: Trees render incorrectly.
public partial class PlainsBiome : Biome
{
	public override void OrchestrateEventGeneration(Vector2 coordinates) { }
	public override bool OrchestrateMobGeneration(Vector2 coordinates)
	{
		// Spawn a mob
		if (GD.Randf() <= 0.5)
		{
			Mob mobInstance = GD.Load<PackedScene>("res://Entities/Mobs/Mob.tscn").Instantiate<Mob>();
			int distFromPlayer = GD.RandRange(-1000, 1000);
			distFromPlayer = Mathf.Abs(distFromPlayer) <= 200 ? distFromPlayer + 300 : distFromPlayer;
			coordinates.X = coordinates.X + distFromPlayer;
			mobInstance.GlobalPosition = coordinates;
			AddChild(mobInstance);
			return true;
		}
		return false;
	}
	public override void OrchestrateLootGeneration(Vector2 coordinates) { }

	public override void _Ready()
	{
		string tilesetName = GlobalManagers.Instance.GetManager<TilesetManager>().BiomeTilesetLookup[Id];
		var tileset = GlobalManagers.Instance.GetManager<TilesetManager>().GetResource(tilesetName);
		if (tileset is not null)
		{
			TileSet = tileset;
		}
		else
		{
			GD.PrintErr($"Couldn't load the tileset: {tileset.Id}");
		}
	}

	private double CompositeNoise(FastNoiseLite noiseFunction, int x, int y)
	{
		return (
			noiseFunction.GetNoise2D(x, y)
			+ 0.5 * noiseFunction.GetNoise2D(2 * x, 2 * y)
			+ 0.25 * noiseFunction.GetNoise2D(4 * x, 4 * y)
		) / 1.75;
	}

	private void GenerateSurface(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_grass");
		GD.Print(yStart, yEnd);
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
				Block grass = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_grass");
				SetCell(currentCellPosition, 1, grass.TilesetCoordinates);
			}
		}
	}

	private void GenerateSolidGround(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_dirt");
		// First we iterate through x, because we want to find the grass block and fill up with dirt
		// to the end
		for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
		{
			bool foundGrass = false;
			for (int y = yStart; y < yEnd; y++)
			{
				Vector2I currentCellPosition = new(x, y);
				Vector2I currentAtlasCoordinates = GetCellAtlasCoords(currentCellPosition);
				Block dirt = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_dirt");
				Block grass = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_grass");
				if (currentAtlasCoordinates == grass.TilesetCoordinates)
				{
					foundGrass = true;
					continue;
				}
				if (foundGrass)
				{
					if (GetCellSourceId(currentCellPosition) == -1)
					{
						SetCell(currentCellPosition, 1, dirt.TilesetCoordinates);
					}
				}
			}
		}
	}

	private void GenerateUnderground(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_dirt_wall");
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				double noise2D = CompositeNoise(noiseFunction, x, y);
				float depthFactor = Mathf.Clamp((float) (y - yStart) / (float) yEnd, 0.0f, 1.0f);
				noise2D = Math.Pow(noise2D, 0.45) * depthFactor;
				Block dirtWall = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_dirt_wall");
				if (dirtWall.CanSpawnAt(y, noise2D))
				{
					SetCell(currentCellPosition, 1, dirtWall.TilesetCoordinates);
				}
			}
		}
	}

	private void GenerateCoalOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_coal");
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				double noise2D = CompositeNoise(noiseFunction, x, y);
				Block coal = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_coal");
				if (coal.CanSpawnAt(y, noise2D))
				{
					SetCell(currentCellPosition, 1, coal.TilesetCoordinates);
				}
			}
		}
	}

	private void GenerateIronOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_iron");
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				double noise2D = CompositeNoise(noiseFunction, x, y);
				Block iron = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_iron");
				if (iron.CanSpawnAt(y, noise2D))
				{
					SetCell(currentCellPosition, 1, iron.TilesetCoordinates);
				}
			}
		}
	}

	private void GenerateDiamondOre(ref ChunkArea areaToBeChunked, ref FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(ref areaToBeChunked, "block_diamond");
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = areaToBeChunked.UpperLeftCorner.X; x < areaToBeChunked.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				double noise2D = CompositeNoise(noiseFunction, x, y);
				Block diamond = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_diamond");
				if (diamond.CanSpawnAt(y, noise2D))
				{
					SetCell(currentCellPosition, 1, diamond.TilesetCoordinates);
				}
			}
		}
	}

	public override void OrchestrateChunkGeneration(Vector2 coordinates, int chunkSize, FastNoiseLite noiseFunction)
	{
		// This returns the Vector with width and height of viewport 
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
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResourceFromCoordinates(ref atlasCoords);
		if (!block.IsBreakable || block.Id == "block_air")
		{
			return;
		}
		GD.Print($"Destroyed: {atlasCoords} = {block.Name}");
		EraseCell(mouseCoordinates);
		playersInventory.AddNewBlock(block);
	}

	public override void TryPlaceBlock(Vector2I mouseCoordinates, ref Inventory playersInventory)
	{
		// This cell is occupied
		if (GetCellSourceId(mouseCoordinates) != -1)
		{
			return;
		}

		// Can return something already
		string blockId = playersInventory.UseCurrentItem();
		if (blockId is null)
		{
			return;
		}
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResource(blockId);
		GD.Print($"Got: {blockId} = {block}");
		SetCell(mouseCoordinates, 1, block.TilesetCoordinates);
	}

	public override void _Process(double delta)
	{
	}
}
