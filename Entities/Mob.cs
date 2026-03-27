using Godot;
using System;

public partial class Mob : Entity
{
	public Node2D Target { get; set; }
	public override void _Ready()
	{
		Target = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}
	
	public override void UpdateVerticalPosition(double delta) 
	{
		Vector2 currentVelocity = Velocity;
		if (!IsOnFloor())
		{
			currentVelocity.Y += 980 * (float)delta; 
		}
		if (IsOnFloor())
		{
			currentVelocity.Y = -JumpForce;
		}
		Velocity = currentVelocity;
	}
	
	public override void UpdateHorizontalPosition(double delta) 
	{
		// TODO:
		// Make this Interface, because:
		// 1. Is the player in the view, e.g.: Is the distance less than 100? If yes then track, otherwise don't.
		// 2. There can be different types of movement, e.g.: Rushing, Stalking, etc.
		
		Vector2 currentVelocity = Velocity;
		// Find the movement direction to get to the player
		MovementDirection = GlobalPosition.DirectionTo(Target.GlobalPosition);
		currentVelocity.X = MovementDirection.X * BaseSpeed;
		Velocity = currentVelocity;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		// right now no processing
	}
	
	public override void UpdateAnimation(double delta)
	{
		// right now no animations
	}
	
	public override void FreeEntity()
	{
		QueueFree();
	}
}
