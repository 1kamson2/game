using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public static class GlobalWorldStateValues
{
	public static bool IsInHardmode
	{
		get => _wasBossSlain;
	}
	private static bool _wasBossSlain = false;
	private static float _maxModifierValue = 4.0f;
	private static float _modifierIncrement = 0.05f;
	private static float _mobHealthModifier = 1.0f;
	private static float _mobDamageModifier = 1.0f;
	private static float _mobSpeedModifier = 1.0f;

	public static void OnBossSlain()
	{
		_wasBossSlain = true;
		GD.Print("Boss was slain.");
	}
	// Mob<Field>Modifier is used to make game harder. It applies to the next spawned instance of mobs.
	public static float MaxModifierValue
	{
		get => IsInHardmode ? 2 * _maxModifierValue : _maxModifierValue;
	}

	public static float MobHealthModifier
	{
		get
		{
			float old = _mobHealthModifier;
			_mobHealthModifier = Math.Clamp(_mobHealthModifier + _modifierIncrement, old, MaxModifierValue);
			return old;
		}
	}
	public static float MobDamageModifier
	{
		get
		{
			float old = _mobDamageModifier;
			_mobDamageModifier = Math.Clamp(_mobDamageModifier + _modifierIncrement, old, MaxModifierValue);
			return old;
		}
	}
	public static float MobSpeedModifier
	{
		get
		{
			float old = _mobSpeedModifier;
			_mobSpeedModifier = Math.Clamp(_mobSpeedModifier + _modifierIncrement, old, MaxModifierValue);
			return old;
		}

	}
}

public partial class WorldManager : ResourceManager<Biome>
{
	private Player _player;
	private Timer _mobSpawnTimer;
	private Timer _mobAttackTimer;
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
	[Export] private int _maxMobCount = 25;
	[Signal] public delegate void MaxMobCountAchievedEventHandler();
	public Godot.Vector2 ViewportSize { get; set; }
	public ChunkArea CurrentArea { get; set; }
	public void ScheduleChunkUnload() { }
	public void ScheduleChunkLoad() { }
	public void ScheduleChunkSave() { }
	public void ScheduleWorldGeneration()
	{
		CurrentBiome.OrchestrateChunkGeneration(CurrentArea, NoiseFunction);
		CurrentBiome.OrchestrateStructureGeneration(CurrentArea, NoiseFunction);
		ScheduleLootGeneration();
	}

	public void ScheduleMobGeneration()
	{
		if (EntityGlobalValues.CurrentMobCount > _maxMobCount)
		{
			GD.Print($"Mob cannot be spawned it will exceed the threshold: {_maxMobCount} mobs spawned.");
			EmitSignal(SignalName.MaxMobCountAchieved);
			return;
		}
		Mob mobSpawned = CurrentBiome.OrchestrateMobGeneration(CurrentArea, NoiseFunction, _player.GlobalPosition);
		if (mobSpawned is null)
		{
			return;
		}

		_mobAttackTimer.Timeout += mobSpawned.OnAttack;
		MaxMobCountAchieved += mobSpawned.ShouldDespawn;
		if (mobSpawned is MobBoss)
		{
			mobSpawned.Death += GlobalWorldStateValues.OnBossSlain;
		}
	}
	public void ScheduleEventGeneration() { }
	public void ScheduleLootGeneration()
	{
		List<Pot> potRefs = CurrentBiome.OrchestrateLootGeneration(CurrentArea, NoiseFunction);
		foreach (var pot in potRefs)
		{
			pot.ContainerLooted += OnContainerLooted;
		}
	}

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
		}
	}

	public virtual void OnBlockPlaced(int x, int y)
	{
		Vector2I mappedCoordinates = CurrentBiome.LocalToMap(CurrentBiome.ToLocal(new(x, y)));
		if (IsCursorInValidRange())
		{
			CurrentBiome.TryPlaceBlock(mappedCoordinates, ref _player.Inventory);
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
		ViewportSize = GetViewport().GetVisibleRect().Size;
		CurrentArea = new ChunkArea(Godot.Vector2.Zero, ViewportSize, ChunkSize);
		// Spawn player
		_player = GD.Load<PackedScene>("res://Entities/Player/Player.tscn").Instantiate<Player>();
		_player.BlockDestroyed += OnBlockDestroyed;
		_player.BlockPlaced += OnBlockPlaced;
		_player.Death += GlobalManagers.Instance.OnPlayerDeath;
		AddChild(_player);

		_mobSpawnTimer = GetNode<Timer>("../MobSpawnTimer/Timer");
		_mobSpawnTimer.Start();
		_mobSpawnTimer.Timeout += ScheduleMobGeneration;

		_mobAttackTimer = GetNode<Timer>("../MobAttackTimer/Timer");
		_mobAttackTimer.Start();
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
		}
		else
		{
			GD.PrintErr("Current Biome is null");
		}
	}

	private bool CanContainerBeLooted()
	{
		return ContainerGlobalValues.LastContainerInteraction.GlobalPosition.DistanceTo(_player.GlobalPosition) <= 60;
	}
	private void OnContainerLooted(string loot_id, int amount)
	{
		if (!CanContainerBeLooted()) { return; }
		UsableItem item = GlobalManagers.Instance.GetManager<ItemManager>().GetResource(loot_id);
		ContainerGlobalValues.LastContainerInteraction.QueueFree();
		ContainerGlobalValues.LastContainerInteraction = null;
		_player.Inventory.AddNewItem(item);
	}

	public override void _Process(double delta)
	{

	}
}
