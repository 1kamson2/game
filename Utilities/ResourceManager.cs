using Godot;
using System;

public static class ResourceManager
{
	public static Texture2D TileToTexture(ref TileSet tileSet, Vector2I atlasCoordinates, int sourceId)
	{
		if (tileSet is null)
		{
			return null;
		}

		TileSetAtlasSource source = tileSet.GetSource(sourceId) as TileSetAtlasSource;
		AtlasTexture atlasTexture = new AtlasTexture();
		atlasTexture.Atlas = source.Texture;
		atlasTexture.Region = source.GetTileTextureRegion(atlasCoordinates);
		return atlasTexture;
	}
}
