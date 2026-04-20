using Godot;

public partial class GameBootstrap : Node
{
    public override void _Ready()
    {
        // Ensure card database is loaded before any gameplay scenes that rely on it.
        CardLoaderV2.LoadCardsFromJson("res://Data/Cards");
    }
}