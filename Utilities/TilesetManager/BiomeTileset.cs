using Godot;

[GlobalClass] public partial class BiomeTileset : TileSet, IRegistrable
{
	[Export] public string Id { get; set; }
}