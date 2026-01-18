using System.Collections.Generic;

public class GameModel
{
    public GameState State { get; private set; } = GameState.MainMenu;

    public Map Map { get; private set; }
    public Player Red { get; private set; }
    public Player Blue { get; private set; }

    public Owner CurrentPlayer { get; private set; } = Owner.RED;
    public int Turn { get; private set; } = 1;

    public Pos RedCastle { get; private set; }
    public Pos BlueCastle { get; private set; }

    public Owner Winner { get; private set; } = Owner.NONE;

    public readonly List<Unit> Units = new();

    public void StartNew(int w = 7, int h = 7, int seed = 123456)
    {
        Units.Clear();

        Map = new Map(w, h);
        Red = new Player(Owner.RED, "Red Empire", 0);
        Blue = new Player(Owner.BLUE, "Blue Kingdom", 0);

        CurrentPlayer = Owner.RED;
        Turn = 1;
        Winner = Owner.NONE;
        State = GameState.Running;

        // Castles (fixed)
        RedCastle = new Pos(0, h - 1);
        BlueCastle = new Pos(w - 1, 0);

        var rt = Map.At(RedCastle);
        rt.Type = TileType.CASTLE;
        rt.Owner = Owner.RED;

        var bt = Map.At(BlueCastle);
        bt.Type = TileType.CASTLE;
        bt.Owner = Owner.BLUE;

        // Starting units (2-2), but do NOT block both adjacent castle tiles
        TryPlaceUnit(new Soldier(Owner.RED, new Pos(0, h - 2)));   // adjacent
        TryPlaceUnit(new Soldier(Owner.RED, new Pos(1, h - 2)));   // not adjacent to castle

        TryPlaceUnit(new Soldier(Owner.BLUE, new Pos(w - 1, 1)));  // adjacent
        TryPlaceUnit(new Soldier(Owner.BLUE, new Pos(w - 2, 1)));  // not adjacent to castle

        // give starting income to first player
        EconomyService.GiveTurnIncome(this);
    }

    public Unit GetUnitAt(Pos p)
    {
        if (Map == null || !Map.InBounds(p)) return null;
        return Map.At(p).Occupant;
    }

    public IEnumerable<Pos> GetReachableMoves(Unit u)
    {
        var result = new HashSet<Pos>();
        if (u == null || Map == null) return result;

        var start = u.Pos;
        bool isKnight = (u is Knight);

        var q = new Queue<(Pos p, int d)>();
        var visited = new HashSet<Pos>();

        q.Enqueue((start, 0));
        visited.Add(start);

        while (q.Count > 0)
        {
            var (p, d) = q.Dequeue();
            if (d == u.Move) continue;

            // KNIGHT forest rule:
            // If the knight already entered a forest tile during this move (p != start and p is forest),
            // it must stop there: no further expansion from that tile this turn.
            if (isKnight && p != start && Map.At(p).Type == TileType.FOREST)
                continue;

            foreach (var n in Neighbors4(p))
            {
                if (!Map.InBounds(n)) continue;
                if (visited.Contains(n)) continue;

                var tile = Map.At(n);

                // cannot step through/onto units
                if (tile.Occupant != null)
                {
                    visited.Add(n);
                    continue;
                }

                visited.Add(n);

                // can end on empty tile
                result.Add(n);

                // enqueue further exploration (BFS)
                q.Enqueue((n, d + 1));
            }
        }

        return result;
    }

    public IEnumerable<Pos> GetAttackTargets(Unit u)
    {
        var result = new HashSet<Pos>();
        if (u == null || Map == null) return result;

        int r = u.Range;
        var from = u.Pos;

        bool isArcher = (u is Archer);

        // Manhattan range
        for (int dy = -r; dy <= r; dy++)
        {
            int rem = r - Abs(dy);
            for (int dx = -rem; dx <= rem; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                var p = new Pos(from.X + dx, from.Y + dy);
                if (!Map.InBounds(p)) continue;

                var tile = Map.At(p);
                var target = tile.Occupant;

                if (target == null || target.Owner == u.Owner) continue;

                int dist = Abs(from.X - p.X) + Abs(from.Y - p.Y);

                // FOREST rule: if target stands in forest, archer can only attack from adjacent (dist == 1)
                if (isArcher && tile.Type == TileType.FOREST && dist > 1)
                    continue;

                result.Add(p);
            }
        }

        return result;
    }

