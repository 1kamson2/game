using Godot;


public partial class WorldManager : ResourceManager<Biome>
{
	private Player _player;
	private Timer _mobSpawnTimer;
	// public Dictionary<BiomeNames, PackedScene> Biomes { get; private set; }
	public Biome CurrentBiome { get; set; }
	// Define how big chunk is, the chunk will be of size ChunkSize x ChunkSize
	private static string _pathToBiomes = "res://World/Overworld/";
	private static string[] _biomeScenes = {
		"PlainsBiome.tscn",
	};
	[Export] public int ChunkSize { get; set; } = int.MinValue;
	// Define the Seed for the world
	[Export] public int Seed { get; set; } = int.MinValue;
	// Define frequency for the noise function
	[Export] public float NoiseFunctionFrequency { get; set; } = float.MinValue;
	// Define noise type for function
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseFunctionNoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Perlin;
	// Define noise function, used for chunk generation
	[Export] public FastNoiseLite NoiseFunction { get; set; } = new FastNoiseLite();
	public Godot.Vector2 ViewportSize { get; set; }
	public ChunkArea CurrentArea { get; set; }
	public void ScheduleChunkUnload() { }
	public void ScheduleChunkLoad() { }
	public void ScheduleChunkSave() { }
	public void ScheduleWorldGeneration()
	{
		CurrentBiome.OrchestrateChunkGeneration(CurrentArea, NoiseFunction);
		CurrentBiome.OrchestrateStructureGeneration(CurrentArea, NoiseFunction);
	}
	public void ScheduleMobGeneration() { }
	public void ScheduleEventGeneration() { }
	public void ScheduleLootGeneration() { }

	public bool IsCursorInValidRange()
	{
		return _player.GlobalPosition.DistanceTo(GetGlobalMousePosition()) < 100.0f;
	}

	public virtual void OnBlockDestroyed(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new(x, y)));
		if (IsCursorInValidRange())
		{
			CurrentBiome.TryDestroyBlock(mappedCoordinates, ref _player.Inventory);
			_player.Inventory.UpdateRecipesState();
		}
	}

	public virtual void OnBlockPlaced(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new(x, y)));
		if (IsCursorInValidRange())
		{
			CurrentBiome.TryPlaceBlock(mappedCoordinates, ref _player.Inventory);
			_player.Inventory.UpdateRecipesState();
		}
	}

	public override bool RegisterResource(Biome resource)
	{
		if (resource is null)
		{
			GD.PrintErr("BiomeTileset resource is null");
			return false;
		}

		if (string.IsNullOrEmpty(resource.Id))
		{
			GD.PrintErr($"{resource.Id} is not a valid ID for BiomeTileset instance.");
			return false;
		}

		if (Registry.ContainsKey(resource.Id))
		{
			GD.PrintErr($"Resource with {resource.Id} already exists in the Registry.");
			GD.PrintErr("If you want to reassign it, then UnregisterResource the already existing resource and Register it again.");
			return false;
		}

		Registry.Add(resource.Id, resource);
		return true;
	}


	public override bool UnregisterResource(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			GD.PrintErr($"`{id}` is not a valid ID.");
		}
		return Registry.Remove(id);
	}

	public override U GetResourceAs<U>(string id)
	{
		Biome biome = GetResource(id);
		if (biome is null)
		{
			GD.PrintErr($"Couldn't find the tileset with the id: `{id}`");
			return null;
		}
		return biome as U;

	}

	public override Biome GetResource(string id)
	{
		bool result = Registry.TryGetValue(id, out Biome biome);
		if (!result)
		{
			GD.PrintErr($"Couldn't find the tileset with the id: `{id}`");
			return null;
		}
		return biome;
	}

	public override void _Ready()
	{
		// FIXME: Potential problem
		ViewportSize = GetViewport().GetVisibleRect().Size;
		CurrentArea = new ChunkArea(Godot.Vector2.Zero, ViewportSize, ChunkSize);
		// Spawn player
		_player = GD.Load<PackedScene>("res://Entities/Player/Player.tscn").Instantiate<Player>();
		_player.BlockDestroyed += OnBlockDestroyed;
		_player.BlockPlaced += OnBlockPlaced;
		AddChild(_player);

		_mobSpawnTimer = GetNode<Timer>("../MobSpawnTimer/Timer");
		_mobSpawnTimer.Start();
		_mobSpawnTimer.Timeout += OnMobSpawnTimeout;
		// TODO: After multiple biomes, do a lazy load.
		// Initialize function parameters and instantiate biomes 
		NoiseFunction.Frequency = NoiseFunctionFrequency;
		NoiseFunction.NoiseType = NoiseFunctionNoiseType;
		// Load the biome scenes and register them
		{
			foreach (var biomeScene in _biomeScenes)
			{
				Biome biome = GD.Load<PackedScene>($"{_pathToBiomes}{biomeScene}").Instantiate<Biome>();
				GD.Print($"Trying to register: {biome.Id}");
				AddChild(biome);
				RegisterResource(biome);
			}
		}
		CurrentBiome = GetResource("biome_plains");
		if (CurrentBiome is not null)
		{
			GD.Print($"Generating chunks: {CurrentBiome.Id}");
			ScheduleWorldGeneration();
		} else
		{
			GD.PrintErr("Current Biome is null");
		}
	}

	private void OnMobSpawnTimeout()
	{
		bool mobSpawned = CurrentBiome.OrchestrateMobGeneration(CurrentArea, NoiseFunction, _player.GlobalPosition);
	}

	public override void _Process(double delta)
	{
	}
}
