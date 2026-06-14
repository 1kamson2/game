using Godot;
using System;

public partial class MainMenu : Control
{
	private Button _start, _exit;

	public override void _Ready()
	{
		_start = GetNode<Button>("MainMenu/Panes/MenuOptions/Start");
		_start.Pressed += OnStartButtonPressed;
		_exit = GetNode<Button>("MainMenu/Panes/MenuOptions/Exit");
		_exit.Pressed += OnExitButtonPressed;

	}

	private void OnStartButtonPressed()
	{
		GD.Print("Pressed Start");
		GetTree().ChangeSceneToFile("res://GameModes/NewWorldSettings.tscn");
	}

	private void OnExitButtonPressed()
	{
		_start.Pressed -= OnStartButtonPressed;
		_exit.Pressed -= OnExitButtonPressed;
		GetTree().Quit();
	}

	public override void _Process(double delta)
	{
	}
}
