using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlainsBiome : Biome
{
	public override void OrchestrateEventGeneration(ChunkArea area, FastNoiseLite noiseFunction) { }

	private void GenerateTreeStructures(ChunkArea area)
	{
		Block grass = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_grass");
		IEnumerable<string> treeKeys = _structureRegistry.Keys.Where(k => k.Contains("tree"));
		var horizontalRange = area.HorizontalRange();
		var verticalRange = area.VerticalRange();
		foreach (string treeKey in treeKeys)
		{
			Structure structure = _structureRegistry[treeKey].Instantiate<Structure>();
			structure.Load();
			for (int x = horizontalRange.lo; x < horizontalRange.hi; x += structure.SpaceInBetween)
			{
				for (int y = verticalRange.lo; y < verticalRange.hi; y++)
				{
					Vector2I currentCellPosition = new(x, y);
					Vector2I currentAtlasCoordinates = GetCellAtlasCoords(currentCellPosition);
					if (currentAtlasCoordinates != grass.TilesetCoordinates) { continue; }
					if (rndGenerator.NextDouble() > structure.SpawnChance) { continue; }
					var mappedTilePositions = structure.TilesGlobalPositions(currentCellPosition);
					for (int idx = 0; idx < structure.TilesPositions.Count(); idx++)
					{
						SetCell(mappedTilePositions[idx], 1, structure.TileAtlasCoords[idx]);
					}
					// Tree generated, exit.
					break;
				}
			}
		}

	}

	private void GenerateCaves(ChunkArea area)
	{
		IEnumerable<string> caveKeys = _structureRegistry.Keys.Where(k => k.Contains("cave"));
		var horizontalRange = area.HorizontalRange();
		var verticalRange = area.VerticalRange();
		foreach (string caveKey in caveKeys)
		{
			Structure structure = _structureRegistry[caveKey].Instantiate<Structure>();
			structure.Load();
			if (rndGenerator.NextDouble() > structure.SpawnChance) { continue; }
			var horizontalSpawnRange = structure.HorizontalSpawnRange;
			var verticalSpawnRange = structure.VerticalSpawnRange;
			int x = Mathf.Clamp(rndGenerator.Next(horizontalSpawnRange.lo, horizontalSpawnRange.hi), horizontalRange.lo, horizontalRange.hi);
			int y = Mathf.Clamp(rndGenerator.Next(verticalSpawnRange.lo, verticalSpawnRange.hi), verticalRange.lo, verticalRange.hi);
			var mappedTilePositions = structure.TilesGlobalPositions(new(x, y));
			for (int idx = 0; idx < structure.TilesPositions.Count(); idx++)
			{
				SetCell(mappedTilePositions[idx], 1, structure.TileAtlasCoords[idx]);
			}
		}
	}

	private void GenerateDungeons(ChunkArea area)
	{
		IEnumerable<string> dungeonKeys = _structureRegistry.Keys.Where(k => k.Contains("dungeon"));
		var horizontalRange = area.HorizontalRange();
		var verticalRange = area.VerticalRange();
		foreach (string dungeonKey in dungeonKeys)
		{
			Structure structure = _structureRegistry[dungeonKey].Instantiate<Structure>();
			structure.Load();
			if (rndGenerator.NextDouble() > structure.SpawnChance) { continue; }
			var horizontalSpawnRange = structure.HorizontalSpawnRange;
			var verticalSpawnRange = structure.VerticalSpawnRange;
			int x = Mathf.Clamp(rndGenerator.Next(horizontalSpawnRange.lo, horizontalSpawnRange.hi), horizontalRange.lo, horizontalRange.hi);
			int y = Mathf.Clamp(rndGenerator.Next(verticalSpawnRange.lo, verticalSpawnRange.hi), verticalRange.lo, verticalRange.hi);
			var mappedTilePositions = structure.TilesGlobalPositions(new(x, y));
			for (int idx = 0; idx < structure.TilesPositions.Count(); idx++)
			{
				SetCell(mappedTilePositions[idx], 1, structure.TileAtlasCoords[idx]);
			}
		}
	}

	private void GenerateMines(ChunkArea area)
	{
		IEnumerable<string> mineKeys = _structureRegistry.Keys.Where(k => k.Contains("mine"));
		var horizontalRange = area.HorizontalRange();
		var verticalRange = area.VerticalRange();
		foreach (string mineKey in mineKeys)
		{
			Structure structure = _structureRegistry[mineKey].Instantiate<Structure>();
			structure.Load();
			if (rndGenerator.NextDouble() > structure.SpawnChance) { continue; }
			var horizontalSpawnRange = structure.HorizontalSpawnRange;
			var verticalSpawnRange = structure.VerticalSpawnRange;
			int x = Mathf.Clamp(rndGenerator.Next(horizontalSpawnRange.lo, horizontalSpawnRange.hi), horizontalRange.lo, horizontalRange.hi);
			int y = Mathf.Clamp(rndGenerator.Next(verticalSpawnRange.lo, verticalSpawnRange.hi), verticalRange.lo, verticalRange.hi);
			var mappedTilePositions = structure.TilesGlobalPositions(new(x, y));
			for (int idx = 0; idx < structure.TilesPositions.Count(); idx++)
			{
				SetCell(mappedTilePositions[idx], 1, structure.TileAtlasCoords[idx]);
			}
		}
	}
	public override void OrchestrateStructureGeneration(ChunkArea area, FastNoiseLite noiseFunction)
	{
		GenerateTreeStructures(area);
		GenerateCaves(area);
		GenerateDungeons(area);
		GenerateMines(area);
	}

	public override Mob OrchestrateMobGeneration(ChunkArea area, FastNoiseLite noiseFunction, Vector2 playerPosition)
	{
		int mobIndex = rndGenerator.Next(MobScenePool.Count);
		// Try to spawn mob
		Mob mobInstance = MobScenePool[mobIndex].Instantiate<Mob>();
		if (rndGenerator.NextDouble() > mobInstance.SpawnChance)
		{
			mobInstance.QueueFree();
			return null;
		}
		int offset = rndGenerator.Next(200, 400);
		int randomXOffset = rndGenerator.NextDouble() <= 0.5 ? -offset : offset;
		int randomYOffset = rndGenerator.Next(-25, 25);
		mobInstance.GlobalPosition = new(playerPosition.X + randomXOffset, playerPosition.Y + randomYOffset);
		AddChild(mobInstance);
		return mobInstance;
	}
	public override List<Pot> OrchestrateLootGeneration(ChunkArea area, FastNoiseLite noiseFunction)
	{
		PackedScene potScene = GD.Load<PackedScene>("res://Entities/Pot.tscn");
		List<Pot> potRefs = Enumerable.Range(0, 50).Select(_ =>
		{
			var pot = potScene.Instantiate<Pot>();
			pot.Position = MapToLocal(new(rndGenerator.Next(-400, 400), rndGenerator.Next(-200, 200)));
			pot.StoredItem = GlobalManagers.Instance.GetManager<ItemManager>().GetRandomItemId();
			pot.Amount = 1;
			return pot;
		}).ToList();

		foreach (var pot in potRefs)
		{
			AddChild(pot);
		}
		return potRefs;
	}

	public override void _Ready()
	{
		_structureRegistry = new Dictionary<string, PackedScene>
		{
			{ "structure_oak_tree",  GD.Load<PackedScene>("res://Resources/Structures/OakTreeStructure.tscn") },
			{ "structure_big_cave",  GD.Load<PackedScene>("res://Resources/Structures/BigCaveStructure.tscn") },
			{ "structure_dungeon",  GD.Load<PackedScene>("res://Resources/Structures/DungeonStructure.tscn") },
			{ "structure_house",  GD.Load<PackedScene>("res://Resources/Structures/HouseStructure.tscn") },
			{ "structure_large_cave",  GD.Load<PackedScene>("res://Resources/Structures/LargeCaveStructure.tscn") },
			{ "structure_medium_cave",  GD.Load<PackedScene>("res://Resources/Structures/MediumCaveStructure.tscn") },
			{ "structure_mine",  GD.Load<PackedScene>("res://Resources/Structures/MineStructure.tscn") },
		};

		MobScenePool = new List<PackedScene>()
		{
			GD.Load<PackedScene>("res://Entities/Mobs/Mob.tscn"),
			GD.Load<PackedScene>("res://Entities/Mobs/MobBoss.tscn"),
			GD.Load<PackedScene>("res://Entities/Mobs/MobStalker.tscn")
		};

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

	private void GenerateSurfaceGrass(ChunkArea area, FastNoiseLite noiseFunction)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(area, "block_grass");
		Block grass = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_grass");
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = area.UpperLeftCorner.X; x < area.LowerRightCorner.X; x++)
			{
				double noise1D = noiseFunction.GetNoise1D(x / 1.33f);
				int groundLevel = (int)((noise1D + 1) * 15) + 30;
				Vector2I currentCellPosition = new Vector2I(x, groundLevel);
				// Check if cell is taken
				if (GetCellSourceId(currentCellPosition) != -1)
				{
					continue;
				}
				SetCell(currentCellPosition, 1, grass.TilesetCoordinates);
			}
		}
	}

	private void GenerateUnderground(ChunkArea area, FastNoiseLite noiseFunction, string block_id,
	bool useDepthFactor = false,
	bool canReplaceBlock = false)
	{
		// WARNING: block_dirt should have the widest boundaries, it defines the generation boundaries for the rest of the blocks
		var (yStart, yEnd) = FindBlockHeightBoundaries(area, "block_dirt");
		Block grass = GlobalManagers.Instance.GetManager<BlockManager>().GetResource("block_grass");
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResource(block_id);
		for (int x = area.UpperLeftCorner.X; x < area.LowerRightCorner.X; x++)
		{
			// First we find the existing grass, after we find it, we can place blocks under
			bool foundGrass = false;
			for (int y = yStart; y < yEnd; y++)
			{
				Vector2I currentCellPosition = new(x, y);
				Vector2I currentAtlasCoordinates = GetCellAtlasCoords(currentCellPosition);

				if (currentAtlasCoordinates == grass.TilesetCoordinates)
				{
					foundGrass = true;
					continue;
				}

				double noise2D = CompositeNoise(noiseFunction, x, y);
				if (useDepthFactor)
				{
					float depthFactor = Mathf.Clamp((float)(y - yStart) / (float)yEnd, 0.0f, 1.0f);
					noise2D *= depthFactor;
				}

				if (foundGrass)
				{
					if (block.CanSpawnAt(y, noise2D))
					{
						if (canReplaceBlock)
						{
							SetCell(currentCellPosition, 1, block.TilesetCoordinates);
						}
						else if (currentAtlasCoordinates.X == -1 && currentAtlasCoordinates.Y == -1)
						{
							SetCell(currentCellPosition, 1, block.TilesetCoordinates);
						}
						continue;
					}
				}
			}
		}
	}

	private void GenerateOres(ChunkArea area, FastNoiseLite noiseFunction, string ore_id)
	{
		var (yStart, yEnd) = FindBlockHeightBoundaries(area, ore_id);
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResource(ore_id);
		for (int y = yStart; y < yEnd; y++)
		{
			for (int x = area.UpperLeftCorner.X; x < area.LowerRightCorner.X; x++)
			{
				Vector2I currentCellPosition = new Vector2I(x, y);
				double noise2D = CompositeNoise(noiseFunction, x, y);

				if (block.CanSpawnAt(y, noise2D))
				{
					SetCell(currentCellPosition, 1, block.TilesetCoordinates);
				}
			}
		}
	}

	public override void OrchestrateChunkGeneration(ChunkArea area, FastNoiseLite noiseFunction)
	{
		GenerateSurfaceGrass(area, noiseFunction);
		GenerateUnderground(area, noiseFunction, "block_dirt");
		GenerateUnderground(area, noiseFunction, "block_stone", canReplaceBlock: true);
		GenerateUnderground(area, noiseFunction, "block_diorite", canReplaceBlock: true);
		GenerateUnderground(area, noiseFunction, "block_dirt_wall", useDepthFactor: true, canReplaceBlock: true);
		GenerateOres(area, noiseFunction, "block_coal");
		GenerateOres(area, noiseFunction, "block_iron");
		GenerateOres(area, noiseFunction, "block_diamond");
		GenerateOres(area, noiseFunction, "block_redstone");
		GenerateOres(area, noiseFunction, "block_emerald");
		GenerateUnderground(area, noiseFunction, "block_bedrock", canReplaceBlock: true);
	}

	public override void TryDestroyBlock(Vector2I mouseCoordinates, ref Inventory playersInventory)
	{
		Vector2I atlasCoords = GetCellAtlasCoords(mouseCoordinates);
		Block block = GlobalManagers.Instance.GetManager<BlockManager>().GetResourceFromCoordinates(atlasCoords);
		if (!block.IsBreakable || block.Id == "block_air")
		{
			return;
		}
		
		EraseCell(mouseCoordinates);
		GD.Print($"Block destroyed: {block.Id}");
		playersInventory.AddNewItem(block);
	}

	public override void TryPlaceBlock(Vector2I mouseCoordinates, ref Inventory playersInventory)
	{
		Vector2I atlasCoords = GetCellAtlasCoords(mouseCoordinates);
		Block blockOccupyingCell = GlobalManagers.Instance.GetManager<BlockManager>().GetResourceFromCoordinates(atlasCoords);

		if (!blockOccupyingCell.IsReplaceable)
		{
			return;
		}

		// Can return something already
		Block block = playersInventory.UseCurrentItemAs<Block>();
		if (block is null)
		{
			return;
		}
		GD.Print($"Placed: {block.Id}");
		SetCell(mouseCoordinates, 1, block.TilesetCoordinates);
	}

	public override void _Process(double delta)
	{
	}
}
