using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileView : MonoBehaviour
{
    public enum Highlight { None, Selected, MoveTarget, AttackTarget }

    [Header("Refs")]
    [SerializeField] private GameObject unitIcon;
    [SerializeField] private TMP_Text unitLabel;
    [SerializeField] private TMP_Text tileOwnerLabel; // TileOwnerLabel (TMP)

    public int X { get; private set; }
    public int Y { get; private set; }

    private GameController controller;
    private Button button;
    private Image bg;

    private Color baseColor = new Color(0.7f, 0.7f, 0.7f);

    private readonly Color selectedColor = new Color(1f, 0.85f, 0.3f);
    private readonly Color moveColor = new Color(0.4f, 0.7f, 1f);
    private readonly Color attackColor = new Color(1f, 0.45f, 0.25f);

    public void Init(int x, int y, GameController gc)
    {
        X = x; Y = y;
        controller = gc;

        button = GetComponent<Button>();
        bg = GetComponent<Image>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        // auto-find refs if not assigned
        if (unitIcon == null)
        {
            var t = transform.Find("UnitIcon");
            if (t != null) unitIcon = t.gameObject;
        }

        if (unitLabel == null && unitIcon != null)
        {
            var t = unitIcon.transform.Find("UnitLabel");
            if (t != null) unitLabel = t.GetComponent<TMP_Text>();
        }

        if (tileOwnerLabel == null)
        {
            var t = transform.Find("TileOwnerLabel");
            if (t != null) tileOwnerLabel = t.GetComponent<TMP_Text>();
        }

        // CRITICAL: overlays must not block tile clicks
        if (unitIcon != null)
        {
            var iconImg = unitIcon.GetComponent<Image>();
            if (iconImg != null) iconImg.raycastTarget = false;
        }
        if (unitLabel != null) unitLabel.raycastTarget = false;
        if (tileOwnerLabel != null) tileOwnerLabel.raycastTarget = false;

        SetUnit(null, false, null);
        SetTileOwnerMark(TileType.PLAINS, Owner.NONE);
        SetBaseColor(baseColor);
        SetHighlight(Highlight.None);
    }

    private void OnClick()
    {
        if (controller == null) return;
        controller.OnTileClicked(X, Y);
    }

    public void SetBaseColor(Color c)
    {
        baseColor = c;
        if (bg != null) bg.color = baseColor;
    }

    public void SetHighlight(Highlight h)
    {
        if (bg == null) return;

        bg.color = h switch
        {
            Highlight.Selected => selectedColor,
            Highlight.MoveTarget => moveColor,
            Highlight.AttackTarget => attackColor,
            _ => baseColor
        };
    }

    public void SetUnit(Owner? owner, bool exhausted, string unitTypeShort)
    {
        if (unitIcon != null)
            unitIcon.SetActive(owner != null);

        if (owner == null)
        {
            if (unitLabel != null) unitLabel.text = "";
            return;
        }

        var iconImg = unitIcon != null ? unitIcon.GetComponent<Image>() : null;
        if (iconImg != null)
        {
            iconImg.color = (owner == Owner.RED)
                ? new Color(1f, 0.2f, 0.2f)
                : new Color(0.2f, 0.6f, 1f);

            var c = iconImg.color;
            c.a = exhausted ? 0.35f : 1f;
            iconImg.color = c;
        }

        if (unitLabel != null)
        {
            unitLabel.text = unitTypeShort ?? "?";
            unitLabel.color = Color.white;
        }
    }

    public void SetTileOwnerMark(TileType type, Owner owner)
    {
        if (tileOwnerLabel == null) return;

        bool capturable = (type == TileType.MINE || type == TileType.TEMPLE);

        if (!capturable || owner == Owner.NONE)
        {
            tileOwnerLabel.text = "";
            return;
        }

        tileOwnerLabel.text = (owner == Owner.RED) ? "R" : "B";
        tileOwnerLabel.color = Color.white;
    }
}
