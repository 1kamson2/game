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
		Health.GetNode<ProgressBar>("Bar").Value = currentHealth;
		Health.GetNode<ProgressBar>("Bar").MaxValue = maxHealth;
		Health.GetNode<Label>("Bar/Text").Text = $"{currentHealth:0} / {maxHealth:0}";
		CreateTween().TweenProperty(Health.GetNode<ProgressBar>("Bar"), "value", currentHealth, 0.2f);

		Speed.GetNode<ProgressBar>("Bar").Value = currentSpeed;
		Speed.GetNode<ProgressBar>("Bar").MaxValue = maxSpeed;
		Speed.GetNode<Label>("Bar/Text").Text = $"{currentSpeed:0} / {maxSpeed:0}";
		CreateTween().TweenProperty(Speed.GetNode<ProgressBar>("Bar"), "value", currentSpeed, 0.2f);

		Damage.GetNode<ProgressBar>("Bar").Value = currentDamage;
		Damage.GetNode<ProgressBar>("Bar").MaxValue = maxDamage;
		Damage.GetNode<Label>("Bar/Text").Text = $"{currentDamage:0} / {maxDamage:0}";
		CreateTween().TweenProperty(Damage.GetNode<ProgressBar>("Bar"), "value", currentDamage, 0.2f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
