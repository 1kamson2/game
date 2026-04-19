using Godot;

[GlobalClass] public partial class CraftingInformation : Resource
{
    /// <summary>
    /// Id defines the item id
    /// </summary>
    [Export] public string Id;
    /// <summary>
    /// Amount needed
    /// </summary>
    [Export] public int Amount;
}