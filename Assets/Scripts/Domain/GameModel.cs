using System.Collections.Generic;
public class GameModel
{
    public GameState State { get; private set; } = GameState.MainMenu;
    public Map Map { get; private set; }
    public Player Red { get; private set; }
    public Player Blue { get; private set; }
    public Owner CurrentPlayer { get; private set; } = Owner.RED;
    public int Turn { get; private set; } = 1;
    public Pos RedCastle { get; private set; }
    public Pos BlueCastle { get; private set; }
    public readonly List<Unit> Units = new();
    public void StartNew(int w = 7, int h = 7, int seed = 123456)
    {
        Map = new Map(w, h);
        Red = new Player(Owner.RED, "Piros Birodalom", 0);
        Blue = new Player(Owner.BLUE, "Kék Királyság", 0);
        RedCastle = new Pos(0, h - 1);
        BlueCastle = new Pos(w - 1, 0);
        Map.At(RedCastle).Type = TileType.CASTLE; Map.At(RedCastle).Owner = Owner.RED;
        Map.At(BlueCastle).Type = TileType.CASTLE; Map.At(BlueCastle).Owner = Owner.BLUE;
        State = GameState.Running;
    }
    public void EndTurn()
    {
        if (CheckVictory()) { State = GameState.GameOver; return; }
        CurrentPlayer = (CurrentPlayer == Owner.RED) ? Owner.BLUE : Owner.RED;
        Turn++;
    }
    bool CheckVictory()
    {
        var redCastleOwner = Map.At(RedCastle).Owner;
        var blueCastleOwner = Map.At(BlueCastle).Owner;
        return redCastleOwner == Owner.BLUE || blueCastleOwner == Owner.RED;
    }
}
