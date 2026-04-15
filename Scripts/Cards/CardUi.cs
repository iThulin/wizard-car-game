using Godot;
using System;

public partial class CardUi : Control
{
    public Card CardInstance { get; private set; }
    public CardHalf TopHalf { get; private set; }
    public CardHalf BottomHalf { get; private set; }
    private DeckUiManager _deckUiManager;

    private Control _visualNode;
    private Control topArea;
    private Control bottomArea;

    // Hover animation
    private bool _cardIsLifted = false;
    private float _restRotation;
    private float _smoothBreathe = 0f;
    private Tween _cardTween;       // handles card-level lift/tilt/scale
    private Tween _halfTween;       // handles per-half highlight modulate

    // Card exit safety (prevents getting stuck in lifted state if mouse leaves card unexpectedly)
    private bool _entryTweenComplete = false;
    private float _notOnCardTimer = 0f;
    private const float StuckExitTimeout = 0.08f; // force exit if mouse gone for 80ms
    private int _restTransformGeneration = 0;

    // Half-hover debounce (prevents flickering at the boundary)
    private string _currentHalf  = "none";   // "top" | "bottom" | "none"
    private string _pendingHalf  = "none";
    private float  _halfTimer    = 0f;
    private const float HalfDebounce = 0.08f;   // seconds before a half-switch commits

    // Panels to tint
    private Panel topPanel;
    private Panel bottomPanel;

    // Affordability tracking
    private static readonly Color TopActiveColor   = new Color(1.3f, 1.2f, 0.9f, 1f); // warm gold glow
    private static readonly Color BottomActiveColor= new Color(1.1f, 1.0f, 1.4f, 1f); // cool purple glow
    private static readonly Color DimColor         = new Color(0.55f, 0.55f, 0.55f, 1f);
    private static readonly Color NeutralColor     = new Color(1f, 1f, 1f, 1f);

    // Signals
    [Signal] public delegate void CardDroppedEventHandler();
    [Signal] public delegate void CardHalfSelectedEventHandler(CardUi cardUi, bool isTop);

    // Drag handling
    private bool _dragPressed = false;
    private bool _dragQueued = false;
    private bool _isDragging = false;
    private Vector2 _dragStartPosition;
    private Vector2 _originalPosition;
    public Vector2 _restPosition => _originalPosition; // Expose original position for external use (e.g., DeckUiManager)
    private const float DragThreshold = 10f;
    private bool _dragTopCard = false;
    private int _lastKnownMana = 999; // 999 = everything affordable until told otherwise
    public bool HasBeenPlaced { get; private set; } = false;

    public override void _Ready()
    {
        _visualNode  = GetNode<Control>("CardVisual");
        _originalPosition = Position;
        _restRotation = Rotation;

        topArea    = GetNode<Control>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl");
        bottomArea = GetNode<Control>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl");
        topPanel   = GetNode<Panel>("CardVisual/VBoxContainer/TopCardPanel");
        bottomPanel= GetNode<Panel>("CardVisual/VBoxContainer/BottomCardPanel");

        if (topPanel == null)    GD.PrintErr("CardUi: topPanel not found");
        if (bottomPanel == null) GD.PrintErr("CardUi: bottomPanel not found");

        // Card-level: entering/leaving the whole card lifts or drops it
        topArea.MouseEntered    += OnCardEnter;
        bottomArea.MouseEntered += OnCardEnter;
        topArea.MouseExited     += OnCardMaybeExit;
        bottomArea.MouseExited  += OnCardMaybeExit;

        // CardUi.cs — at end of _Ready(), before rest transform is known
        Position = _originalPosition + new Vector2(0, 300f); // start below
        Modulate = new Color(1, 1, 1, 0);                    // start transparent

        topArea.GuiInput    += (e) => OnCardGuiInput(e, true);
        bottomArea.GuiInput += (e) => OnCardGuiInput(e, false);
    }

