using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class End : Control
{
	public override void _Ready()
	{
		
	}

	public override async void _Process(double delta)
	{
		await Task.Delay(5000);
		GetTree().ChangeSceneToFile("res://GameModes/MainMenu.tscn");

	}
}
