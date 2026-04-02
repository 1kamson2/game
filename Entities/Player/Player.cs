using Godot;
using System;

public partial class Player : Entity
{
    [Signal] public delegate void BlockDestroyedEventHandler(int x, int y);
	[Signal] public delegate void BlockPlacedEventHandler(int x, int y);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("player");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		OnMouseLeftClick();
		OnMouseRightClick();
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

	public void OnMouseLeftClick()
	{
		if (Input.IsActionJustPressed("left_click"))
		{
			Vector2 mouseGlobalPosition = GetGlobalMousePosition();
			GD.Print("Left clicked on: ", mouseGlobalPosition);
			EmitSignal(SignalName.BlockDestroyed, mouseGlobalPosition.X, mouseGlobalPosition.Y);
		}
		
	}

	public void OnMouseRightClick()
	{
		if (Input.IsActionJustPressed("right_click"))
		{
			Vector2 mouseGlobalPosition = GetGlobalMousePosition();
			EmitSignal(SignalName.BlockPlaced, mouseGlobalPosition.X, mouseGlobalPosition.Y);
		}
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
