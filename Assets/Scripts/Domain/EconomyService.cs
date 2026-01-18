public static class EconomyService
{
    // Állítható balansz:
    public const int BASE_INCOME = 6;   // minden körben ennyi jár az aktuális játékosnak
    public const int MINE_INCOME = 4;   // minden saját bánya ennyit ad körönként

    public static void GiveTurnIncome(GameModel model)
    {
        if (model == null || model.Map == null) return;

        int income = BASE_INCOME;

        for (int y = 0; y < model.Map.Height; y++)
            for (int x = 0; x < model.Map.Width; x++)
            {
                var t = model.Map.Tiles[y, x];
                if (t.Type == TileType.MINE && t.Owner == model.CurrentPlayer)
                    income += MINE_INCOME;
            }

        var player = (model.CurrentPlayer == Owner.RED) ? model.Red : model.Blue;
        player.Resources += income;
    }
}
