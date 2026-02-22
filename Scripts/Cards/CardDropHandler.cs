using Godot;
using System;

public partial class CardDropHandler : Node3D
{
    private Camera3D camera;

    public override void _Ready()
    {
        var cameraNode = GetViewport().GetCamera3D();
        if (cameraNode != null)
            camera = cameraNode;
        else
            GD.PrintErr("Camera3D not found for CardDropHandler!");
    }

    public void TryDropCardOnTile()
{
    if (!DragPayloadManager.IsDragging || camera == null) return;

    Vector2 mousePos = GetViewport().GetMousePosition();
    Vector3 from = camera.ProjectRayOrigin(mousePos);
    Vector3 to = from + camera.ProjectRayNormal(mousePos) * 1000f;

    var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D
    {
        From = from,
        To = to,
        CollisionMask = 1
    });

    if (result.TryGetValue("collider", out var colliderVar))
    {
        Node colliderNode = colliderVar.As<Node>();
        if (colliderNode == null) return;

        var hexTile = GetParentHexTile(colliderNode);
        if (hexTile == null) return;

        var cardUi = DragPayloadManager.DraggedCard;
        bool isTop = DragPayloadManager.IsTopHalf;

        if (cardUi != null)
        {
            var half = isTop ? cardUi.TopHalf : cardUi.BottomHalf;
            var halfName = half?.Name ?? "(null half)";

            GD.Print($"Card dropped on tile at {hexTile.GlobalPosition} — Playing {halfName}");
            cardUi.EmitSignal(CardUi.SignalName.CardHalfSelected, cardUi, isTop);

            DragPayloadManager.IsDragging = false;
        }
    }
}

    private HexTile GetParentHexTile(Node node)
    {
        while (node != null && node is not HexTile)
        {
            node = node.GetParent();
        }
        return node as HexTile;
    }
}
