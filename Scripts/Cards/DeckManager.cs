using Godot;
using System;
using System.Collections.Generic;

// ============================================================
// DeckManager.cs
//
// Purpose:        Scene-graph orchestrator that ties one
//                 UnitDeckData (the active unit's deck) to the
//                 DeckUI / HandUI nodes. Delegates all deck
//                 operations to UnitDeckData; owns the UI
//                 refresh + overflow-flagging logic.
// Layer:          System
// Collaborators:  UnitDeckData.cs (the active deck),
//                 DeckUiManager.cs (UI refresh),
//                 CardUi.cs (per-card UI), CardDatabase.cs
//                 (deck generation)
// See:            README §6 — Per-Unit Deck Management
// ============================================================

/// <summary>Combat-side scene-graph orchestrator for one unit's deck at a time. Wires the active <see cref="UnitDeckData"/> to the on-screen DeckUI/HandUI nodes, delegates deck ops, and recomputes hand-overflow discard flags after every change.</summary>
public partial class DeckManager : Node2D
{
    /// <summary>Maximum hand size for the active deck. Hand entries beyond this index get the discard-flag overlay.</summary>
    [Export] public int MaxHandSize = 5;

    private DeckUiManager uiManager;
    private Control handUIContainer;
    public Control HandContainer => handUIContainer;

    // The currently displayed unit's deck
    private UnitDeckData _activeDeck;

    // ── Public accessors (so existing code doesn't break) ───────
    public List<Card> DrawPile => _activeDeck?.DrawPile ?? new();
    public List<Card> Hand => _activeDeck?.Hand ?? new();
    public List<Card> DiscardPile => _activeDeck?.DiscardPile ?? new();

    public override void _Ready()
    {
        uiManager = GetNodeOrNull<DeckUiManager>("../../DeckUI/DeckUIManager");
        GD.Print(uiManager == null ? "DeckUIManager is NULL" : "DeckUIManager found");

        handUIContainer = GetNodeOrNull<Control>("../../DeckUI/HandUI");
        GD.Print(handUIContainer == null ? "HandUI is NULL" : "HandUI found");
    }

    // ── Active deck switching ───────────────────────────────────────────

    /// <summary>Sets the deck currently being displayed and refreshes the UI. Call when the selected player unit changes.</summary>
    public void SetActiveDeck(UnitDeckData deck)
    {
        _activeDeck = deck;
        uiManager?.SafeRefreshUI();
        CallDeferred(nameof(DeferredRefreshDiscardFlags));
    }

    /// <summary>Currently displayed deck. May be null before <see cref="SetActiveDeck"/> is called.</summary>
    public UnitDeckData GetActiveDeck() => _activeDeck;

    // ── Deck operations (delegated to active deck) ──────────────────────

    /// <summary>Replaces the active deck's contents with the given card list and refreshes the UI.</summary>
    public void InitializeDeck(List<Card> startingDeck)
    {
        if (_activeDeck == null) return;
        _activeDeck.Initialize(startingDeck);
        uiManager?.SafeRefreshUI();
    }

    /// <summary>Convenience pass-through to <see cref="CardDatabase.BuildRandomDeck"/> for the given school.</summary>
    public List<Card> GenerateStartingDeck(CardSchool school, int count = 5)
    {
        return CardDatabase.BuildRandomDeck(school, count);
    }

    /// <summary>Shuffles the active deck's draw pile and refreshes the UI.</summary>
    public void ShuffleDrawPile()
    {
        _activeDeck?.Shuffle();
        uiManager?.SafeRefreshUI();
    }

    /// <summary>Draws <paramref name="count"/> cards into the active hand and refreshes the UI. Logs every drawn card to the console.</summary>
    public void DrawCards(int count)
    {
        if (_activeDeck == null) return;

        var drawn = _activeDeck.Draw(count);
        foreach (var card in drawn)
            GD.Print($"Drew: {card.TopHalf?.Name ?? card.CardName} / {card.BottomHalf?.Name ?? ""}");

        uiManager?.SafeRefreshUI();
        CallDeferred(nameof(DeferredRefreshDiscardFlags));
    }

    private void DeferredRefreshDiscardFlags()
    {
        RefreshDiscardFlags();
    }

    /// <summary>Removes a specific card from the active hand without sending it to discard (used when a card is exiled or consumed mid-cast).</summary>
    public void RemoveCardFromHand(Card card)
    {
        if (_activeDeck == null) return;
        if (_activeDeck.Hand.Remove(card))
            GD.Print($"Removed card: {card.TopHalf?.Name ?? card.CardName}");
        uiManager?.SafeRefreshUI();
        CallDeferred(nameof(DeferredRefreshDiscardFlags));
    }

    /// <summary>Shuffles the discard pile back into the draw pile (mostly invoked automatically when the draw pile runs dry).</summary>
    public void Reshuffle()
    {
        _activeDeck?.Reshuffle();
        uiManager?.SafeRefreshUI();
    }

    /// <summary>Moves a card from hand to discard, refreshes the UI, and recomputes overflow flags.</summary>
    public void DiscardCard(Card card)
    {
        if (_activeDeck == null) return;
        _activeDeck.Discard(card);
        uiManager?.SafeRefreshUI();
        CallDeferred(nameof(DeferredRefreshDiscardFlags));
    }

    /// <summary>Diagnostic — prints the active deck's pile counts to the Godot console.</summary>
    public void PrintDeckState()
    {
        if (_activeDeck == null) { GD.Print("No active deck."); return; }
        GD.Print($"DeckManager state — Draw: {_activeDeck.DrawPile.Count}, Hand: {_activeDeck.Hand.Count}, Discard: {_activeDeck.DiscardPile.Count}");
    }

    /// <summary>Updates the visual "this card will be discarded at end of turn" flag on each CardUi in the hand container. Cards beyond <see cref="MaxHandSize"/> get the flag.</summary>
    public void RefreshDiscardFlags()
    {
        if (_activeDeck == null || handUIContainer == null) return;

        int overflowCount = _activeDeck.Hand.Count - _activeDeck.MaxHandSize;

        var cardUis = new List<CardUi>();
        foreach (Node child in handUIContainer.GetChildren())
        {
            if (child is CardUi cui) cardUis.Add(cui);
        }

        for (int i = 0; i < cardUis.Count; i++)
        {
            bool shouldFlag = overflowCount > 0 && i < overflowCount;
            cardUis[i].SetDiscardFlagged(shouldFlag);
        }
    }
}