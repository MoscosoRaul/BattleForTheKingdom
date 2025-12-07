using System;
public static class CombatService
{
    static readonly Random rng = new Random();
    public static (bool attackerWins, int atkRoll, int defRoll) Resolve(Unit attacker, Unit defender)
    {
        int ra = rng.Next(1, 7), rd = rng.Next(1, 7);
        int atk = attacker.BaseAtk + ra, def = defender.BaseDef + rd;
        return (atk > def, ra, rd);
    }
}
