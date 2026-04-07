using Godot;
using System;
using System.Collections.Generic;

public partial class WorldManager : Node
{
	private Player _player;
	private Timer _mobSpawnTimer;
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

	public virtual void OnBlockDestroyed(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new(x, y)));
		CurrentBiome.TryDestroyBlock(mappedCoordinates, ref _player.Inventory);
	}

	public virtual void OnBlockPlaced(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new(x, y)));
		CurrentBiome.TryPlaceBlock(mappedCoordinates, ref _player.Inventory);
	}
	
	public override void _Ready()
	{
		// TODO: After multiple biomes, do a lazy load.
		// Initialize function parameters and instantiate biomes 
		NoiseFunction.Frequency = NoiseFunctionFrequency;
		NoiseFunction.NoiseType = NoiseFunctionNoiseType;
		Biomes = new() {
			[BiomeNames.Overworld] = GD.Load<PackedScene>("res://World/Overworld/PlainsBiome.tscn")
		};
		CurrentBiome = Biomes[BiomeNames.Overworld].Instantiate() as Biome;
		AddChild(CurrentBiome);
		CurrentBiome.LoadTextures();
		CurrentBiome.OrchestrateChunkGeneration(Vector2.Zero, ChunkSize, NoiseFunction);

		// Spawn player
		_player = GD.Load<PackedScene>("res://Entities/Player/Player.tscn").Instantiate() as Player;
		_player.BlockDestroyed += OnBlockDestroyed;
		_player.BlockPlaced += OnBlockPlaced;
		AddChild(_player);

		_mobSpawnTimer = GetNode<Timer>("MobSpawnTimer/Timer");
		_mobSpawnTimer.Start();
		_mobSpawnTimer.Timeout += OnMobSpawnTimeout;
	}

	private void OnMobSpawnTimeout()
	{
		bool mobSpawned = CurrentBiome.OrchestrateMobGeneration(_player.GlobalPosition);
	}

	public override void _Process(double delta)
	{
	}
}