    public bool TryMove(Pos from, Pos to, out string error)
    {
        error = null;

        if (State == GameState.GameOver) { error = "Game is over."; return false; }
        if (!Map.InBounds(from) || !Map.InBounds(to)) { error = "Out of bounds."; return false; }

        var unit = GetUnitAt(from);
        if (unit == null) { error = "No unit on source."; return false; }
        if (unit.Owner != CurrentPlayer) { error = "Not your unit."; return false; }
        if (unit.HasActedThisTurn) { error = "This unit already acted this turn."; return false; }

        var dstTile = Map.At(to);
        if (dstTile.Occupant != null) { error = "Destination occupied."; return false; }

        var reachable = new HashSet<Pos>(GetReachableMoves(unit));
        if (!reachable.Contains(to)) { error = "Not reachable."; return false; }

        Map.At(from).Occupant = null;
        dstTile.Occupant = unit;
        unit.SetPos(to);

        // capture mine/temple on step
        if (dstTile.IsCapturable && dstTile.Owner != unit.Owner)
            dstTile.Owner = unit.Owner;

        // castle capture -> immediate win
        if (dstTile.Type == TileType.CASTLE && dstTile.Owner != unit.Owner)
        {
            dstTile.Owner = unit.Owner;
            DeclareWinner(unit.Owner);
        }

        unit.MarkActed();
        CheckTempleVictory();
        return true;
    }

    public bool TryAttack(
        Pos from,
        Pos targetPos,
        out (bool attackerWins, int atkRoll, int defRoll) result,
        out string error)
    {
        result = (false, 0, 0);
        error = null;

        if (State == GameState.GameOver) { error = "Game is over."; return false; }
        if (!Map.InBounds(from) || !Map.InBounds(targetPos)) { error = "Out of bounds."; return false; }

        var attacker = GetUnitAt(from);
        var defender = GetUnitAt(targetPos);

        if (attacker == null) { error = "No attacker unit."; return false; }
        if (defender == null) { error = "No defender unit."; return false; }
        if (attacker.Owner != CurrentPlayer) { error = "Not your unit."; return false; }
        if (attacker.HasActedThisTurn) { error = "This unit already acted this turn."; return false; }
        if (defender.Owner == attacker.Owner) { error = "Cannot attack own unit."; return false; }

        int dist = Abs(attacker.Pos.X - targetPos.X) + Abs(attacker.Pos.Y - targetPos.Y);

        // FOREST rule: if defender is in forest, archer can only attack adjacent
        int effectiveRange = attacker.Range;
        if (attacker is Archer && Map.At(targetPos).Type == TileType.FOREST)
            effectiveRange = 1;

        if (dist > effectiveRange) { error = "Out of range."; return false; }

        result = CombatService.Resolve(attacker, defender);

        if (result.attackerWins)
        {
            // defender dies
            Map.At(targetPos).Occupant = null;
            Units.Remove(defender);

            // attacker moves onto target tile
            Map.At(from).Occupant = null;

            var dstTile = Map.At(targetPos);
            dstTile.Occupant = attacker;
            attacker.SetPos(targetPos);

            // capture only mine/temple
            if (dstTile.IsCapturable && dstTile.Owner != attacker.Owner)
                dstTile.Owner = attacker.Owner;
        }

        attacker.MarkActed();
        CheckTempleVictory();
        return true;
    }

    public void EndTurn()
    {
        if (State == GameState.GameOver) return;

        if (CheckCastleVictory()) return;
        if (CheckTempleVictory()) return;

        CurrentPlayer = (CurrentPlayer == Owner.RED) ? Owner.BLUE : Owner.RED;
        Turn++;

        foreach (var u in Units)
            if (u.Owner == CurrentPlayer)
                u.ResetActed();

        EconomyService.GiveTurnIncome(this);
    }

    // ---------------- helpers ----------------

    private void TryPlaceUnit(Unit u)
    {
        if (u == null || Map == null) return;
        if (!Map.InBounds(u.Pos)) return;

        var t = Map.At(u.Pos);
        if (t.Occupant != null) return;

        t.Occupant = u;
        Units.Add(u);
    }

