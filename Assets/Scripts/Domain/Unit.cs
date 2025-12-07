public abstract class Unit
{
    public Owner Owner { get; }
    public Pos Pos { get; private set; }
    public int BaseAtk { get; }
    public int BaseDef { get; }
    public int Move { get; }
    public int Range { get; }
    public int Price { get; }
    protected Unit(Owner owner, Pos pos, int atk, int def, int move, int range, int price)
    {
        Owner = owner; Pos = pos; BaseAtk = atk; BaseDef = def; Move = move; Range = range; Price = price;
    }
    public void SetPos(Pos p) => Pos = p;
}
public class Soldier : Unit { public Soldier(Owner o, Pos p) : base(o, p, 9, 9, 1, 1, 20) { } }
public class Knight : Unit { public Knight(Owner o, Pos p) : base(o, p, 12, 12, 2, 1, 55) { } }
public class Archer : Unit { public Archer(Owner o, Pos p) : base(o, p, 10, 7, 1, 1, 30) { } } // Range=1
