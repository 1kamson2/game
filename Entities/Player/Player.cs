using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Entity, IEntityCanAttack, IEntityIsAttackable
{
	[Signal] public delegate void StatsChangedEventHandler(float currentHealth, float maxHealth, float currentSpeed, float maxSpeed, float currentDamage, float maxDamage);
	public Inventory Inventory;
	public Stats Stats;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		AddToGroup("player");
		Inventory = GetNode<Inventory>("UI/HUD_Layout/Wrapper/Inventory");
		Stats = GetNode<Stats>("UI/HUD_Layout/Wrapper/Stats");
		StatsChanged += Stats.OnStatsChanged;
		Death += OnDeath;
		EmitSignal(SignalName.StatsChanged, CurrentHealth, _baseHealth, CurrentSpeed, _baseSpeed, CurrentDamage, _baseDamage);
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		switch (@event)
		{
			case var e when e.IsActionPressed("left_click"):
				OnMouseLeftClick();
				break;
			case var e when e.IsActionPressed("right_click"):
				OnMouseRightClick();
				break;
		}
	}

	public void OnBeingAttacked(float damageAmount, Entity target)
	{
		CurrentHealth -= damageAmount;
		GD.Print($"Attacked by {target}");
		EmitSignal(SignalName.StatsChanged, CurrentHealth, _baseHealth, CurrentSpeed, _baseSpeed, CurrentDamage, _baseDamage);
		if (CurrentHealth <= 0)
		{
			EmitSignal(SignalName.Death);
		}
	}

	public void OnAttack(float damageAmount, Entity target)
	{
		EmitSignal(SignalName.EntityAttacked, damageAmount, target);
	}

	public void OnMouseLeftClick()
	{
		if (EntityGlobalValues.EntityTargetedByPlayer is not null)
		{
			CurrentEntityState = EntityState.Attacking;
			OnAttack(CurrentDamage, EntityGlobalValues.EntityTargetedByPlayer);
		}
		else
		{
			CurrentEntityState = EntityState.Mining;
			Vector2 mouseGlobalPosition = GetGlobalMousePosition();
			EmitSignal(SignalName.BlockDestroyed, mouseGlobalPosition.X, mouseGlobalPosition.Y);
		}


	}

	public void OnMouseRightClick()
	{
		switch (Inventory.CurrentItem)
		{
			case UsableItem item:
				Inventory.UseCurrentItemAs<UsableItem>();
				MaxHealth += (float)GlobalManagers.Instance.GetManager<ItemManager>().ApplyModifiersToField(_baseHealth, item, "health");
				CurrentDamage += (float)GlobalManagers.Instance.GetManager<ItemManager>().ApplyModifiersToField(_baseDamage, item, "damage");
				CurrentSpeed += (float)GlobalManagers.Instance.GetManager<ItemManager>().ApplyModifiersToField(_baseSpeed, item, "speed");
				EmitSignal(SignalName.StatsChanged, CurrentHealth, MaxHealth, CurrentSpeed, CurrentSpeed, CurrentDamage, CurrentDamage);
				break;
			case Block block:
				CurrentEntityState = EntityState.Placing;
				Vector2 mouseGlobalPosition = GetGlobalMousePosition();
				EmitSignal(SignalName.BlockPlaced, mouseGlobalPosition.X, mouseGlobalPosition.Y);
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 currentVelocity = Velocity;
		if (!IsOnFloor())
		{
			currentVelocity.Y += 980 * (float)delta;
			CurrentEntityState = EntityState.FallingDown;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			currentVelocity.Y = -CurrentJumpForce;
			CurrentEntityState = EntityState.Jumping;
		}

		float direction = Input.GetAxis("go_left", "go_right");
		if (direction != 0)
		{
			currentVelocity.X = direction * CurrentSpeed;
			CurrentEntityState = EntityState.Running;
		}
		else
		{
			currentVelocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
			CurrentEntityState = EntityState.Idling;
		}
		Velocity = currentVelocity;
		MoveAndSlide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateAnimation(delta);
	}

	public override bool CheckIfAnimationLocked()
	{
		return EntityAnimation.IsPlaying() && (
			EntityAnimation.Animation == "attack" ||
			EntityAnimation.Animation == "mine_or_place"
		);
	}


	public override void UpdateAnimation(double delta)
	{
		switch (CurrentEntityState)
		{
			case EntityState.Running:
				EntityAnimation.Play("run");
				EntityAnimation.FlipH = Input.GetAxis("go_left", "go_right") < 0;
				break;
			case EntityState.Idling:
				EntityAnimation.Play("idle");
				break;
			case EntityState.Jumping:
				EntityAnimation.Play("jumping");
				break;
			case EntityState.FallingDown:
				EntityAnimation.Play("falling_down");
				break;
			case EntityState.Attacking:
				EntityAnimation.Play("attack");
				break;
			case EntityState.Mining or EntityState.Placing:
				EntityAnimation.Play("mine_or_place");
				break;
		}
	}

	public void OnDeath()
	{
		CurrentHealth = _baseHealth;
		CurrentSpeed = _baseSpeed;
		CurrentDamage = _baseDamage;
		GlobalPosition = new(0, 0);
		EmitSignal(SignalName.StatsChanged, CurrentHealth, _baseHealth, CurrentSpeed, _baseSpeed, CurrentDamage, _baseDamage);
	}

	public override void FreeEntity()
	{
		QueueFree();
	}
}
