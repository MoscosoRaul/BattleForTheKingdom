using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveLoadManager
{
    public const string DefaultFileName = "bftk_save.json";

    public static void Save(GameModel model, int seed, string fileName = DefaultFileName)
    {
        if (model == null || model.Map == null)
        {
            Debug.LogError("SaveLoadManager.Save: nincs model vagy map.");
            return;
        }

        var root = ToDto(model, seed);
        string json = JsonUtility.ToJson(root, true);

        string path = GetPath(fileName);
        File.WriteAllText(path, json);

        Debug.Log($"[SAVE] Mentve ide: {path}");
    }

    public static SaveRoot Load(string fileName = DefaultFileName)
    {
        string path = GetPath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[LOAD] Nem találom a mentést: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        var root = JsonUtility.FromJson<SaveRoot>(json);

        if (root == null || root.game == null)
        {
            Debug.LogError("[LOAD] Hibás mentésfájl (root/game null).");
            return null;
        }

        Debug.Log($"[LOAD] Betöltve innen: {path}");
        return root;
    }

    private static string GetPath(string fileName)
        => Path.Combine(Application.persistentDataPath, fileName);

    private static SaveRoot ToDto(GameModel model, int seed)
    {
        var map = model.Map;

        var mapDto = new MapDto
        {
            width = map.Width,
            height = map.Height,
            tiles = map.AllTiles()
                      .Select(t => new TileDto
                      {
                          x = t.x,
                          y = t.y,
                          type = t.tile.Type.ToString(),
                          owner = t.tile.Owner.ToString()
                      })
                      .ToArray()
        };

        var players = new[]
        {
            new PlayerDto
            {
                id = "RED",
                name = model.Red?.Name ?? "Piros Birodalom",
                resources = model.Red?.Resources ?? 0,
                limits = new LimitsDto()
            },
            new PlayerDto
            {
                id = "BLUE",
                name = model.Blue?.Name ?? "Kék Királyság",
                resources = model.Blue?.Resources ?? 0,
                limits = new LimitsDto()
            }
        };

        var units = model.Units.Select((u, i) => new UnitDto
        {
            id = $"u{i + 1}",
            owner = u.Owner.ToString(),
            type = u.GetType().Name, // Soldier/Knight/Archer
            pos = new PosDto { x = u.Pos.X, y = u.Pos.Y },
            hp = 1
        }).ToArray();

        var root = new SaveRoot
        {
            version = "1.0.0",
            metadata = new MetadataDto
            {
                created_at_utc = DateTime.UtcNow.ToString("o"),
                seed = seed
            },
            game = new GameDto
            {
                state = (model.State == GameState.GameOver) ? "GameOver" : "Running",
                turn = model.Turn,
                currentPlayer = model.CurrentPlayer.ToString(),
                map = mapDto,
                players = players,
                units = units,
                castles = new CastlesDto
                {
                    RED = new PosDto { x = model.RedCastle.X, y = model.RedCastle.Y },
                    BLUE = new PosDto { x = model.BlueCastle.X, y = model.BlueCastle.Y }
                }
            },
            checksum_sha1 = "" // most üresen hagyjuk
        };

        return root;
    }
}

/// <summary>
/// Kis helper, hogy könnyen végig tudjunk menni a map tile-okon.
/// </summary>
public static class MapExtensions
{
    public static System.Collections.Generic.IEnumerable<(int x, int y, Tile tile)> AllTiles(this Map map)
    {
        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
                yield return (x, y, map.Tiles[y, x]);
    }
}
