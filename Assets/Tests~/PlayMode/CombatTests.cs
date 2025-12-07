using NUnit.Framework;

public class CombatTests
{
    [Test]
    public void Dice_Rolls_Are_Between_1_And_6()
    {
        var attacker = new Soldier(Owner.RED, new Pos(0, 0));
        var defender = new Soldier(Owner.BLUE, new Pos(0, 1));

        for (int i = 0; i < 50; i++)
        {
            var (_, atkRoll, defRoll) = CombatService.Resolve(attacker, defender);

            Assert.GreaterOrEqual(atkRoll, 1);
            Assert.LessOrEqual(atkRoll, 6);

            Assert.GreaterOrEqual(defRoll, 1);
            Assert.LessOrEqual(defRoll, 6);
        }
    }
}
