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

	public void OnStatsChanged(float currentHealth, float maxHealth, float currentSpeed, float maxSpeed, float currentDamage, float maxDamage)
	{
		Health.GetNode<Label>("Value").Text = currentHealth.ToString();
		Health.GetNode<Label>("MaxValue").Text = maxHealth.ToString();
		Speed.GetNode<Label>("Value").Text = currentSpeed.ToString();
		Speed.GetNode<Label>("MaxValue").Text = maxSpeed.ToString();
		Damage.GetNode<Label>("Value").Text = currentDamage.ToString();
		Damage.GetNode<Label>("MaxValue").Text = maxDamage.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
