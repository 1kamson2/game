using Godot;

[GlobalClass]
public partial class Stat : Resource, IRegistrable
{
    [Export] public string Id { get; set; }
    [Export] public double Value { get; set; }
    [Export] public StatModifierType ModifierType { get; set; }
}