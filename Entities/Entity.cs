using Godot;
using System;

public enum EntityState
{
	Attacking,
	Idling,
	Mining,
	Placing,
	Running,
	Jumping,
	FallingDown,
	BeingHit,
}

/// <summary>
/// EntityGlobalValues are used to define the global state of entity variables
/// </summary>
public static class EntityGlobalValues
{
	/// <summary>
	/// EntityTargetedByPlayer defines what entity was targeted by the player
	/// </summary>
	public static Entity EntityTargetedByPlayer { get; set; } = null;
	/// <summary>
	/// PlayerTargetedByEntity defines what entity targeted the player.
	/// </summary>
	public static Entity PlayerTargetedByEntity { get; set; } = null;
	public static int CurrentMobCount { get; set; } = 0;
	public static void FreeEntityTargetedByPlayer() => EntityTargetedByPlayer = null;
	public static void FreePlayerTargetedByEntity() => PlayerTargetedByEntity = null;
}

public interface IEntityCanAttack
{
	public abstract void OnAttack(float damageAmount, Entity target);
}

public interface IEntityIsAttackable
{
	public abstract void OnBeingAttacked(float damageAmount, Entity target);
}

/// <summary>
/// Entity class defines an abstract class for any entity in the game.
/// </summary>
public abstract partial class Entity : CharacterBody2D
{
	[Signal] public delegate void BlockDestroyedEventHandler(int x, int y);
	[Signal] public delegate void BlockPlacedEventHandler(int x, int y);
	[Signal] public delegate void EntityAttackedEventHandler(float damageAmount, Entity target);
	[Signal] public delegate void DeathEventHandler();
	protected AnimatedSprite2D EntityAnimation { get; set; }
	protected static Color HitModulate = new(10.0f, 1.0f, 1.0f, 1.0f);
	protected EntityState _currentEntityState = EntityState.Idling;
	protected virtual EntityState CurrentEntityState 
	{ 
		get => _currentEntityState; 
		set => _currentEntityState = CheckIfAnimationLocked() ? _currentEntityState : value; 
	}
	/// <summary>
	/// _baseHealth defines the default HP value for entity. Other stats such as buffs/nerfs should be evaluated from this value.
	/// </summary>
	[Export] protected float _baseHealth = 100;
	/// <summary>
	/// CurrentHealth defines the current HP for the entity.
	/// </summary>
	public float CurrentHealth { get; protected set; }
	/// <summary>
	/// MaxHealth defines the maximum possible HP for the entity after applying buffs/nerfs (e.g.: from different items).
	/// </summary>
	public float MaxHealth { get; protected set; }
	/// <summary>
	/// _baseSpeed defines the base speed for entity. Other stats such as buffs/nerfs should evalute from this value.
	/// </summary>
	[Export] protected float _baseSpeed = 100;
	/// <summary>
	/// CurrentSpeed defines the speed for entity after applying buffs/nerfs (e.g.: from different items).
	/// </summary>
	public float CurrentSpeed { get; protected set; }
	public float MaxSpeed { get; protected set; }
	/// <summary>
	/// _baseDamage defines the base damage for entity. Other stats such as buffs/nerfs should evalute from this value.
	/// </summary>
	[Export] protected float _baseDamage = 10;
	/// <summary>
	/// CurrentDamage defines the damage for entity after applying buffs/nerfs (e.g.: from different items).
	/// </summary>	
	public float CurrentDamage { get; protected set; }
	public float MaxDamage { get; protected set; }
	/// <summary>
	/// _baseJumpForce defines the base jump force for entity. Other stats such as buffs/nerfs should evalute from this value.
	/// </summary>
	[Export] protected float _baseJumpForce = 200;
	/// <summary>
	/// CurrentJumpForce defines the jump force for entity after applying buffs/nerfs (e.g.: from different items).
	/// </summary>
	public float CurrentJumpForce { get; set; }
	public float MaxJumpForce { get; protected set; }

    public override void _Ready()
    {
        CurrentHealth = _baseHealth;
		MaxHealth = _baseHealth;
		CurrentSpeed = _baseSpeed;
		MaxSpeed = _baseSpeed;
		CurrentDamage = _baseDamage;
		MaxDamage = _baseDamage;
		CurrentJumpForce = _baseJumpForce;
		MaxJumpForce = _baseJumpForce;
		EntityAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }
	
	public abstract void UpdateAnimation(double delta);
	
	/// <summary>
	/// Check if the current animation CAN'T be interrupted, e.g.: breaking blocks, placing blocks, attacking, etc.
	/// </summary>
	/// <returns>True if it CAN'T be interrupted, otherwise false.</returns>
	public abstract bool CheckIfAnimationLocked();
	public abstract void FreeEntity();
}