    private bool CheckCastleVictory()
    {
        var redOwner = Map.At(RedCastle).Owner;
        var blueOwner = Map.At(BlueCastle).Owner;

        if (redOwner == Owner.BLUE) { DeclareWinner(Owner.BLUE); return true; }
        if (blueOwner == Owner.RED) { DeclareWinner(Owner.RED); return true; }
        return false;
    }

    private bool CheckTempleVictory()
    {
        int temples = 0;
        int red = 0;
        int blue = 0;

        for (int y = 0; y < Map.Height; y++)
            for (int x = 0; x < Map.Width; x++)
            {
                var t = Map.Tiles[y, x];
                if (t.Type != TileType.TEMPLE) continue;
                temples++;
                if (t.Owner == Owner.RED) red++;
                else if (t.Owner == Owner.BLUE) blue++;
            }

        if (temples == 0) return false;

        if (red == temples) { DeclareWinner(Owner.RED); return true; }
        if (blue == temples) { DeclareWinner(Owner.BLUE); return true; }
        return false;
    }

    private void DeclareWinner(Owner w)
    {
        Winner = w;
        State = GameState.GameOver;
    }

    private static IEnumerable<Pos> Neighbors4(Pos p)
    {
        yield return new Pos(p.X - 1, p.Y);
        yield return new Pos(p.X + 1, p.Y);
        yield return new Pos(p.X, p.Y - 1);
        yield return new Pos(p.X, p.Y + 1);
    }

    private static int Abs(int v) => v < 0 ? -v : v;

    // ---------------- save/load ----------------

    public void LoadFromSave(SaveRoot root)
    {
        if (root == null || root.game == null || root.game.map == null)
            return;

        var g = root.game;

        Turn = g.turn <= 0 ? 1 : g.turn;

        if (!System.Enum.TryParse(g.currentPlayer, out Owner cp))
            cp = Owner.RED;
        CurrentPlayer = cp;

        State = (g.state == "GameOver") ? GameState.GameOver : GameState.Running;
        Winner = Owner.NONE;

        Map = new Map(g.map.width, g.map.height);

        if (g.map.tiles != null)
        {
            foreach (var td in g.map.tiles)
            {
                if (td == null) continue;

                if (!System.Enum.TryParse(td.type, out TileType tt))
                    tt = TileType.PLAINS;

                if (!System.Enum.TryParse(td.owner, out Owner to))
                    to = Owner.NONE;

                var tile = Map.Tiles[td.y, td.x];
                tile.Type = tt;
                tile.Owner = to;
                tile.Occupant = null;
            }
        }

        if (g.castles != null && g.castles.RED != null && g.castles.BLUE != null)
        {
            RedCastle = new Pos(g.castles.RED.x, g.castles.RED.y);
            BlueCastle = new Pos(g.castles.BLUE.x, g.castles.BLUE.y);
        }
        else
        {
            RedCastle = new Pos(0, Map.Height - 1);
            BlueCastle = new Pos(Map.Width - 1, 0);
        }

        int redRes = 0, blueRes = 0;
        string redName = "Red Empire", blueName = "Blue Kingdom";

        if (g.players != null)
        {
            foreach (var p in g.players)
            {
                if (p == null) continue;
                if (p.id == "RED") { redRes = p.resources; if (!string.IsNullOrEmpty(p.name)) redName = p.name; }
                if (p.id == "BLUE") { blueRes = p.resources; if (!string.IsNullOrEmpty(p.name)) blueName = p.name; }
            }
        }

        Red = new Player(Owner.RED, redName, redRes);
        Blue = new Player(Owner.BLUE, blueName, blueRes);

        Units.Clear();
        if (g.units != null)
        {
            foreach (var ud in g.units)
            {
                if (ud == null || ud.pos == null) continue;

                if (!System.Enum.TryParse(ud.owner, out Owner ou))
                    ou = Owner.RED;

                var pos = new Pos(ud.pos.x, ud.pos.y);

                Unit u = ud.type switch
                {
                    "Soldier" => new Soldier(ou, pos),
                    "Knight" => new Knight(ou, pos),
                    "Archer" => new Archer(ou, pos),
                    _ => new Soldier(ou, pos)
                };

                Units.Add(u);
                if (Map.InBounds(pos))
                    Map.At(pos).Occupant = u;
            }
        }
        foreach (var u in Units)
            if (u.Owner == CurrentPlayer)
                u.ResetActed();
    }
}
