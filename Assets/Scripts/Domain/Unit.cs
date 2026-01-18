/// <summary>
/// Base unit (stats are fixed by type).
/// "HasActedThisTurn" enforces: each unit may act (move OR attack) at most once per turn.
/// </summary>
public abstract class Unit
{
    public Owner Owner { get; }
    public Pos Pos { get; private set; }

    public int BaseAtk { get; }
    public int BaseDef { get; }
    public int Move { get; }
    public int Range { get; }
    public int Price { get; }

    public bool HasActedThisTurn { get; private set; }

    protected Unit(Owner owner, Pos pos, int atk, int def, int move, int range, int price)
    {
        Owner = owner;
        Pos = pos;
        BaseAtk = atk;
        BaseDef = def;
        Move = move;
        Range = range;
        Price = price;
    }

    public void SetPos(Pos p) => Pos = p;

    public void MarkActed() => HasActedThisTurn = true;
    public void ResetActed() => HasActedThisTurn = false;
}

public class Soldier : Unit
{
    public Soldier(Owner o, Pos p) : base(o, p, atk: 9, def: 9, move: 1, range: 1, price: 20) { }
}

public class Knight : Unit
{
    public Knight(Owner o, Pos p) : base(o, p, atk: 12, def: 12, move: 2, range: 2, price: 55) { }
}

public class Archer : Unit
{
    // Raul request: archer shoots from 2 tiles away.
    public Archer(Owner o, Pos p) : base(o, p, atk: 10, def: 7, move: 1, range: 2, price: 30) { }
}
