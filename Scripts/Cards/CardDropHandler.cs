using Godot;

public partial class CardDropHandler : Node3D
{
    private Camera3D camera;
    public HexTile CurrentHoveredTile { get; private set; }

    [Signal]
    public delegate void CardDroppedOnTileEventHandler(CardUi cardUi, bool isTop, HexTile tile);

    public override void _Ready()
    {
        camera = GetViewport().GetCamera3D();
        if (camera == null)
            GD.PrintErr("Camera3D not found for CardDropHandler!");
    }

    public override void _Process(double delta)
    {
        if (!DragPayloadManager.IsDragging || camera == null)
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

        // Trust CurrentHoveredTile — it's updated every frame by _Process.
        // No need to re-raycast here.
        var tile = CurrentHoveredTile;
        if (tile == null) return;

        var cardUi = DragPayloadManager.DraggedCard;
        bool isTop = DragPayloadManager.IsTopHalf;
        if (cardUi == null) return;

        var halfName = (isTop ? cardUi.TopHalf : cardUi.BottomHalf)?.Name ?? "(null half)";
        GD.Print($"Card dropped on tile {tile.Axial} — Playing {halfName}");

        EmitSignal(SignalName.CardDroppedOnTile, cardUi, isTop, tile);

        DragPayloadManager.IsDragging = false;
        ClearHoverHighlight();
    }

    private void ClearHoverHighlight()
    {
        CurrentHoveredTile?.SetDragHoverHighlight(false);
        CurrentHoveredTile = null;
    }

    // Single shared raycast used by both _Process and any future callers
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
            CollisionMask = 1  // tiles only
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