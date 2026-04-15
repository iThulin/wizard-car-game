using Godot;

public partial class CameraController : Node3D
{
    // ── Tuning ───────────────────────────────────────────────────────────────
    [Export] public float MoveSpeed        = 12f;
    [Export] public float MoveLerpSpeed    = 10f;  // how snappy panning feels
    [Export] public float ZoomSpeed        = 4f;   // units per scroll tick
    [Export] public float ZoomLerpSpeed    = 8f;   // how snappy zoom feels
    [Export] public float MinZoom          = 5f;
    [Export] public float MaxZoom          = 30f;  // was 40 — tighter ceiling
    [Export] public float EdgeScrollMargin = 20f;
    [Export] public float DragSpeed        = 0.3f;
    [Export] public float RotationSpeed    = 0.3f;
    [Export] public float MinPitch         = -75f;
    [Export] public float MaxPitch         = -15f;
    [Export] public float BoundsPad        = 2f;   // was 4 — less overshoot

    // ── State ────────────────────────────────────────────────────────────────
    private float _yaw   = -45f;
    private float _pitch = -35f;

    private Camera3D        _camera;
    private Node3D          _pivot;
    private CardDropHandler _cardDropHandler;

    private Vector2 _lastMousePos;
    private Vector2 _mouseDelta;
    private bool    _dragging = false;

    private Vector3 _boundsMin = new Vector3(-10, 0, -10);
    private Vector3 _boundsMax = new Vector3(100, 0, 100);

    // Smooth zoom: we track a target Z distance and lerp toward it
    private float _zoomTarget;

    // Smooth pan: lerp the controller position toward a desired position
    private Vector3 _desiredPosition;

    private const float MouseToKeyboardRotationRatio = 200f;

    // ── Init ─────────────────────────────────────────────────────────────────
    public override void _Ready()
    {
        EnsureCameraNodes();
        _cardDropHandler = GetNodeOrNull<CardDropHandler>("/root/Main Scene/CardDropHandler");

        if (_pivot != null)
            _pivot.RotationDegrees = new Vector3(_pitch, _yaw, 0f);

        _desiredPosition = Position;
        _zoomTarget      = _camera?.Position.Z ?? 20f;
    }

    // ── FrameGrid ─────────────────────────────────────────────────────────────
    public void FrameGrid(Vector3 min, Vector3 max)
    {
        if (!EnsureCameraNodes()) return;

        _camera.Current = true;
        _boundsMin = min;
        _boundsMax = max;

        Vector3 center   = (min + max) * 0.5f;
        Vector3 size     = max - min;
        float   boardSpan = Mathf.Max(size.X, size.Z);

        // Reset rig
        Position = center;
        _desiredPosition = center;
        _pivot.Position         = Vector3.Zero;
        _pivot.RotationDegrees  = Vector3.Zero;
        _camera.Position        = Vector3.Zero;
        _camera.RotationDegrees = Vector3.Zero;

        _yaw   = -45f;
        _pitch = -35f;
        _pivot.RotationDegrees = new Vector3(_pitch, _yaw, 0f);

        // Start closer — 0.6 instead of 0.9
        float startZoom = Mathf.Clamp(boardSpan * 0.6f, MinZoom, MaxZoom);
        _camera.Position = new Vector3(0f, 0f, startZoom);
        _zoomTarget      = startZoom;

        GD.Print($"FrameGrid center: {center}");
        GD.Print($"Controller pos: {Position}");
        GD.Print($"Pivot rot: {_pivot.RotationDegrees}");
        GD.Print($"Camera local pos: {_camera.Position}");
        GD.Print($"Camera global pos: {_camera.GlobalPosition}");
        GD.Print($"Camera current: {_camera.Current}");
    }

