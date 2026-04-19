using Godot;
using System;
using System.Collections.Generic;

public partial class TilesetManager : ResourceManager<BiomeTileset>
{
	/// <summary>
	/// Returns the Tileset Id based on the Biome Id. Example: BiomeTilesetLookup["biome_plains"] = "biome_plains_tileset"
	/// </summary>
	public Dictionary<string, string> BiomeTilesetLookup { get; set; } = new();
	public TilesetManager()
	{
		// Load plains biome tileset
		{
			BiomeTileset plainsBiomeTileset = GD.Load<TileSet>("res://Resources/biome_plains_tileset.tres") as BiomeTileset;
			if (!RegisterResource(plainsBiomeTileset))
			{
				GD.PrintErr("Couldn't load the plains biome");
			}
			BiomeTilesetLookup["biome_plains"] = "biome_plains_tileset";
		}
	}
	public override bool RegisterResource(BiomeTileset resource)
	{
		if (resource is null)
		{
			GD.PrintErr("BiomeTileset resource is null");
			return false;
		}

		if (string.IsNullOrEmpty(resource.Id))
		{
			GD.PrintErr($"{resource.Id} is not a valid ID for BiomeTileset instance.");
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

	public override BiomeTileset GetResource(string id)
	{
		bool result = Registry.TryGetValue(id, out BiomeTileset tileset);
		if (!result)
		{
			GD.PrintErr($"Couldn't find the tileset with the id: `{id}`");
			return null;
		}
		return tileset;
	}

	public override U GetResourceAs<U>(string id)
	{
		TileSet tileset = GetResource(id);
		if (tileset is null)
		{
			GD.PrintErr($"Couldn't find the tileset with the id: `{id}`");
			return null;
		}
		return tileset as U;
	}

	public Texture2D GetTileTexture(string biomeId, int sourceId, Vector2I atlasCoords)
	{
		if (!BiomeTilesetLookup.TryGetValue(biomeId, out string tilesetId))
		{
			GD.PrintErr($"No tilset registered for biome {biomeId}");
			return null;
		}

		if (!Registry.TryGetValue(tilesetId, out BiomeTileset tileset))
		{
			GD.PrintErr($"Tileset with id `{biomeId}` doesn't exist");
			return null;
		}

		TileSetAtlasSource tileSource = tileset.GetSource(sourceId) as TileSetAtlasSource;
		Rect2I tileRegion = tileSource.GetTileTextureRegion(atlasCoords);
		Image image = tileSource.Texture.GetImage();
		Image tileImage = image.GetRegion(tileRegion);
		return ImageTexture.CreateFromImage(tileImage);
	}
}
