using Godot;
using System;
using System.Linq;

public partial class NewWorldSettings : Control
{
	private LineEdit _seedText;
	private PackedScene _playScene;
	private const int _seedModulo = 1 << 21;
	public override void _Ready()
	{
		_seedText = GetNode<LineEdit>("SeedSettings/Center/SeedContainer/Value");
		_playScene = GD.Load<PackedScene>("res://GameModes/Play.tscn");
		_seedText.TextSubmitted += OnTextSubmitted;
	}

	private void OnTextSubmitted(string seed)
	{
		int seedValue = seed.Select(c => (int) c).Sum() % _seedModulo;
		GD.Print($"Seed => {seedValue}");
		Node2D playTree = _playScene.Instantiate<Node2D>();
		GlobalManagers gm = playTree.GetNode("GlobalManagers") as GlobalManagers;
		gm.WorldManagerSingleton.Seed = seedValue;
		GetTree().ChangeSceneToNode(playTree);
	}

	public override void _Process(double delta)
	{
	}
}
