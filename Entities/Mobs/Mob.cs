using Godot;

public interface IMobController
{
	public Player Target { get; set; }
	[Export] public float MaxDistanceToPlayer { get; set; }
	public Vector2 PhysicsProcessNoAggroed(double delta);
	public Vector2 PhysicsProcessAggroed(double delta);
	public bool IsMobAggroed();
}

// INFO: https://docs.godotengine.org/en/stable/classes/class_astargrid2d.html
/// <summary>
/// Mob is a generic Mob class. It creates a generic mob. Other Mob classes should derive from this class.
/// </summary>
[GlobalClass]
public partial class Mob : Entity, IMobController
{
	public Player Target { get; set; }
	public Vector2 WanderingCoordinates { get; set; }
	[Export] public float MaxDistanceToPlayer { get; set; }
	[Export] public float SpawnChance { get; set; }

	public override void _Ready()
	{
		base._Ready();
		MaxHealth *= GlobalWorldStateValues.MobHealthModifier;
		CurrentHealth = MaxHealth;
		MaxSpeed *= GlobalWorldStateValues.MobSpeedModifier;
		CurrentSpeed = MaxSpeed;
		MaxDamage *= GlobalWorldStateValues.MobDamageModifier;
		CurrentDamage = MaxDamage;
		Target = GetTree().GetFirstNodeInGroup("player") as Player;
		Target.EntityAttacked += OnBeingAttacked;
		EntityAttacked += Target.OnBeingAttacked;
		Death += FreeEntity;
		WanderingCoordinates = new(-50, 50);
		EntityGlobalValues.CurrentMobCount++;
	}

	public virtual Vector2 PhysicsProcessNoAggroed(double delta)
	{
		Vector2 currentVelocity = Velocity;
		currentVelocity.Y = !IsOnFloor() ? currentVelocity.Y + 980 * (float)delta : currentVelocity.Y;
		if (IsOnFloor())
		{
			// Wandering Logic
			Vector2 direction = GlobalPosition.DirectionTo(WanderingCoordinates).Sign();
			currentVelocity.X = direction.X * CurrentSpeed;
			currentVelocity.Y = GD.Randf() < 0.05f || direction.Y < 0 ? -CurrentJumpForce : currentVelocity.Y;
		}
		return currentVelocity;
	}

	public override void _MouseEnter()
	{
		EntityGlobalValues.EntityTargetedByPlayer = this;
		GD.Print($"Entered {this}");
	}

	public override void _MouseExit()
	{
		if (EntityGlobalValues.EntityTargetedByPlayer == this)
		{
			EntityGlobalValues.EntityTargetedByPlayer = null;
			GD.Print($"Exited {this}");
		}
	}

	public virtual Vector2 PhysicsProcessAggroed(double delta)
	{

		Vector2 currentVelocity = Velocity;
		currentVelocity.Y = !IsOnFloor() ? currentVelocity.Y + 980 * (float)delta : currentVelocity.Y;
		if (IsOnFloor())
		{
			// Wandering Logic
			Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition).Sign();
			currentVelocity.X = direction.X * CurrentSpeed;
			currentVelocity.Y = GD.Randf() < 0.05f || direction.Y < 0 ? -CurrentJumpForce : currentVelocity.Y;
		}
		return currentVelocity;
	}

	public virtual bool IsMobAggroed()
	{
		return GlobalPosition.DistanceTo(Target.GlobalPosition) < MaxDistanceToPlayer;
	}

	public virtual bool AchievedWanderingCoordinates()
	{
		return GlobalPosition.DistanceTo(WanderingCoordinates) < 100f;
	}

	public virtual Vector2 NewWanderingCoordinates()
	{
		return WanderingCoordinates + new Vector2(GD.RandRange(-50, 50), WanderingCoordinates.Y);
	}

	public virtual void OnBeingAttacked(float damageAmount, Entity target)
	{
		if (target != this)
		{
			return;
		}
		CurrentHealth -= damageAmount;
		CurrentEntityState = EntityState.BeingHit;
		EntityAnimation.Modulate = HitModulate;
		Tween tween = CreateTween();
		tween.TweenProperty(EntityAnimation, "modulate", Colors.White, 0.1f);
		if (CurrentHealth <= 0)
		{
			EmitSignal(SignalName.Death);
		}
	}

	public virtual void OnAttack()
	{
		if (Target.GlobalPosition.DistanceTo(this.GlobalPosition) < 50)
		{
			CurrentEntityState = EntityState.Attacking;
			EmitSignal(SignalName.EntityAttacked, CurrentDamage, Target);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		CurrentEntityState = EntityState.Running;
		if (IsMobAggroed())
		{
			Velocity = PhysicsProcessAggroed(delta);
		}
		else
		{
			WanderingCoordinates = AchievedWanderingCoordinates() ? NewWanderingCoordinates() : WanderingCoordinates;
			Velocity = PhysicsProcessNoAggroed(delta);
		}
		MoveAndSlide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateAnimation(delta);
	}

	public override void UpdateAnimation(double delta)
	{
		switch (CurrentEntityState)
		{
			case EntityState.Running:
				EntityAnimation.Play("run");
				break;
			case EntityState.Idling:
				EntityAnimation.Play("idle");
				break;
			case EntityState.Attacking:
				EntityAnimation.Play("attack");
				break;
		}
	}


	public override bool CheckIfAnimationLocked()
	{
		return (EntityAnimation.IsPlaying() && (
		EntityAnimation.Animation == "attack"));
	}

	public void ShouldDespawn()
	{
		if (Target.GlobalPosition.DistanceTo(GlobalPosition) > 250)
		{
			FreeEntity();
		}
	}

	public override void FreeEntity()
	{
		Target.EntityAttacked -= OnBeingAttacked;
		Death -= FreeEntity;
		GlobalManagers.Instance.GetManager<WorldManager>().MaxMobCountAchieved -= ShouldDespawn;
		EntityGlobalValues.FreeEntityTargetedByPlayer();
		EntityGlobalValues.CurrentMobCount--;
		RemoveFromGroup("mobs");
		QueueFree();
	}
}
