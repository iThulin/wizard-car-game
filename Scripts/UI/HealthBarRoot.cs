using Godot;

public partial class HealthBarRoot : Node3D
{
    [Export] public NodePath HealthFillPath = "HealthFill";
    [Export] public NodePath ManaFillPath = "ManaFill";
    [Export] public NodePath HealthTextPath = "HealthText";
    [Export] public NodePath ManaTextPath = "ManaText";

    [Export] public float FullBarWidth = 1.6f;

    private Node3D _healthFill;
    private Node3D _manaFill;
    private Label3D _healthText;
    private Label3D _manaText;
    private Camera3D _camera;

    public override void _Ready()
    {
        _healthFill = GetNodeOrNull<Node3D>(HealthFillPath);
        _manaFill = GetNodeOrNull<Node3D>(ManaFillPath);
        _healthText = GetNodeOrNull<Label3D>(HealthTextPath);
        _manaText = GetNodeOrNull<Label3D>(ManaTextPath);
        _camera = GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        if (_camera != null)
            LookAt(_camera.GlobalPosition, Vector3.Up, true);
    }

    public void SetHealth(int current, int max)
    {
        if (_healthFill != null)
        {
            float pct = max <= 0 ? 0f : Mathf.Clamp((float)current / max, 0f, 1f);
            _healthFill.Scale = new Vector3(pct, 1f, 1f);

            float offset = -(FullBarWidth * (1f - pct)) * 0.5f;
            _healthFill.Position = new Vector3(offset, _healthFill.Position.Y, _healthFill.Position.Z);
        }

        if (_healthText != null)
            _healthText.Text = $"{current}/{max}";
    }

    public void SetMana(int current, int max)
    {
        if (_manaFill != null)
        {
            float pct = max <= 0 ? 0f : Mathf.Clamp((float)current / max, 0f, 1f);
            _manaFill.Scale = new Vector3(pct, 1f, 1f);

            float offset = -(FullBarWidth * (1f - pct)) * 0.5f;
            _manaFill.Position = new Vector3(offset, _manaFill.Position.Y, _manaFill.Position.Z);
        }

        if (_manaText != null)
            _manaText.Text = $"{current}/{max}";
    }
}