using NUnit.Framework;

public class MapGeneratorTests
{
    [Test]
    public void Generates_7x7_WithCornerCastles()
    {
        var gm = new GameModel();
        gm.StartNew(7, 7, 123);

        Assert.AreEqual(7, gm.Map.Width);
        Assert.AreEqual(7, gm.Map.Height);
        Assert.AreEqual(TileType.CASTLE, gm.Map.At(gm.RedCastle).Type);
        Assert.AreEqual(TileType.CASTLE, gm.Map.At(gm.BlueCastle).Type);
    }
}
