using UnityEngine;

public class GameController : MonoBehaviour
{
    public int width = 7, height = 7;
    public int seed = 123456;

    public GameModel Model { get; private set; }
    public MapView mapView;

    void Start()
    {
        Model = new GameModel();
        Model.StartNew(width, height, seed);
        if (mapView != null)
            mapView.Show(Model.Map, Model.RedCastle, Model.BlueCastle);
    }

    public void OnEndTurnButton()   // <- PUBLIC, VOID, NO PARAMS !!!
    {
        Model.EndTurn();
        Debug.Log("End turn called!");
        if (Model.State == GameState.GameOver)
            Debug.Log("Game Over!");
    }

    public void OnSaveButton()     // <- PUBLIC, VOID, NO PARAMS !!!
    {
        SaveLoadManager.Save(Model, seed);
        Debug.Log("Save called!");
    }
}

