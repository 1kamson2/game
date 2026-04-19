using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class BlockManager : ResourceManager<Block>
{
    private string _blockFilesDirectory = "res://Resources/Blocks/";
    /// <summary>
	/// BlockLookup translates id to Block
	/// </summary>
	protected Dictionary<string, Block> BlockLookup { get; set; } = new();
    /// <summary>
    /// BlockIdLookup translates Tile coordinates to Block
    /// </summary>
    protected Dictionary<Vector2I, string> BlockIdLookup { get; set; } = new();
    public BlockManager()
    {
        {
            string[] files = DirAccess.GetFilesAt(_blockFilesDirectory);
            foreach (string file in files)
            {
                string fullPath = $"{_blockFilesDirectory}{file}";
                GD.Print($"Loading the item file: {fullPath}");
                Block block = GD.Load<Block>(fullPath);
                GD.Print($"Trying to register: {block.Id}");
                if (RegisterResource(block))
                {
                    if (!BlockLookup.TryGetValue(block.Id, out _))
                    {
                        BlockLookup[block.Id] = block;
                    }
                    if (!BlockIdLookup.TryGetValue(block.TilesetCoordinates, out _))
                    {
                        BlockIdLookup[block.TilesetCoordinates] = block.Id;
                    }
                }
            }
        }
    }

    public override bool RegisterResource(Block resource)
    {
        if (resource is null)
        {
            GD.PrintErr("Block resource is null");
            return false;
        }

        if (string.IsNullOrEmpty(resource.Id))
        {
            GD.PrintErr($"{resource.Id} is not a valid ID for Block instance.");
            return false;
        }

        if (Registry.ContainsKey(resource.Id))
        {
            GD.PrintErr($"Resource with {resource.Id} already exists in the Registry.");
            GD.PrintErr("If you want to reassign it, then UnregisterResource the already existing resource and Register it again.");
            return false;
        }

        Registry.Add(resource.Id, resource);
        return true;
    }

    public override bool UnregisterResource(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            GD.PrintErr($"`{id}` is not a valid ID.");
        }
        return Registry.Remove(id);
    }

    public override Block GetResource(string id)
    {
        bool result = Registry.TryGetValue(id, out Block block);
        if (!result)
        {
            GD.PrintErr($"Couldn't find the block with the id: `{id}`");
            return null;
        }
        return block;
    }

    public Block GetResourceFromCoordinates(ref Vector2I coordinates)
    {
        BlockIdLookup.TryGetValue(coordinates, out string id);
        if (string.IsNullOrEmpty(id))
        {
            GD.PrintErr($"No entry for: `{coordinates}`");
            return null;
        }
        return GetResource(id);
    }

    public override U GetResourceAs<U>(string id)
    {
        Block block = GetResource(id);
        if (block is null)
        {
            GD.PrintErr($"Couldn't find the block with the id: `{id}`");
            return null;
        }
        return block as U;
    }
}
