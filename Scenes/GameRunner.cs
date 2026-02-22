using Godot;
using System.Collections.Generic;

public partial class GameRunner : Node
{
    public GameState State;
    private Entity Me, Opp;
    private List<Card> _compiled = new();

    public override void _Ready()
    {
        // Ensure global card pool is loaded once (acts like autoload)
        if (CardDatabase.Blueprints.Count == 0)
        CardDatabase.LoadFromCsv("res://Data/cards.csv");

        State = new GameState();
        Me = State.PlayerA; Opp = State.PlayerB;

        // Load CSV → compile → build a tiny deck for demo
        var rows = CardCsvReader.Load("res://Data/cards.csv", out var diags);
        foreach (var d in diags) GD.PrintRich($"[color=yellow]{d}[/color]");
        foreach (var r in rows) _compiled.Add(CardCsvCompiler.Compile(r));

        // Simple library: take first 10 compiled cards
        for (int i=0; i<10 && i<_compiled.Count; i++) State.LibraryA.Add(_compiled[i]);

        // Draw opening hand
        State.Draw(Me, 3);
        DumpHand();

        // Listen to events
        State.Bus.OnEvent += ge => GD.Print($"Event: {ge.Type}");
        State.OpenPriorityWindow();

        GD.Print("Keys: [T]=cast top of card 1 | [B]=bottom | [Y]=channel top | [SPACE]=pass | [R]=resolve top");
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("ui_select")) { Pass(); } // space by default
        if (e.IsActionPressed("ui_accept")) { ResolveTop(); } // enter
        if (e is InputEventKey k && k.Pressed)
        {
            if (k.Keycode == Key.T) CastTop(0);
            if (k.Keycode == Key.B) CastBottom(0);
            if (k.Keycode == Key.Y) CastTopChannel(0);
        }
    }

    void CastTop(int handIndex)
    {
        if (State.HandA.Count <= handIndex) { GD.Print("No card."); return; }
        var c = State.HandA[handIndex];
        var a = c.TopHalf;
        if (a == null) { GD.Print("No top half."); return; }
        if (Rules.TryCast(a, State, Me)) GD.Print($"→ Cast top: {a.Name}");
    }
    void CastTopChannel(int handIndex)
    {
        if (State.HandA.Count <= handIndex) return;
        var a = State.HandA[handIndex].TopHalf?.ChannelVariant;
        if (a==null){ GD.Print("No channel."); return; }
        if (Rules.TryCast(a, State, Me)) GD.Print($"→ Cast channel: {a.Name}");
    }
    void CastBottom(int handIndex)
    {
        if (State.HandA.Count <= handIndex) return;
        var a = State.HandA[handIndex].BottomHalf;
        if (a == null) { GD.Print("No bottom half."); return; }
        if (Rules.TryCast(a, State, Me)) GD.Print($"→ Cast bottom: {a.Name}");
    }

    void Pass()
    {
        var advanced = State.Priority.PassPriority(State);
        if (!advanced) GD.Print($"Pass. Priority → {(State.Priority.PriorityHolder==Me?"Me":"Opp")}");
    }

    void ResolveTop()
    {
        if (State.Stack.IsEmpty) { GD.Print("Stack empty."); return; }
        State.Resolver.ResolveTop(State);
    }

    void DumpHand()
    {
        GD.Print("Hand:");
        for (int i=0; i<State.HandA.Count; i++)
        {
            var c = State.HandA[i];
            GD.Print($"[{i}] {c.CardName}  (Top:{c.TopHalf?.Name ?? "-"} | Bottom:{c.BottomHalf?.Name ?? "-"})");
        }
    }
}