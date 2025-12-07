using NUnit.Framework;
using System.IO;

public class SaveLoadTests
{
    [Test]
    public void Can_Save_And_Create_File()
    {
        var gm = new GameModel();
        gm.StartNew(7, 7, 123);

        // mentés
        SaveLoadManager.Save(gm, 123, "test_save.json");

        var path = SaveLoadManager.SavePath("test_save.json");
        Assert.IsTrue(File.Exists(path), "Save file was not created.");

        // nyers betöltés
        var root = SaveLoadManager.LoadRaw("test_save.json");
        Assert.AreEqual("Running", root.game.state);
        Assert.AreEqual(7, root.game.map.width);
    }
}
