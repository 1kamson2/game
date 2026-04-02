using Godot;
using System;
using System.Collections.Generic;

public partial class WorldManager : Node
{
	public Dictionary<BiomeNames, PackedScene> Biomes { get; private set; }
	public Biome CurrentBiome { get; set; }
	// Define how big chunk is, the chunk will be of size ChunkSize x ChunkSize
	[Export] public int ChunkSize { get; set; } = int.MinValue;
	// Define the Seed for the world
	[Export] public int Seed { get; set; } = int.MinValue;
	// Define frequency for the noise function
	[Export] public float NoiseFunctionFrequency { get; set; } = float.MinValue;
	// Define noise type for function
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseFunctionNoiseType { get ; set; } = FastNoiseLite.NoiseTypeEnum.Perlin;
	// Define noise function, used for chunk generation
	[Export] public FastNoiseLite NoiseFunction { get; set; } = new FastNoiseLite();
	public void ScheduleChunkUnload() {}
	public void ScheduleChunkLoad() {}
	public void ScheduleChunkSave() {}
	public void ScheduleWorldGeneration() {}
	public void ScheduleMobGeneration() {}
	public void ScheduleEventGeneration() {}
	public void ScheduleLootGeneration() {}

	// FIXME: It works, but it is completely cooked.
	public virtual void OnBlockDestroyed(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new Vector2(x, y)));
		CurrentBiome.EraseCell(mappedCoordinates);
	}

	// FIXME: It works, but it is completely cooked.
	// FIXME: A block's hitbox is way smaller than texture.
	public virtual void OnBlockPlaced(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new Vector2(x, y)));
		CurrentBiome.SetCell(mappedCoordinates, 1, CurrentBiome.BiomeParts[BiomePartNames.SolidGround].AtlasCoordinates);
	}
	
	public override void _Ready()
	{
		// TODO: After multiple biomes, do a lazy load.
		NoiseFunction.Frequency = NoiseFunctionFrequency;
		NoiseFunction.NoiseType = NoiseFunctionNoiseType;
		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		player.BlockDestroyed += OnBlockDestroyed;
		player.BlockPlaced += OnBlockPlaced;
		Biomes = new() {
			[BiomeNames.Overworld] = GD.Load<PackedScene>("res://World/Overworld/PlainsBiome.tscn")
		};

		PlainsBiome plainsBiome = (PlainsBiome) Biomes[BiomeNames.Overworld].Instantiate();
		CurrentBiome = plainsBiome;
		AddChild(plainsBiome);
		plainsBiome.LoadTileTextures();
		plainsBiome.OrchestrateChunkGeneration(Vector2.Zero, ChunkSize, NoiseFunction);
	}

	public override void _Process(double delta)
	{
	}
}
