using Godot;

/// <summary>
/// Global pause menu controller. Listens for ESC, instantiates the pause overlay
/// over whatever scene is currently active, and provides context-aware menu options.
/// 
/// SETUP:
///   1. Save as res://Scripts/Systems/PauseManager.cs
///   2. Project Settings → Globals → Autoload, name it "PauseManager"
///   3. Make sure the autoload's "Process Mode" is set to "Always" so ESC still
///      works while the tree is paused (this script sets it in _Ready, but the
///      autoload setting is the safety net).
/// 
/// USAGE FROM ANY SCREEN:
///   PauseManager.Instance.OpenPauseMenu();   // e.g. from a settings button
///   PauseManager.Instance.ClosePauseMenu();
/// 
/// CONTEXT (see PauseContext enum below):
///   The manager auto-detects context based on the current scene path. Override
///   it explicitly if needed:
///     PauseManager.Instance.SetContext(PauseContext.InCombat);
/// </summary>
public partial class PauseManager : Node
{
    public static PauseManager Instance { get; private set; }

    // Path to the pause menu scene. Adjust if you put it elsewhere.
    [Export] public string PauseMenuScenePath = "res://Scenes/UI/PauseMenu.tscn";

    public enum PauseContext
    {
        MainMenu,    // Campus / class select / pre-run — no "forfeit run" option
        Overworld,   // Mid-run, on the overworld map — show "Return to Campus" (forfeit)
        InCombat,    // Mid-combat — show "Forfeit Combat" (returns to overworld)
    }

    public PauseContext CurrentContext { get; private set; } = PauseContext.MainMenu;

    private PauseMenu _activeMenu;
    private PackedScene _pauseMenuScene;

    public bool IsPaused => _activeMenu != null && IsInstanceValid(_activeMenu);

    public override void _Ready()
    {
        Instance = this;
        // Stay processing while the rest of the tree is paused.
        ProcessMode = ProcessModeEnum.Always;

        _pauseMenuScene = GD.Load<PackedScene>(PauseMenuScenePath);
        if (_pauseMenuScene == null)
            GD.PrintErr($"[PauseManager] Could not load pause menu scene at: {PauseMenuScenePath}");

        // Listen for scene changes so we can refresh context automatically.
        GetTree().NodeAdded += OnNodeAdded;
    }

    private void OnNodeAdded(Node n)
    {
        // Cheap heuristic: when the root scene is replaced, infer context from its name/path.
        if (n.GetParent() == GetTree().Root && n != this)
            CallDeferred(nameof(InferContextFromCurrentScene));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // ESC toggles the pause menu globally.
        if (@event is InputEventKey k && k.Pressed && !k.Echo
            && k.Keycode == Key.Escape)
        {
            if (IsPaused) ClosePauseMenu();
            else          OpenPauseMenu();

            GetViewport().SetInputAsHandled();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Public API
    // ════════════════════════════════════════════════════════════════════════

    public void OpenPauseMenu()
    {
        if (IsPaused) return;
        if (_pauseMenuScene == null) return;

        InferContextFromCurrentScene();

        _activeMenu = _pauseMenuScene.Instantiate<PauseMenu>();
        // Add to root so it overlays everything, including modal popups.
        GetTree().Root.AddChild(_activeMenu);
        _activeMenu.Configure(CurrentContext);

        // Pause the rest of the tree. PauseMenu itself uses ProcessMode.Always.
        GetTree().Paused = true;
    }

    public void ClosePauseMenu()
    {
        if (_activeMenu != null && IsInstanceValid(_activeMenu))
        {
            _activeMenu.QueueFree();
            _activeMenu = null;
        }
        GetTree().Paused = false;
    }

    public void SetContext(PauseContext ctx) => CurrentContext = ctx;

    // ════════════════════════════════════════════════════════════════════════
    //  Context inference — guesses from the current scene's file path.
    //  Adjust the path matches if you reorganize your scenes.
    // ════════════════════════════════════════════════════════════════════════

    private void InferContextFromCurrentScene()
    {
        var current = GetTree().CurrentScene;
        if (current == null) return;

        string path = current.SceneFilePath ?? "";
        string lower = path.ToLower();

        if (lower.Contains("/combat/") || lower.Contains("gamerunner") || lower.Contains("/combatscene"))
            CurrentContext = PauseContext.InCombat;
        else if (lower.Contains("/overworld") || lower.Contains("overworldscene"))
            CurrentContext = PauseContext.Overworld;
        else
            CurrentContext = PauseContext.MainMenu;
    }
}
