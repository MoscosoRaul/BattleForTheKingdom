public class Map
{
    public readonly int Width;
    public readonly int Height;
    public readonly Tile[,] Tiles;
    public Map(int w, int h)
    {
        Width = w; Height = h;
        Tiles = new Tile[h, w];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                Tiles[y, x] = new Tile { Type = TileType.PLAINS, Owner = Owner.NONE };
    }
    public Tile At(Pos p) => Tiles[p.Y, p.X];
    public bool InBounds(Pos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
}
