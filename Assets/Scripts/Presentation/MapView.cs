using UnityEngine;
public class MapView : MonoBehaviour
{
    Map map; Pos redCastle, blueCastle; public float cellSize = 1f;
    public void Show(Map m, Pos red, Pos blue) { map = m; redCastle = red; blueCastle = blue; }
    void OnDrawGizmos()
    {
        if (map == null) return;
        for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++)
            {
                var t = map.Tiles[y, x];
                Gizmos.color = Color.gray;
                if (t.Type == TileType.FOREST) Gizmos.color = Color.green;
                else if (t.Type == TileType.MINE) Gizmos.color = Color.yellow;
                else if (t.Type == TileType.TEMPLE) Gizmos.color = Color.cyan;
                else if (t.Type == TileType.CASTLE) Gizmos.color = Color.red;
                var pos = new Vector3(x * cellSize, y * -cellSize, 0);
                Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.95f);
            }
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(redCastle.X * cellSize, redCastle.Y * -cellSize, 0), Vector3.one * cellSize * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(new Vector3(blueCastle.X * cellSize, blueCastle.Y * -cellSize, 0), Vector3.one * cellSize * 0.5f);
    }
}
