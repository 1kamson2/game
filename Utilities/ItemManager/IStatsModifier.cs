using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;


/// <summary>
/// IStatsModifier is an interface that allows to change entity stats.
/// </summary>
public interface IStatsModifier
{
    [Export] public Stat[] Stats { get; set; }
}