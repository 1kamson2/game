using Godot;
using System;

public partial class Player : Entity, IEntityCanAttack, IEntityIsAttackable
{
	public Inventory Inventory;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		AddToGroup("player");
		Inventory = GetNode<Inventory>("Inventory");
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

	public void OnBeingAttacked(float damageAmount, Entity target)
	{
		
	}

	public void OnAttack(float damageAmount, Entity target)
	{
		EmitSignal(SignalName.EntityAttacked, damageAmount, target);
	}

	public void OnMouseLeftClick()
	{
		if (Input.IsActionJustPressed("left_click"))
		{
			if (EntityGlobalValues.EntityTargetedByPlayer is not null)
			{
				OnAttack(CurrentDamage, EntityGlobalValues.EntityTargetedByPlayer);
			}
			else
			{
				Vector2 mouseGlobalPosition = GetGlobalMousePosition();
				EmitSignal(SignalName.BlockDestroyed, mouseGlobalPosition.X, mouseGlobalPosition.Y);
			}
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

    public override void _PhysicsProcess(double delta)
    {
		Vector2 currentVelocity = Velocity;
		if (!IsOnFloor())
		{
			currentVelocity.Y += 980 * (float) delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			currentVelocity.Y = -CurrentJumpForce;
		}

		float direction = Input.GetAxis("go_left", "go_right");
		if (direction != 0)
		{
			currentVelocity.X = direction * CurrentSpeed;
		}
		else
		{
			currentVelocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
		}
		Velocity = currentVelocity;
		MoveAndSlide();
    }
	
	public override void FreeEntity()
	{
		QueueFree();
	}
}
