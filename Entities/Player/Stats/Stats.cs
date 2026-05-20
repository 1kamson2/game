using Godot;
using System;

public partial class Stats : PanelContainer
{
	HBoxContainer Health { get; set; }
	HBoxContainer Speed { get; set; }
	HBoxContainer Damage { get; set; }

	public override void _Ready()
	{
		Health = GetNode<HBoxContainer>("MC/Container/Health");
		Speed = GetNode<HBoxContainer>("MC/Container/Speed");
		Damage = GetNode<HBoxContainer>("MC/Container/Damage");
	}

	public void UpdateValues(float health=float.MinValue, float speed=float.MinValue, float damage=float.MinValue)
	{
		if (health != float.MinValue)
		{
			Health.GetNode<Label>("Value").Text = health.ToString();
		}
		if (speed != float.MinValue)
		{
			Speed.GetNode<Label>("Value").Text = speed.ToString();
		}
		if (damage != float.MinValue)
		{
			Damage.GetNode<Label>("Value").Text = damage.ToString();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