    // ── Input ────────────────────────────────────────────────────────────────
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Right)
                _dragging = mb.Pressed;

            if (mb.ButtonIndex == MouseButton.WheelUp)
                _zoomTarget = Mathf.Clamp(_zoomTarget - ZoomSpeed, MinZoom, MaxZoom);

            if (mb.ButtonIndex == MouseButton.WheelDown)
                _zoomTarget = Mathf.Clamp(_zoomTarget + ZoomSpeed, MinZoom, MaxZoom);

            if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
                _cardDropHandler?.TryDropCardOnTile();
        }

        if (@event is InputEventMouseMotion motion)
            _mouseDelta = motion.Relative;
    }

    // ── Process ──────────────────────────────────────────────────────────────
    public override void _Process(double delta)
    {
        if (!EnsureCameraNodes()) return;

        float dt = (float)delta;

        HandlePan(dt);
        HandleRotation(dt);
        HandleZoom(dt);

        _mouseDelta  = Vector2.Zero;
        _lastMousePos = GetViewport().GetMousePosition();
    }

    // ── Pan ───────────────────────────────────────────────────────────────────
    private void HandlePan(float delta)
    {
        Vector3 forward = -_pivot.GlobalTransform.Basis.Z;
        forward.Y = 0;
        forward   = forward.Normalized();

        Vector3 right = _pivot.GlobalTransform.Basis.X;
        right.Y = 0;
        right   = right.Normalized();

        Vector3 inputDir = Vector3.Zero;

        // Keyboard
        if (Input.IsActionPressed("ui_up"))    inputDir += forward;
        if (Input.IsActionPressed("ui_down"))  inputDir -= forward;
        if (Input.IsActionPressed("ui_right")) inputDir += right;
        if (Input.IsActionPressed("ui_left"))  inputDir -= right;

        // Edge scroll — only when mouse is not near the bottom (card hand area)
        Vector2 mousePos    = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        bool    inCardArea  = mousePos.Y > viewportSize.Y * 0.75f;

        if (!inCardArea)
        {
            if (mousePos.X < EdgeScrollMargin)                    inputDir -= right;
            if (mousePos.X > viewportSize.X - EdgeScrollMargin)  inputDir += right;
            if (mousePos.Y < EdgeScrollMargin)                    inputDir += forward;
            if (mousePos.Y > viewportSize.Y - EdgeScrollMargin)  inputDir -= forward;
        }

        if (inputDir != Vector3.Zero)
            inputDir = inputDir.Normalized();

        _desiredPosition += inputDir * MoveSpeed * delta;

        // Right-click drag pan
        if (_dragging)
        {
            Vector2 dm = GetViewport().GetMousePosition() - _lastMousePos;
            _desiredPosition += (-right   * dm.X
                               + forward  * dm.Y) * DragSpeed * delta;
        }

        // Clamp to arena bounds
        _desiredPosition.X = Mathf.Clamp(_desiredPosition.X,
            _boundsMin.X - BoundsPad, _boundsMax.X + BoundsPad);
        _desiredPosition.Z = Mathf.Clamp(_desiredPosition.Z,
            _boundsMin.Z - BoundsPad, _boundsMax.Z + BoundsPad);

        // Smooth lerp toward desired
        Position = Position.Lerp(_desiredPosition, MoveLerpSpeed * delta);
    }

    // ── Rotation ─────────────────────────────────────────────────────────────
    private void HandleRotation(float delta)
    {
        if (Input.IsActionPressed("rotate_left"))
            _yaw -= RotationSpeed * MouseToKeyboardRotationRatio * delta;
        if (Input.IsActionPressed("rotate_right"))
            _yaw += RotationSpeed * MouseToKeyboardRotationRatio * delta;

        if (Input.IsMouseButtonPressed(MouseButton.Middle))
        {
            _yaw   -= _mouseDelta.X * RotationSpeed;
            _pitch -= _mouseDelta.Y * RotationSpeed;
            _pitch  = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        }

        _pivot.RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }

    // ── Zoom ─────────────────────────────────────────────────────────────────
    private void HandleZoom(float delta)
    {
        Vector3 camPos = _camera.Position;
        camPos.Z = Mathf.Lerp(camPos.Z, _zoomTarget, ZoomLerpSpeed * delta);
        _camera.Position = camPos;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private bool EnsureCameraNodes()
    {
        if (_pivot == null)
            _pivot = GetNodeOrNull<Node3D>("CameraPivot");

        if (_camera == null && _pivot != null)
            _camera = _pivot.GetNodeOrNull<Camera3D>("Camera3D");

        if (_pivot == null || _camera == null) return false;
        return true;
    }
}