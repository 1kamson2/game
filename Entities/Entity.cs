using Godot;
using System;

public abstract partial class Entity : CharacterBody2D
{
	[Export] public string EntityName { get; set; } = "Entity";
	[Export] public float CurrentHealth { get; protected set; } = 100;
	[Export] public float MaxHealth { get; protected set; } = 100;
	[Export] public float BaseSpeed { get; protected set; } = 400;
	[Export] public float Damage { get; protected set; } = 10;
	[Export] public float JumpForce { get; set; } = 300;
	public Vector2 MovementDirection { get; set; } = new Vector2(0, 0);
	
	public abstract void UpdateAnimation(double delta);
	// Update the position on the X-axis
	public abstract void UpdateHorizontalPosition(double delta);
	// Update the position on the Y-axis
	public abstract void UpdateVerticalPosition(double delta);
	// Free the entity if no longer needed
	
	// The default definition of physics process.
	public override void _PhysicsProcess(double delta)
	{
		UpdateHorizontalPosition(delta);
		UpdateVerticalPosition(delta);
		MoveAndSlide();
	}
	public abstract void FreeEntity();
}
