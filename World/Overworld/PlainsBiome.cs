using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// TODO: Trees render incorrectly.
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
					if (GD.Randf() > structure.SpawnChance) { continue; }
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

	public override void OrchestrateStructureGeneration(ChunkArea area, FastNoiseLite noiseFunction)
	{
		GenerateTreeStructures(area);
	}

	public override bool OrchestrateMobGeneration(ChunkArea area, FastNoiseLite noiseFunction, Vector2 playerPosition)
	{
		// Spawn a mob
		if (GD.Randf() <= 0.5)
		{
			Mob mobInstance = GD.Load<PackedScene>("res://Entities/Mobs/Mob.tscn").Instantiate<Mob>();
			int randomDistance = GD.RandRange(-500, 500);
			Vector2 mobPosition = playerPosition;
			var horizontalRange = area.HorizontalRange();
			var verticalRange = area.VerticalRange();
			mobPosition.X = Math.Clamp(mobPosition.X + randomDistance, horizontalRange.lo, horizontalRange.hi);
			mobPosition.Y = Math.Clamp(mobPosition.Y + randomDistance, verticalRange.lo, verticalRange.hi);
			mobInstance.GlobalPosition = mobPosition;
			AddChild(mobInstance);
			return true;
		}
		return false;
	}
	public override void OrchestrateLootGeneration(ChunkArea area, FastNoiseLite noiseFunction) { }

	public override void _Ready()
	{
		_structureRegistry = new Dictionary<string, PackedScene>
		{
			{ "structure_oak_tree",  GD.Load<PackedScene>("res://Resources/Structures/OakTreeStructure.tscn") },
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
				// TODO: This can be probably cached
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
		GenerateUnderground(area, noiseFunction, "block_bedrock", canReplaceBlock: true);
		GenerateOres(area, noiseFunction, "block_coal");
		GenerateOres(area, noiseFunction, "block_iron");
		GenerateOres(area, noiseFunction, "block_diamond");
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
