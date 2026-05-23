using Godot;
using System;


public static class ContainerGlobalValues
{
	public static Pot LastContainerInteraction { get; set; } = null;
}

public partial class Pot : Area2D
{
	[Signal] public delegate void ContainerLootedEventHandler(string loot_id, int amount);
	[Export] public string StoredItem { get; set; }
	[Export] public int Amount { get; set; }
	protected AnimatedSprite2D PotAnimation { get; set; }

    public override void _Ready()
    {
        base._Ready();
		PotAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

	public void Loot()
	{
		PotAnimation.Play("loot");
		EmitSignal(SignalName.ContainerLooted, StoredItem, Amount);
	}

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			ContainerGlobalValues.LastContainerInteraction = this;
			Loot();
		}
	}
}