    public override void _Process(double delta)
    {
        if (_entryTweenComplete && !_isDragging) // ← add !_isDragging
        {
            float targetBreathe = Mathf.Sin(
                (float)Time.GetTicksMsec() / 1000f * 1.2f + GetIndex() * 0.8f) * 2.5f;
            _smoothBreathe = Mathf.Lerp(_smoothBreathe, targetBreathe, (float)delta * 12f);

            float liftOffset = _cardIsLifted ? -35f : 0f;
            Position = _originalPosition + new Vector2(0, liftOffset + _smoothBreathe);
        }

        if (!_cardIsLifted || _isDragging) return;

        Vector2 mouse = GetViewport().GetMousePosition();

        // Check if mouse is within the card's overall global rect expanded slightly
        Rect2 cardRect = GetGlobalRect().GrowIndividual(10, 40, 10, 10);
        bool onCard = cardRect.HasPoint(mouse);

        if (!onCard)
        {
            _notOnCardTimer += (float)delta;
            if (_notOnCardTimer >= StuckExitTimeout)
            {
                DoCardExit();
                return;
            }
        }
        else
        {
            _notOnCardTimer = 0f;
        }

        // Half highlight — use the actual current rects since we're confirmed on card
        string detected = topArea.GetGlobalRect().HasPoint(mouse)    ? "top"
                        : bottomArea.GetGlobalRect().HasPoint(mouse) ? "bottom"
                        : "none";

        if (detected == _currentHalf)
        {
            _pendingHalf = detected;
            _halfTimer   = 0f;
        }
        else if (detected != _pendingHalf)
        {
            _pendingHalf = detected;
            _halfTimer   = 0f;
        }
        else
        {
            _halfTimer += (float)delta;
            if (_halfTimer >= HalfDebounce)
            {
                _currentHalf = _pendingHalf;
                _halfTimer   = 0f;
                ApplyHalfHighlight(_currentHalf);
            }
        }
    }

    public void SetDeckUiManager(DeckUiManager manager)
    {
        _deckUiManager = manager;
    }

    private void OnCardEnter()
    {
        if (_cardIsLifted) return;
        if (!_entryTweenComplete) return; // ← don't allow hover during draw animation

        _cardIsLifted = true;
        _notOnCardTimer = 0f;

        _cardTween?.Kill();
        _cardTween = CreateTween().SetParallel(true);
        _cardTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        _cardTween.TweenProperty(this, "rotation", _restRotation * 0.2f, 0.15f);
        _cardTween.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.15f);
        ZIndex = 100;

