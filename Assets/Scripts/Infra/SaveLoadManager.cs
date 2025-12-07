using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SaveLoadManager
{
    public static string SavePath(string filename = "bftk_save.json")
        => Path.Combine(Application.persistentDataPath, filename);

    public static void Save(GameModel gm, int seed, string filename = "bftk_save.json")
    {
        var dto = ToDto(gm, seed);
        var json = JsonUtility.ToJson(dto, prettyPrint: true);
        File.WriteAllText(SavePath(filename), json);
        Debug.Log($"Saved: {SavePath(filename)}");
    }

    public static SaveRoot LoadRaw(string filename = "bftk_save.json")
    {
        var json = File.ReadAllText(SavePath(filename));
        return JsonUtility.FromJson<SaveRoot>(json);
    }

    static SaveRoot ToDto(GameModel gm, int seed)
    {
        var root = new SaveRoot();
        root.metadata.created_at_utc = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        root.metadata.seed = seed;
        root.game.state = gm.State.ToString();
        root.game.turn = gm.Turn;
        root.game.currentPlayer = gm.CurrentPlayer.ToString();
        root.game.redCastle = new PosDto { x = gm.RedCastle.X, y = gm.RedCastle.Y };
        root.game.blueCastle = new PosDto { x = gm.BlueCastle.X, y = gm.BlueCastle.Y };
        var m = new MapDto { width = gm.Map.Width, height = gm.Map.Height };
        var tiles = new List<TileDto>();
        for (int y = 0; y < m.height; y++) for (int x = 0; x < m.width; x++)
            {
                var t = gm.Map.Tiles[y, x];
                tiles.Add(new TileDto { x = x, y = y, type = t.Type.ToString(), owner = t.Owner.ToString() });
            }
        m.tiles = tiles.ToArray();
        root.game.map = m;
        root.game.players = new[] {
            new PlayerDto{ id="RED",  name=gm.Red.Name,  resources=gm.Red.Resources  },
            new PlayerDto{ id="BLUE", name=gm.Blue.Name, resources=gm.Blue.Resources }
        };
        var unitDtos = new List<UnitDto>(); int i = 1;
        foreach (var u in gm.Units)
        {
            unitDtos.Add(new UnitDto
            {
                id = "u" + (i++),
                owner = u.Owner.ToString(),
                type = u.GetType().Name,
                pos = new PosDto { x = u.Pos.X, y = u.Pos.Y },
                hp = 1
            });
        }
        root.game.units = unitDtos.ToArray();
        return root;
    }
}
