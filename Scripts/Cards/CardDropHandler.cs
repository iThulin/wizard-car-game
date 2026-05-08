using Godot;

public partial class CardDropHandler : Node3D
{
    private Camera3D camera;
    public HexTile CurrentHoveredTile { get; private set; }

    // Tracks last-frame drag state so we can detect transitions
    private bool _wasDragging = false;

    [Signal]
    public delegate void CardDroppedOnTileEventHandler(CardUi cardUi, bool isTop, HexTile tile);

    [Signal]
    public delegate void CardDragStartedEventHandler(CardUi cardUi, bool isTop);

    [Signal]
    public delegate void CardDragEndedEventHandler();

    public override void _Ready()
    {
        camera = GetViewport().GetCamera3D();
        if (camera == null)
            GD.PrintErr("Camera3D not found for CardDropHandler!");
    }

    public override void _Process(double delta)
    {
        bool isDragging = DragPayloadManager.IsDragging;

        // Detect drag start
        if (isDragging && !_wasDragging)
        {
            var cardUi = DragPayloadManager.DraggedCard;
            bool isTop = DragPayloadManager.IsTopHalf;
            if (cardUi != null)
                EmitSignal(SignalName.CardDragStarted, cardUi, isTop);
        }
        // Detect drag end (without a drop — i.e. cancelled)
        else if (!isDragging && _wasDragging)
        {
            EmitSignal(SignalName.CardDragEnded);
        }

        _wasDragging = isDragging;

        if (!isDragging || camera == null)
        {
            if (CurrentHoveredTile != null) ClearHoverHighlight();
            return;
        }

        var newTile = RaycastToHexTile();

        if (newTile != CurrentHoveredTile)
        {
            ClearHoverHighlight();
            CurrentHoveredTile = newTile;
            newTile?.SetDragHoverHighlight(true);
        }
    }

    public void TryDropCardOnTile()
    {
        if (!DragPayloadManager.IsDragging) return;

        var cardUi = DragPayloadManager.DraggedCard;
        bool isTop = DragPayloadManager.IsTopHalf;
        var tile = CurrentHoveredTile;

        // Always reset drag state at the end of an attempt
        DragPayloadManager.IsDragging = false;
        ClearHoverHighlight();

        // Fire the cast attempt only if a valid tile was hovered
        if (tile != null && cardUi != null)
        {
            var halfName = (isTop ? cardUi.TopHalf : cardUi.BottomHalf)?.Name ?? "(null half)";
            GD.Print($"Card dropped on tile {tile.Axial} — Playing {halfName}");
            EmitSignal(SignalName.CardDroppedOnTile, cardUi, isTop, tile);
        }

        // Always snap the card back visually. If the cast succeeded, the deck
        // manager will animate it to the discard pile from there.
        cardUi?.EndDrag();
    }

    private void ClearHoverHighlight()
    {
        CurrentHoveredTile?.SetDragHoverHighlight(false);
        CurrentHoveredTile = null;
    }

    private HexTile RaycastToHexTile()
    {
        if (camera == null) return null;

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 from = camera.ProjectRayOrigin(mousePos);
        Vector3 to = from + camera.ProjectRayNormal(mousePos) * 1000f;

        var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D
        {
            From = from,
            To = to,
            CollisionMask = 1
        });

        if (!result.TryGetValue("collider", out var colliderVar)) return null;
        return GetParentHexTile(colliderVar.As<Node>());
    }

    private HexTile GetParentHexTile(Node node)
    {
        while (node != null && node is not HexTile)
            node = node.GetParent();
        return node as HexTile;
    }
}