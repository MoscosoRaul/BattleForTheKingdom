using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text resourcesText;

    [SerializeField] private Button soldierBtn;
    [SerializeField] private Button knightBtn;
    [SerializeField] private Button archerBtn;
    [SerializeField] private Button closeBtn;

    private GameController gc;

    private void Awake()
    {
        // Ha nincs root bekötve, használjuk saját magunkat.
        if (root == null) root = gameObject;

        // Induláskor elrejtjük (ez normális).
        Hide();
        Debug.Log("[RecruitPanel] Awake OK, hidden");
    }

    public void Init(GameController controller)
    {
        gc = controller;
        Debug.Log("[RecruitPanel] Init OK, gc set");

        if (soldierBtn != null)
        {
            soldierBtn.onClick.RemoveAllListeners();
            soldierBtn.onClick.AddListener(() => TryRecruit("Soldier"));
        }

        if (archerBtn != null)
        {
            archerBtn.onClick.RemoveAllListeners();
            archerBtn.onClick.AddListener(() => TryRecruit("Archer"));
        }

        if (knightBtn != null)
        {
            knightBtn.onClick.RemoveAllListeners();
            knightBtn.onClick.AddListener(() => TryRecruit("Knight"));
        }

        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(Hide);
        }

        Refresh();
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        Debug.Log("[RecruitPanel] Show()");
        Refresh();
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        Debug.Log("[RecruitPanel] Hide()");
    }

    public void Refresh()
    {
        if (gc == null || gc.Model == null) return;

        int res = (gc.Model.CurrentPlayer == Owner.RED) ? gc.Model.Red.Resources : gc.Model.Blue.Resources;
        if (resourcesText != null)
            resourcesText.text = $"Resources: {res}";
    }

    private void TryRecruit(string type)
    {
        if (gc == null)
        {
            Debug.Log("[RecruitPanel] TryRecruit FAILED: gc null");
            return;
        }

        Debug.Log("[RecruitPanel] TryRecruit: " + type);
        bool ok = gc.TryRecruitFromCastle(type);
        Refresh();

        if (ok) Hide();
    }
}
