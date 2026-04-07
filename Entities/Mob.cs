using Godot;
using System;
using System.ComponentModel;

public interface IMobController
{
	public Player Target { get; set; }
	[Export] public float MaxDistanceToPlayer { get; set; }
	public Vector2 PhysicsProcessNoAggroed(double delta);
	public Vector2 PhysicsProcessAggroed(double delta);
	public bool IsMobAggroed();
}

public partial class Mob : Entity, IMobController
{
	public Player Target { get; set; }
	[Export] public float MaxDistanceToPlayer { get; set; } = 500.0f;

	public override void _Ready()
	{
		CurrentHealth = _baseHealth;
		MaxHealth = _baseHealth;
		CurrentSpeed = _baseSpeed;
		CurrentDamage = _baseDamage;
		CurrentJumpForce = _baseJumpForce;
		Target = GetTree().GetFirstNodeInGroup("player") as Player;
		Target.EntityAttacked += OnBeingAttacked;
	}

	public Vector2 PhysicsProcessNoAggroed(double delta)
	{
		Vector2 currentVelocity = Velocity;
		if (!IsOnFloor())
		{
			currentVelocity.Y += 980 * (float) delta; 
		}
		else if (IsOnFloor() && GD.Randf() < 0.02f)
		{
			currentVelocity.Y = -CurrentJumpForce;
		}

		if (GD.Randf() < 0.3f)
		{
			currentVelocity.X = (GD.Randf() * 2) * CurrentSpeed;
		}
		else
		{
			currentVelocity.X = -(GD.Randf() * 2) * CurrentSpeed;
		}
		return currentVelocity;
	}

    public override void _MouseEnter()
    {
        EntityGlobals.EntityTargetedByPlayer = this;
		GD.Print($"Entered {this}");
    }

	public override void _MouseExit()
	{
		if (EntityGlobals.EntityTargetedByPlayer == this)
		{
			EntityGlobals.EntityTargetedByPlayer = null;
			GD.Print($"Exited {this}");
		}
	}
	
	public Vector2 PhysicsProcessAggroed(double delta) 
	{
		return GlobalPosition.DirectionTo(Target.GlobalPosition) * CurrentSpeed;
	}

	public bool IsMobAggroed()
	{
		return GlobalPosition.DistanceTo(Target.GlobalPosition) < 300.0f;
	}

	public void OnBeingAttacked(float damageAmount, Entity target)
	{
		if (target != this)
		{
			return;
		}
		CurrentHealth -= damageAmount;
	}

	public void OnAttack(float damageAmount)
	{
		return;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsMobAggroed())
		{
			Velocity = PhysicsProcessAggroed(delta);
		} 
		else
		{
			Velocity = PhysicsProcessNoAggroed(delta);
		}
		MoveAndSlide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		if (CurrentHealth < 0)
		{
			FreeEntity();
		}
	}
	
	public override void UpdateAnimation(double delta)
	{
		// right now no animations
	}
	
	public override void FreeEntity()
	{
		Target.EntityAttacked -= OnBeingAttacked;
		EntityGlobals.FreeEntityTargetedByPlayer();
		GD.Print("I died bye.");
		QueueFree();
	}
}
