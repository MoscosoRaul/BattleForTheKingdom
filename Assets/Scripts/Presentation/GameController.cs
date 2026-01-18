using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Game")]
    public int width = 7, height = 7;
    public int seed = 123456;

    public GameModel Model { get; private set; }

    [Header("Board")]
    [SerializeField] private BoardRenderer boardRenderer;

    [Header("HUD")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text combatText;

    [Header("Buttons (optional)")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    [Header("Recruit UI (can be left empty, will auto-find)")]
    [SerializeField] private RecruitPanel recruitPanel;

    private Pos? selectedPos = null;
    private HashSet<Pos> moveTargets = new();
    private HashSet<Pos> attackTargets = new();

    private void Start()
    {
        // auto-find references if not set
        if (boardRenderer == null)
            boardRenderer = FindFirstObjectByType<BoardRenderer>();

        if (recruitPanel == null)
            recruitPanel = FindFirstObjectByType<RecruitPanel>(FindObjectsInactive.Include);

        Debug.Log("[GameController] boardRenderer = " + (boardRenderer != null));
        Debug.Log("[GameController] recruitPanel = " + (recruitPanel != null));

        // create model + new game
        Model = new GameModel();
        Model.StartNew(width, height, seed);

        // Build UI grid + generate terrain into the MODEL map + initial render
        if (boardRenderer != null)
            boardRenderer.GenerateAndRender(this, Model, ref seed);

        // init recruit panel listeners
        if (recruitPanel != null)
            recruitPanel.Init(this);

        ClearSelection();
        ClearCombatMessage();
        SyncAll();
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    // TileView calls this
    public void OnTileClicked(int x, int y)
    {
        // Debug spam (ha nem kell, kikommentezheted)
        // Debug.Log($"CLICK ({x},{y})");

        if (Model == null || Model.State == GameState.GameOver) return;

        var p = new Pos(x, y);

        // Castle click -> open recruit
        var clickedTile = Model.Map.At(p);
        if (clickedTile.Type == TileType.CASTLE && clickedTile.Owner == Model.CurrentPlayer)
        {
            if (recruitPanel == null)
                recruitPanel = FindFirstObjectByType<RecruitPanel>(FindObjectsInactive.Include);

            if (recruitPanel != null)
            {
                recruitPanel.Show();
                recruitPanel.Refresh();
            }
            return;
        }

        var clickedUnit = Model.GetUnitAt(p);

        // No selection yet -> select your unit that can still act
        if (selectedPos == null)
        {
            if (clickedUnit != null && clickedUnit.Owner == Model.CurrentPlayer && !clickedUnit.HasActedThisTurn)
                Select(p);
            else
                ClearSelection();

            SyncAll();
            return;
        }

        // Has selection
        var sel = selectedPos.Value;
        var selectedUnit = Model.GetUnitAt(sel);

        if (selectedUnit == null || selectedUnit.Owner != Model.CurrentPlayer)
        {
            ClearSelection();
            SyncAll();
            return;
        }

        // click same tile -> deselect
        if (p == sel)
        {
            ClearSelection();
            SyncAll();
            return;
        }

        // click another own unit -> switch selection (if can act)
        if (clickedUnit != null && clickedUnit.Owner == Model.CurrentPlayer && !clickedUnit.HasActedThisTurn)
        {
            Select(p);
            SyncAll();
            return;
        }

        // ---- ACTIONS ----
        // We attempt attack first, then move (even if highlight sets are wrong).

        // 1) attack attempt
        {
            // 1) attack attempt
            {
                var attacker = Model.GetUnitAt(sel);
                var defender = Model.GetUnitAt(p);

                bool okA = Model.TryAttack(sel, p, out var fight, out string errA);
                if (okA)
                {
                    int aBase = attacker != null ? attacker.BaseAtk : 0;
                    int dBase = defender != null ? defender.BaseDef : 0;

                    int aTotal = aBase + fight.atkRoll;
                    int dTotal = dBase + fight.defRoll;

                    string outcome = fight.attackerWins ? "WIN" : "LOSE/TIE";
                    ShowCombatMessage($"ATK {aBase}+{fight.atkRoll}={aTotal}  vs  DEF {dBase}+{fight.defRoll}={dTotal}  ->  {outcome}");

                    ClearSelection();
                    SyncAll();
                    return;
                }
            }
            // optional: ShowCombatMessage("ATTACK FAILED: " + errA);
        }

        // 2) move attempt
        {
            bool okM = Model.TryMove(sel, p, out string errM);
            if (okM)
            {
                ClearCombatMessage(); // move után ne maradjon kint régi attack info
                ClearSelection();
                SyncAll();
                return;
            }
            // ha akarsz minimál visszajelzést mozgásra is:
            // ShowCombatMessage("MOVE FAIL: " + errM);
        }

        // otherwise keep selection + highlights
        SyncAll();
    }

    public void OnEndTurnButton()
    {
        if (Model == null || Model.State == GameState.GameOver) return;

        Model.EndTurn();
        ClearSelection();
        ClearCombatMessage();
        SyncAll();

        if (recruitPanel != null)
            recruitPanel.Refresh();
    }

    public void OnSaveButton()
    {
        if (Model == null) return;
        SaveLoadManager.Save(Model, seed);
        ShowCombatMessage("Saved.");
    }

    public void OnLoadButton()
    {
        var root = SaveLoadManager.Load();
        if (root == null) return;

        if (root.metadata != null) seed = root.metadata.seed;

        Model.LoadFromSave(root);

        ClearSelection();
        ClearCombatMessage();
        SyncAll();

        if (recruitPanel != null)
            recruitPanel.Refresh();

        ShowCombatMessage("Loaded.");
    }

    // ---------------- recruit ----------------

    public bool TryRecruitFromCastle(string unitType)
    {
        if (Model == null || Model.State == GameState.GameOver) return false;

        // simple limit: max 10 units per player
        int owned = 0;
        foreach (var uu in Model.Units)
            if (uu.Owner == Model.CurrentPlayer) owned++;
        if (owned >= 10)
        {
            ShowCombatMessage("Recruit failed: unit limit (10).");
            return false;
        }

        Owner owner = Model.CurrentPlayer;
        Pos castle = (owner == Owner.RED) ? Model.RedCastle : Model.BlueCastle;

        int price = unitType switch
        {
            "Soldier" => 20,
            "Knight" => 55,
            "Archer" => 30,
            _ => 999999
        };

        var player = (owner == Owner.RED) ? Model.Red : Model.Blue;

        if (player.Resources < price)
        {
            ShowCombatMessage($"Recruit failed: need {price}, have {player.Resources}.");
            return false;
        }

        // find an empty adjacent tile
        Pos? place = FindEmptyAdjacent(castle);
        if (place == null)
        {
            ShowCombatMessage("Recruit failed: no free adjacent tile.");
            return false;
        }

        Unit u = unitType switch
        {
            "Soldier" => new Soldier(owner, place.Value),
            "Knight" => new Knight(owner, place.Value),
            "Archer" => new Archer(owner, place.Value),
            _ => null
        };

        if (u == null) return false;

        var t = Model.Map.At(place.Value);
        if (t.Occupant != null) return false;

        // place into model
        t.Occupant = u;
        Model.Units.Add(u);

        // pay
        player.Resources -= price;

        ClearCombatMessage();
        SyncAll();
        return true;
    }

    private Pos? FindEmptyAdjacent(Pos p)
    {
        var neigh = new[]
        {
            new Pos(p.X - 1, p.Y),
            new Pos(p.X + 1, p.Y),
            new Pos(p.X, p.Y - 1),
            new Pos(p.X, p.Y + 1),
        };

        foreach (var n in neigh)
        {
            if (!Model.Map.InBounds(n)) continue;
            if (Model.Map.At(n).Occupant != null) continue;
            return n;
        }

        return null;
    }

    // ---------------- selection + sync ----------------

    private void Select(Pos p)
    {
        selectedPos = p;
        RecomputeTargets();
    }

    private void ClearSelection()
    {
        selectedPos = null;
        moveTargets.Clear();
        attackTargets.Clear();
    }

    private void RecomputeTargets()
    {
        moveTargets.Clear();
        attackTargets.Clear();

        if (selectedPos == null) return;

        var u = Model.GetUnitAt(selectedPos.Value);
        if (u == null) return;

        foreach (var mp in Model.GetReachableMoves(u))
            moveTargets.Add(mp);

        foreach (var ap in Model.GetAttackTargets(u))
            attackTargets.Add(ap);
    }

    private void SyncAll()
    {
        if (selectedPos != null)
            RecomputeTargets();

        if (boardRenderer != null)
            boardRenderer.SyncFromModel(Model, selectedPos, moveTargets, attackTargets);

        UpdateHUD();
        UpdateGameOverUI();

        // recruit panel resource frissítés (ha épp nyitva van)
        if (recruitPanel != null)
            recruitPanel.Refresh();
    }

    private void UpdateHUD()
    {
        if (turnText == null || Model == null) return;

        string p = (Model.CurrentPlayer == Owner.RED) ? "RED" : "BLUE";
        turnText.text = $"TURN: {Model.Turn} |\nPLAYER:\n{p}";
    }

    private void UpdateGameOverUI()
    {
        bool isGameOver = (Model != null && Model.State == GameState.GameOver);

        if (gameOverText != null)
        {
            if (!isGameOver)
            {
                gameOverText.text = "";
                gameOverText.gameObject.SetActive(false);
            }
            else
            {
                string w = (Model.Winner == Owner.RED) ? "RED" : "BLUE";
                gameOverText.text = $"{w} WINS!";
                gameOverText.gameObject.SetActive(true);
            }
        }

        if (endTurnButton != null) endTurnButton.interactable = !isGameOver;
        if (saveButton != null) saveButton.interactable = true;
        if (loadButton != null) loadButton.interactable = true;
    }

    // ---------------- combat message ----------------

    private void ShowCombatMessage(string msg)
    {
        if (combatText == null) return;
        combatText.text = msg;
    }

    private void ClearCombatMessage()
    {
        if (combatText == null) return;
        combatText.text = "";
    }
}
