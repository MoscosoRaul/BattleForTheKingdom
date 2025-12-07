public class RecruitmentService
{
    public const int MaxSoldier = 6, MaxKnight = 2, MaxArcher = 4, MaxTotal = 8;
    public bool CanRecruit(Player p, UnitType t, int cs, int ck, int ca, int total, int price)
    {
        if (total >= MaxTotal) return false;
        if (t == UnitType.Soldier && cs >= MaxSoldier) return false;
        if (t == UnitType.Knight && ck >= MaxKnight) return false;
        if (t == UnitType.Archer && ca >= MaxArcher) return false;
        return p.Resources >= price;
    }
}