        _deckUiManager?.OnCardHoverChanged(this, true);
    }

    private void OnCardMaybeExit()
    {
        // Only actually exit if the mouse isn't still over the other half.
        // Use CallDeferred so Godot can process the new MouseEntered before we check.
        CallDeferred(nameof(CheckCardExit));
    }

    private void CheckCardExit()
    {
        Vector2 mouse = GetViewport().GetMousePosition();
        bool stillOnCard = topArea.GetGlobalRect().HasPoint(mouse) ||
                        bottomArea.GetGlobalRect().HasPoint(mouse);
        if (stillOnCard) return;

        DoCardExit();
    }

    private void DoCardExit()
    {
        if (!_cardIsLifted) return;

        _cardIsLifted = false;
        _notOnCardTimer = 0f;
        _currentHalf  = "none";
        _pendingHalf  = "none";
        _halfTimer    = 0f;

        _cardTween?.Kill();
        _cardTween = CreateTween().SetParallel(true);
        _cardTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        // Position is handled by _Process — only tween rotation and scale
        _cardTween.TweenProperty(this, "rotation", _restRotation, 0.12f);
        _cardTween.TweenProperty(this, "scale", Vector2.One, 0.12f);
        ZIndex = 0;

        _deckUiManager?.OnCardHoverChanged(this, false);
        RefreshAffordability(_lastKnownMana);
    }

    private void ApplyHalfHighlight(string activeHalf)
    {
        _halfTween?.Kill();
        _halfTween = CreateTween().SetParallel(true);
        _halfTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        if (activeHalf == "top")
        {
            _halfTween.TweenProperty(topPanel,    "modulate", TopActiveColor,    0.1f);
            _halfTween.TweenProperty(bottomPanel, "modulate", DimColor,          0.1f);
        }
        else if (activeHalf == "bottom")
        {
            _halfTween.TweenProperty(topPanel,    "modulate", DimColor,          0.1f);
            _halfTween.TweenProperty(bottomPanel, "modulate", BottomActiveColor, 0.1f);
        }
        else
        {
            // Restore to affordability colors, not blindly to white
            var topBase    = (TopHalf?.ManaCost    ?? 0) > _lastKnownMana
                ? new Color(0.7f, 0.5f, 0.5f, 1f) : Colors.White;
            var bottomBase = (BottomHalf?.ManaCost ?? 0) > _lastKnownMana
                ? new Color(0.7f, 0.5f, 0.5f, 1f) : Colors.White;

            _halfTween.TweenProperty(topPanel,    "modulate", topBase,    0.1f);
            _halfTween.TweenProperty(bottomPanel, "modulate", bottomBase, 0.1f);
        }
    }

    public void RefreshAffordability(int currentMana)
    {
        _lastKnownMana = currentMana;

        topPanel.Modulate    = (TopHalf?.ManaCost    ?? 0) > currentMana
            ? new Color(0.7f, 0.5f, 0.5f, 1f) : Colors.White;
        bottomPanel.Modulate = (BottomHalf?.ManaCost ?? 0) > currentMana
            ? new Color(0.7f, 0.5f, 0.5f, 1f) : Colors.White;
    }

    public void SetRestTransform(Vector2 position, float rotation)
    {
        bool isFirstPlacement = !HasBeenPlaced;
        HasBeenPlaced = true;

        _originalPosition = position;
        _restRotation     = rotation;
        _entryTweenComplete = false;
        _cardIsLifted = false;
        _notOnCardTimer = 0f;

        int generation = ++_restTransformGeneration;

        if (isFirstPlacement)
        {
            // Full draw-in animation for new cards
            Vector2 screenSize = GetViewport().GetVisibleRect().Size;
            Position  = new Vector2(position.X, screenSize.Y + 50f);
            Rotation  = rotation;
            Modulate  = new Color(1, 1, 1, 0f);
            Scale     = Vector2.One;

            float delay = GetIndex() * 0.09f;

            var tween = CreateTween().SetParallel(true);
            tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(this, "position", position, 0.45f).SetDelay(delay);
            tween.TweenProperty(this, "rotation", rotation, 0.45f).SetDelay(delay);
            tween.TweenProperty(this, "modulate", Colors.White, 0.30f)
                .SetDelay(delay + 0.10f);

            var timer = GetTree().CreateTimer(delay + 0.45f);
            timer.Timeout += () =>
            {
                if (generation == _restTransformGeneration)
                    _entryTweenComplete = true;
            };
        }
        else
        {
            // Already in hand — silently update rest position with no animation
            // Just unlock _Process immediately
            _entryTweenComplete = true;
        }
    }

    public void SetCard(Card card)
    {
        CardInstance = card;
        SetCard(card.TopHalf, card.BottomHalf); // call theexisting SetCard(CardHalf, CardHalf)
    }

    private void OnCardGuiInput(InputEvent @event, bool isTop)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                _dragPressed = true;
                _dragQueued  = false;
                _dragStartPosition = mb.GlobalPosition;
                _dragTopCard = isTop;
            }
            else
            {
                _dragPressed = false;
                if (!_dragQueued)
                    SnapBack();
            }
        }
        else if (_dragPressed && @event is InputEventMouseMotion motion)
        {
            if (!_dragQueued &&
                (motion.GlobalPosition - _dragStartPosition).Length() > DragThreshold)
            {
                _dragQueued = true;
                PlayGrabAnimation();
                // No SetDragPreview — card itself is the visual
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void PlayGrabAnimation()
    {
        _cardTween?.Kill();
        _cardTween = CreateTween().SetParallel(true);
        _cardTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

        _isDragging = true;

        // Tilt slightly in drag direction, shrink a touch, brighten
        float tiltDir = _dragTopCard ? -0.06f : 0.06f;
        _cardTween.TweenProperty(this, "rotation",
            _restRotation * 0.2f + tiltDir, 0.12f);
        _cardTween.TweenProperty(this, "scale",
            new Vector2(0.92f, 0.92f), 0.12f);
        _cardTween.TweenProperty(this, "modulate",
            new Color(1.1f, 1.1f, 1.1f, 0.85f), 0.12f);
        ZIndex = 200;
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        _isDragging = true;
        DragPayloadManager.DraggedCard = this;
        DragPayloadManager.IsTopHalf   = _dragTopCard;
        DragPayloadManager.IsDragging  = true;

        return new Godot.Collections.Dictionary
        {
            { "card", this },
            { "is_top", _dragTopCard }
        };
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.Obj is Godot.Collections.Dictionary dict)
        {
            return dict.ContainsKey("card"); //&& dict["card"] is CardUi;
        }
        return false;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (data.Obj is Godot.Collections.Dictionary dict)
        {
            if (dict.ContainsKey("card") && dict["card"] is object cardObj && cardObj is CardUi)
            {
                CardUi droppedCard = (CardUi)cardObj;

                droppedCard._isDragging = false; // ← reset on the card that was dragged
                droppedCard.Modulate = Colors.White;

                var container = GetParent();
                if (container is Control control)
                {
                    int dropIndex = control.GetChildren().IndexOf(this);
                    control.RemoveChild(droppedCard);
                    control.AddChild(droppedCard);
                    control.MoveChild(droppedCard, dropIndex);
                    droppedCard.SnapBack();
                    EmitSignal(SignalName.CardDropped);
                }
            }
        }
        DragPayloadManager.IsDragging = false;
    }

    private void SnapBack()
    {
        _isDragging = false;
        DoCardExit();
        DragPayloadManager.IsDragging = false;
        EmitSignal(SignalName.CardDropped);
    }

    public void SetCard(CardHalf top, CardHalf bottom)
{
    TopHalf = top;
    BottomHalf = bottom;

    // --- TOP ---
    var nameLabelTop = GetNodeOrNull<Label>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl/TopSpellContainer/HBoxContainer/NameLabel");
    var manaLabelTop = GetNodeOrNull<Label>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl/TopSpellContainer/HBoxContainer/ManaPanel/ManaLabel");
    var descLabelTop = GetNodeOrNull<RichTextLabel>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl/TopSpellContainer/DescriptionLabel");
    var channelPanelTop = GetNodeOrNull<Panel>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl/TopSpellContainer/ChannelPanel");
    var channelLabelTop = GetNodeOrNull<RichTextLabel>("CardVisual/VBoxContainer/TopCardPanel/TopCardControl/TopSpellContainer/ChannelPanel/ChannelLabel");

    if (nameLabelTop != null) nameLabelTop.Text = top?.Name ?? "";
    if (manaLabelTop != null) manaLabelTop.Text = (top?.ManaCost ?? 0).ToString();
    if (descLabelTop != null) descLabelTop.Text = top?.RulesText ?? "";

    var topChannelText = top?.ChannelVariant?.RulesText ?? "";
    if (channelPanelTop != null) channelPanelTop.Visible = !string.IsNullOrWhiteSpace(topChannelText);
    if (channelLabelTop != null) channelLabelTop.Text = topChannelText;

    // --- BOTTOM ---
    var nameLabelBot = GetNodeOrNull<Label>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl/BottomSpellContainer/NameHBoxContainer/NameLabel");
    var manaLabelBot = GetNodeOrNull<Label>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl/BottomSpellContainer/NameHBoxContainer/ManaPanel/ManaLabel");
    var descLabelBot = GetNodeOrNull<RichTextLabel>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl/BottomSpellContainer/DescriptionLabel");
    var channelPanelBot = GetNodeOrNull<Panel>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl/BottomSpellContainer/ChannelPanel");
    var channelLabelBot = GetNodeOrNull<RichTextLabel>("CardVisual/VBoxContainer/BottomCardPanel/BottomCardControl/BottomSpellContainer/ChannelPanel/ChannelLabel");

    if (nameLabelBot != null) nameLabelBot.Text = bottom?.Name ?? "";
    if (manaLabelBot != null) manaLabelBot.Text = (bottom?.ManaCost ?? 0).ToString();
    if (descLabelBot != null) descLabelBot.Text = bottom?.RulesText ?? "";

    var botChannelText = bottom?.ChannelVariant?.RulesText ?? "";
    if (channelPanelBot != null) channelPanelBot.Visible = !string.IsNullOrWhiteSpace(botChannelText);
    if (channelLabelBot != null) channelLabelBot.Text = botChannelText;
}
}
