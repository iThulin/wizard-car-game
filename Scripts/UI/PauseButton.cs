using Godot;

/// <summary>
/// Drop-in pause/settings button for any screen. Anchors itself to the top-right corner.
/// Pressing it (or pressing ESC) opens the pause menu.
/// 
/// USAGE:
///   Drop one of these onto any scene root (campus, overworld, combat, card library).
///   That's it — no wiring, no script edits needed.
/// 
/// You can also instance the PauseButton.tscn directly in the editor.
/// </summary>
public partial class PauseButton : Button
{
    [Export] public bool AnchorTopRight = true;
    [Export] public Vector2 OffsetFromCorner = new Vector2(-12, 12);

    public override void _Ready()
    {
        // Default appearance — small, unobtrusive corner button.
        Text = "☰";
        if (CustomMinimumSize == Vector2.Zero)
            CustomMinimumSize = new Vector2(40, 40);
        AddThemeFontSizeOverride("font_size", UITheme.FontSizePauseButton);
        FocusMode = FocusModeEnum.None;
        ProcessMode = ProcessModeEnum.Always;

        if (AnchorTopRight)
        {
            // Anchor to top-right corner of parent.
            AnchorLeft = 1f;
            AnchorTop = 0f;
            AnchorRight = 1f;
            AnchorBottom = 0f;
            // Offsets are measured from the anchor point.
            OffsetLeft = OffsetFromCorner.X - CustomMinimumSize.X;
            OffsetTop = OffsetFromCorner.Y;
            OffsetRight = OffsetFromCorner.X;
            OffsetBottom = OffsetFromCorner.Y + CustomMinimumSize.Y;
        }

        Pressed += OpenPauseMenu;
    }

    private void OpenPauseMenu()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OpenPauseMenu();
        else
            GD.PrintErr("[PauseButton] PauseManager autoload not found.");
    }
}
