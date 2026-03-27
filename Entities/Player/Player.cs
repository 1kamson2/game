using Godot;
using System;

public partial class Player : Entity
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("player");
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
	
	public override void UpdateHorizontalPosition(double delta)
	{
		MovementDirection = Input.GetVector("go_left", "go_right", "jump", "drop_faster");
		Vector2 currentVelocity = Velocity;
		currentVelocity.X = MovementDirection.X * BaseSpeed;
		Velocity = currentVelocity;
	}
	
	public override void UpdateVerticalPosition(double delta)
	{
		Vector2 currentVelocity = Velocity;
		if (!IsOnFloor())
		{
			currentVelocity.Y += 980 * (float)delta; 
		}
		if (IsOnFloor() && MovementDirection.Y < 0)
		{
			currentVelocity.Y = -JumpForce;
		}
		Velocity = currentVelocity;
	}
	
	public override void FreeEntity()
	{
		QueueFree();
	}
}
