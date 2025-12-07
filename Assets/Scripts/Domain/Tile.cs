public class Tile
{
    public TileType Type;
    public Owner Owner;
    public Unit Occupant; // null, ha üres
    public bool IsCapturable => Type == TileType.MINE || Type == TileType.TEMPLE;
    public bool IsEmpty => Occupant == null;
}
