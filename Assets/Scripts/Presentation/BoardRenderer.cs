using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform boardParent;
    [SerializeField] private GameObject tilePrefab;

    [Header("Generation")]
    [SerializeField] private bool randomSeedOnPlay = false;
    [SerializeField] private int seed = 123456;

    [Header("Percentages (sum <= 1.0, castles are fixed 2 tiles)")]
    [Range(0f, 1f)][SerializeField] private float plainsP = 0.40f;
    [Range(0f, 1f)][SerializeField] private float forestP = 0.25f;
    [Range(0f, 1f)][SerializeField] private float mineP = 0.20f;
    [Range(0f, 1f)][SerializeField] private float templeP = 0.10f;

    [Header("Temple rules")]
    [SerializeField] private bool fixedTemples = true;
    [SerializeField] private Vector2Int fixedTempleRed = new Vector2Int(1, 5);
    [SerializeField] private Vector2Int fixedTempleBlue = new Vector2Int(5, 1);

    private TileView[,] tileViews;

    public void GenerateAndRender(GameController controller, GameModel model, ref int seedRef)
    {
        if (boardParent == null || tilePrefab == null)
        {
            Debug.LogError("BoardRenderer: boardParent vagy tilePrefab nincs beállítva!");
            return;
        }
        if (controller == null || model == null || model.Map == null)
        {
            Debug.LogError("BoardRenderer: controller/model/map hiányzik!");
            return;
        }

        if (randomSeedOnPlay)
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        seedRef = seed;

        int width = model.Map.Width;
        int height = model.Map.Height;

        BuildTileViews(width, height, controller);

        // terrain generation into MODEL
        GenerateFairDiagonal(model.Map, seed, model.RedCastle, model.BlueCastle);

        // make sure non-castle owners are NONE initially
        for (int y = 0; y < model.Map.Height; y++)
            for (int x = 0; x < model.Map.Width; x++)
            {
                var p = new Pos(x, y);
                if (p == model.RedCastle || p == model.BlueCastle) continue;
                model.Map.At(p).Owner = Owner.NONE;
            }

        SyncFromModel(model, null, null, null);
    }

    public void SyncFromModel(GameModel model, Pos? selected, HashSet<Pos> moveTargets, HashSet<Pos> attackTargets)
    {
        if (model == null || model.Map == null || tileViews == null) return;

        for (int y = 0; y < model.Map.Height; y++)
        {
            for (int x = 0; x < model.Map.Width; x++)
            {
                var tv = tileViews[x, y];
                if (tv == null) continue;

                var p = new Pos(x, y);
                var tile = model.Map.At(p);

                tv.SetBaseColor(GetBaseColor(tile.Type, tile.Owner));

                // unit icon
                var occ = tile.Occupant;
                if (occ == null) tv.SetUnit(null, false, null);
                else tv.SetUnit(occ.Owner, occ.HasActedThisTurn, ShortType(occ));

                // mindig: tile alapszín + foglalás jel
                tv.SetBaseColor(GetTileColor(tile.Type, tile.Owner));
                tv.SetTileOwnerMark(tile.Type, tile.Owner);

                // highlights: Selected > Attack > Move
                if (selected != null && p == selected.Value)
                    tv.SetHighlight(TileView.Highlight.Selected);
                else if (attackTargets != null && attackTargets.Contains(p))
                    tv.SetHighlight(TileView.Highlight.AttackTarget);
                else if (moveTargets != null && moveTargets.Contains(p))
                    tv.SetHighlight(TileView.Highlight.MoveTarget);
                else
                    tv.SetHighlight(TileView.Highlight.None);
            }
        }
    }

    private void BuildTileViews(int w, int h, GameController controller)
    {
        for (int i = boardParent.childCount - 1; i >= 0; i--)
            Destroy(boardParent.GetChild(i).gameObject);

        tileViews = new TileView[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var go = Instantiate(tilePrefab, boardParent);
                var tv = go.GetComponent<TileView>();
                if (tv == null)
                {
                    Debug.LogError("Tile prefab-ról hiányzik a TileView!");
                    continue;
                }

                tv.Init(x, y, controller);
                tileViews[x, y] = tv;
            }
        }
    }

    // ---------- terrain generation (fair diagonal) ----------

    private void GenerateFairDiagonal(Map map, int seedValue, Pos redCastle, Pos blueCastle)
    {
        var rng = new System.Random(seedValue);

        // reset all to PLAINS first
        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
                map.At(new Pos(x, y)).Type = TileType.PLAINS;

        // castles fixed
        map.At(redCastle).Type = TileType.CASTLE;
        map.At(blueCastle).Type = TileType.CASTLE;

        var redHalf = new List<Vector2Int>();
        var blueHalf = new List<Vector2Int>();
        var diag = new List<Vector2Int>();

        int diagSum = map.Width - 1;

        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
            {
                var p = new Pos(x, y);
                if (p == redCastle || p == blueCastle) continue;

                int s = x + y;
                if (s < diagSum) redHalf.Add(new Vector2Int(x, y));
                else if (s > diagSum) blueHalf.Add(new Vector2Int(x, y));
                else diag.Add(new Vector2Int(x, y));
            }

        Shuffle(redHalf, rng);
        Shuffle(blueHalf, rng);
        Shuffle(diag, rng);

        int total = map.Width * map.Height;
        int nonCastle = total - 2;

        int forestCount = Mathf.RoundToInt(nonCastle * forestP);
        int mineCount = Mathf.RoundToInt(nonCastle * mineP);
        int templeCount = Mathf.RoundToInt(nonCastle * templeP);

        // fixed temples if possible
        if (fixedTemples && templeCount >= 2)
        {
            if (IsPlaceable(map, fixedTempleRed, redCastle, blueCastle))
            {
                map.At(new Pos(fixedTempleRed.x, fixedTempleRed.y)).Type = TileType.TEMPLE;
                RemoveIfPresent(redHalf, fixedTempleRed);
                RemoveIfPresent(blueHalf, fixedTempleRed);
                RemoveIfPresent(diag, fixedTempleRed);
                templeCount--;
            }

            if (IsPlaceable(map, fixedTempleBlue, redCastle, blueCastle))
            {
                map.At(new Pos(fixedTempleBlue.x, fixedTempleBlue.y)).Type = TileType.TEMPLE;
                RemoveIfPresent(redHalf, fixedTempleBlue);
                RemoveIfPresent(blueHalf, fixedTempleBlue);
                RemoveIfPresent(diag, fixedTempleBlue);
                templeCount--;
            }
        }

        PlaceFair(map, redHalf, blueHalf, diag, TileType.FOREST, forestCount);
        PlaceFair(map, redHalf, blueHalf, diag, TileType.MINE, mineCount);
        PlaceFair(map, redHalf, blueHalf, diag, TileType.TEMPLE, templeCount);
        // PLAINS default
    }

    private void PlaceFair(Map map, List<Vector2Int> redHalf, List<Vector2Int> blueHalf, List<Vector2Int> diag, TileType type, int count)
    {
        if (count <= 0) return;

        int half = count / 2;
        int rest = count % 2;

        PlaceFromList(map, redHalf, type, half);
        PlaceFromList(map, blueHalf, type, half);

        if (rest > 0)
        {
            if (!PlaceFromList(map, diag, type, 1))
            {
                if (!PlaceFromList(map, redHalf, type, 1))
                    PlaceFromList(map, blueHalf, type, 1);
            }
        }
    }

    private bool PlaceFromList(Map map, List<Vector2Int> cells, TileType type, int amount)
    {
        if (amount <= 0) return true;

        int placed = 0;
        while (placed < amount && cells.Count > 0)
        {
            var c = cells[0];
            cells.RemoveAt(0);

            var t = map.At(new Pos(c.x, c.y));
            if (t.Type == TileType.PLAINS)
            {
                t.Type = type;
                placed++;
            }
        }

        return placed == amount;
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private bool IsPlaceable(Map map, Vector2Int v, Pos redCastle, Pos blueCastle)
    {
        var p = new Pos(v.x, v.y);
        if (!map.InBounds(p)) return false;
        if (p == redCastle || p == blueCastle) return false;
        return true;
    }

    private void RemoveIfPresent(List<Vector2Int> list, Vector2Int v)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i] == v) list.RemoveAt(i);
    }

    private Color GetBaseColor(TileType type, Owner owner)
    {
        if (type == TileType.CASTLE)
        {
            return (owner == Owner.RED)
                ? new Color(0.6f, 0.1f, 0.1f)
                : new Color(0.1f, 0.1f, 0.6f);
        }

        return type switch
        {
            TileType.PLAINS => new Color(0.7f, 0.7f, 0.7f),
            TileType.FOREST => new Color(0.15f, 0.55f, 0.2f),
            TileType.MINE => new Color(0.25f, 0.25f, 0.25f),
            TileType.TEMPLE => new Color(0.65f, 0.55f, 0.9f),
            _ => new Color(0.7f, 0.7f, 0.7f),
        };
    }
    private string ShortType(Unit u)
    {
        // az osztálynévbõl dolgozunk: Soldier/Knight/Archer
        string n = u.GetType().Name;
        return n switch
        {
            "Soldier" => "S",
            "Archer" => "A",
            "Knight" => "K",
            _ => "?"
        };
    }
    private Color GetTileColor(TileType type, Owner owner)
    {
        // --- FIX PALETTE (as requested) ---
        Color plain = new Color(0.70f, 0.90f, 0.60f); // világoszöld
        Color forest = new Color(0.12f, 0.38f, 0.18f); // sötétzöld

        Color mineNeutral = new Color(0.80f, 0.80f, 0.80f); // világos szürke
        Color mineCaptured = new Color(0.28f, 0.28f, 0.28f); // sötétszürke

        Color templeNeutral = new Color(0.95f, 0.95f, 0.95f); // fehér (kicsit tört)
        Color templeRed = new Color(1.00f, 0.65f, 0.80f); // rózsaszín
        Color templeBlue = new Color(0.65f, 0.85f, 1.00f); // világoskék

        // Castle: maradjon a régi erõs csapatszín
        if (type == TileType.CASTLE)
        {
            if (owner == Owner.RED) return new Color(0.95f, 0.25f, 0.25f);
            if (owner == Owner.BLUE) return new Color(0.25f, 0.60f, 0.98f);
            return new Color(0.55f, 0.55f, 0.55f); // fallback
        }

        // Temple: fehér -> elfoglalva csapatszín szerint
        if (type == TileType.TEMPLE)
        {
            if (owner == Owner.RED) return templeRed;
            if (owner == Owner.BLUE) return templeBlue;
            return templeNeutral;
        }

        // Mine: világos szürke -> elfoglalva sötétszürke (owner mindegy)
        if (type == TileType.MINE)
        {
            return (owner == Owner.NONE) ? mineNeutral : mineCaptured;
        }

        // Forest / Plains
        if (type == TileType.FOREST) return forest;
        if (type == TileType.PLAINS) return plain;

        // Default
        return plain;
    }

}
