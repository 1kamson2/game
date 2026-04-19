using Godot;
using System;
using System.Collections.Generic;

public interface IRegistrable
{
	/// <summary>
	/// Id is used to identify a resource. It is recommended to assign the Id in the resource and don't assign it in the code.
	/// </summary>
	[Export] string Id { get; set; }
}

public abstract partial class ResourceManager<T> : Node2D where T : IRegistrable
{
	protected Dictionary<string, T> Registry { get; } = new();
	public abstract bool RegisterResource(T resource);
	public abstract bool UnregisterResource(string id);
	public abstract T GetResource(string id);
	public abstract U GetResourceAs<U>(string id) where U : T;
}