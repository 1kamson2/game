using Godot;

[GlobalClass] public partial class Block : Resource, IRegistrable, IInventoryContainer
{
    [Export] public string Id { get; set; }
    [Export] public int MaxStackSize { get; set; }
    [Export] public bool IsStackable { get; set; }
    [Export] public string Name { get; set; }
    [Export] public bool IsBreakable { get; set; } = true;
    public int CurrentStackSize { get; set; }
    // The boundary is inclusive
    [Export] public Vector2 NoiseBoundaries { get; set; }
    // The boundary is inclusive
    [Export] public Vector2 HeightBoundaries { get; set; }
    // Coordinates in the tileset
    [Export] public Vector2I TilesetCoordinates { get; set; }

    public Block(ref Vector2 noiseBoundaries, ref Vector2 heightBoundaries, ref Vector2I atlasCoordinates)
    {
        // TODO: Add checking of the parameters later.
        NoiseBoundaries = noiseBoundaries;
        HeightBoundaries = heightBoundaries;
        TilesetCoordinates = atlasCoordinates;
    }

    public Block() { }

    public Block WithNoiseBoundaries(float lowerNoiseBoundary, float upperNoiseBoundary)
    {
        NoiseBoundaries = new(lowerNoiseBoundary, upperNoiseBoundary);
        return this;
    }

    public Block WithHeightBoundaries(int lowerHeightBoundary, int upperHeightBoundary)
    {
        HeightBoundaries = new(lowerHeightBoundary, upperHeightBoundary);
        return this;
    }

    public Block WithAtlasCoordinates(int x, int y)
    {
        TilesetCoordinates = new(x, y);
        return this;
    }

    public bool CanSpawnAt(int y, double currentNoise)
    {
        bool yRequirement = (y >= HeightBoundaries.X &&
                             y <= HeightBoundaries.Y);
        bool noiseRequirement = (currentNoise >= NoiseBoundaries.X &&
                                 currentNoise <= NoiseBoundaries.Y);
        return yRequirement && noiseRequirement;
    }
}