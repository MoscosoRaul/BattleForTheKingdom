[System.Serializable]
public class SaveRoot
{
    public string version = "1.0.0";
    public Metadata metadata = new();
    public GameDto game = new();
}
[System.Serializable] public class Metadata { public string created_at_utc; public int seed; }
[System.Serializable]
public class GameDto
{
    public string state; public int turn; public string currentPlayer;
    public MapDto map; public PlayerDto[] players; public UnitDto[] units;
    public PosDto redCastle; public PosDto blueCastle;
}
[System.Serializable] public class MapDto { public int width; public int height; public TileDto[] tiles; }
[System.Serializable] public class TileDto { public string type; public string owner; public int x; public int y; }
[System.Serializable] public class PlayerDto { public string id; public string name; public int resources; }
[System.Serializable] public class UnitDto { public string id; public string owner; public string type; public PosDto pos; public int hp; }
[System.Serializable] public class PosDto { public int x; public int y; }
