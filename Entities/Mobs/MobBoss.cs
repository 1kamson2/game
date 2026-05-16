using Godot;
using System;
using System.ComponentModel;


// TODO: Look into: https://docs.godotengine.org/en/stable/classes/class_astargrid2d.html
[GlobalClass]
public partial class MobBoss : Mob
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void FreeEntity()
	{
		Target.EntityAttacked -= OnBeingAttacked;
		EntityGlobalValues.FreeEntityTargetedByPlayer();
		QueueFree();
	}
}
