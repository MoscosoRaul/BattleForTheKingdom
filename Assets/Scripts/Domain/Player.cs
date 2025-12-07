public class Player
{
    public Owner Id { get; }
    public string Name { get; }
    public int Resources { get; set; }
    public Player(Owner id, string name, int resources = 0) { Id = id; Name = name; Resources = resources; }
}
