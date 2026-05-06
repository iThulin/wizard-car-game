using Godot;

/// <summary>
/// Pause menu overlay controller. Instantiated by PauseManager when ESC is pressed.
/// 
/// Auto-shows/hides buttons based on PauseContext (main menu, overworld, in combat).
/// Opens the Settings menu and Card Library as inline overlays inside the pause menu
/// panel — no scene changes — so the underlying game state isn't lost.
/// </summary>
public partial class PauseMenu : Control
{
    [Export] public string SettingsScenePath    = "res://Scenes/UI/SettingsMenu.tscn";
    [Export] public string CardLibraryScenePath = "res://Scenes/UI/CardLibrary.tscn";
    [Export] public string MainMenuScenePath    = "res://Scenes/Campus/CampusScene.tscn";
    [Export] public string OverworldScenePath   = "res://Scenes/Overworld/OverworldScene.tscn";

    // ── Buttons ─────────────────────────────────────────────────────────────
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _cardLibraryButton;
    private Button _returnToCampusButton; // only in Overworld / InCombat
    private Button _forfeitCombatButton;  // only in InCombat
    private Button _quitButton;

    // ── Containers ──────────────────────────────────────────────────────────
    private Control _menuContainer;       // The center menu panel
    private Control _overlayHost;         // Hosts settings/library inside pause
    private ColorRect _backdrop;

    // Currently-open inline panel (settings or library), if any.
    private Node _currentInlinePanel;

    public override void _Ready()
    {
        // Stay alive while the tree is paused.
        ProcessMode = ProcessModeEnum.Always;

        _backdrop          = GetNode<ColorRect>("Backdrop");
        _menuContainer     = GetNode<Control>("MenuContainer");
        _overlayHost       = GetNode<Control>("OverlayHost");

        _resumeButton         = GetNode<Button>("MenuContainer/Margin/VBox/ResumeButton");
        _settingsButton       = GetNode<Button>("MenuContainer/Margin/VBox/SettingsButton");
        _cardLibraryButton    = GetNode<Button>("MenuContainer/Margin/VBox/CardLibraryButton");
        _returnToCampusButton = GetNode<Button>("MenuContainer/Margin/VBox/ReturnToCampusButton");
        _forfeitCombatButton  = GetNode<Button>("MenuContainer/Margin/VBox/ForfeitCombatButton");
        _quitButton           = GetNode<Button>("MenuContainer/Margin/VBox/QuitButton");

        _resumeButton.Pressed         += OnResumePressed;
        _settingsButton.Pressed       += OnSettingsPressed;
        _cardLibraryButton.Pressed    += OnCardLibraryPressed;
        _returnToCampusButton.Pressed += OnReturnToCampusPressed;
        _forfeitCombatButton.Pressed  += OnForfeitCombatPressed;
        _quitButton.Pressed           += OnQuitPressed;

        // Tapping the backdrop dismisses the pause menu (only when no inline panel is open).
        _backdrop.GuiInput += OnBackdropInput;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Configuration — called by PauseManager right after instancing
    // ════════════════════════════════════════════════════════════════════════

    public void Configure(PauseManager.PauseContext ctx)
    {
        bool inMainMenu  = ctx == PauseManager.PauseContext.MainMenu;
        bool inOverworld = ctx == PauseManager.PauseContext.Overworld;
        bool inCombat    = ctx == PauseManager.PauseContext.InCombat;

        // Card library is useful everywhere except the (already-open) library scene itself.
        _cardLibraryButton.Visible = true;

        // "Return to Campus" = forfeit a run. Only meaningful mid-run.
        _returnToCampusButton.Visible = inOverworld;

        // "Forfeit Combat" returns to the overworld with a defeat.
        _forfeitCombatButton.Visible = inCombat;

        // Quit is always available.
        _quitButton.Visible = true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Button handlers
    // ════════════════════════════════════════════════════════════════════════

    private void OnResumePressed()
    {
        PauseManager.Instance?.ClosePauseMenu();
    }

    private void OnSettingsPressed()
    {
        OpenInlineOverlay(SettingsScenePath, configureBackButton: settings =>
        {
            // Configure the settings menu's "Back" to close the inline overlay
            // instead of changing scenes.
            if (settings is SettingsMenu sm)
                sm.ReturnScenePath = ""; // empty = QueueFree on back press
        });
    }

    private void OnCardLibraryPressed()
    {
        OpenInlineOverlay(CardLibraryScenePath, configureBackButton: lib =>
        {
            // The card library uses a back button that calls ChangeSceneToFile.
            // Override its return path to be empty/null so we can intercept it
            // by hiding the inline overlay instead.
            // (We assume CardLibraryUi has the same "ReturnScenePath" export pattern.)
            var prop = lib.GetType().GetProperty("ReturnScenePath");
            if (prop != null && prop.CanWrite)
                prop.SetValue(lib, "__INLINE__"); // sentinel value; library can also be patched

            // Connect a "back" detection: poll for the BackButton and rewire it.
            CallDeferred(nameof(RewireCardLibraryBackButton), lib);
        });
    }

    private void RewireCardLibraryBackButton(Node libRoot)
    {
        if (libRoot == null || !IsInstanceValid(libRoot)) return;
        var backBtn = libRoot.FindChild("BackButton", recursive: true) as Button;
        if (backBtn == null) return;

        // Drop existing connections to ChangeSceneToFile by replacing with our own.
        // (We can't easily disconnect lambdas, so we just connect a higher-priority one.)
        backBtn.Pressed += CloseInlineOverlay;
    }

    private void OnReturnToCampusPressed()
    {
        // Forfeit the current run, go back to the campus scene.
        PauseManager.Instance?.ClosePauseMenu();
        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }

    private void OnForfeitCombatPressed()
    {
        // Return to the overworld as if combat were lost.
        // EncounterRouter.OnCombatFinished triggers the proper transition.
        if (EncounterRouter.Instance != null)
        {
            PauseManager.Instance?.ClosePauseMenu();
            EncounterRouter.Instance.OnCombatFinished(playerWon: false);
        }
        else
        {
            // Fallback: just go to the overworld scene.
            PauseManager.Instance?.ClosePauseMenu();
            GetTree().ChangeSceneToFile(OverworldScenePath);
        }
    }

    private void OnQuitPressed()
    {
        // Optional: hook in autosave here before quitting.
        // SaveManager.Save();
        GetTree().Quit();
    }

    private void OnBackdropInput(InputEvent @event)
    {
        // Click outside the menu = resume — but only if no inline panel is open.
        if (_currentInlinePanel != null) return;
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            OnResumePressed();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Inline overlay management (settings / card library inside the pause menu)
    // ════════════════════════════════════════════════════════════════════════

    private void OpenInlineOverlay(string scenePath, System.Action<Node> configureBackButton = null)
    {
        if (_currentInlinePanel != null) return;

        var packed = GD.Load<PackedScene>(scenePath);
        if (packed == null)
        {
            GD.PrintErr($"[PauseMenu] Could not load: {scenePath}");
            return;
        }

        var inst = packed.Instantiate();
        _currentInlinePanel = inst;

        // Make the inline panel fill the OverlayHost.
        if (inst is Control c)
        {
            c.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            c.ProcessMode = ProcessModeEnum.Always; // keep it interactive while paused
        }

        _overlayHost.AddChild(inst);
        configureBackButton?.Invoke(inst);

        // Hide the menu container while the overlay is showing.
        _menuContainer.Visible = false;

        // Ensure that if the overlay frees itself (e.g. settings menu's Back), we restore the menu.
        inst.TreeExited += OnInlinePanelClosed;
    }

    private void CloseInlineOverlay()
    {
        if (_currentInlinePanel != null && IsInstanceValid(_currentInlinePanel))
            _currentInlinePanel.QueueFree();
        // OnInlinePanelClosed will fire and restore the menu.
    }

    private void OnInlinePanelClosed()
    {
        _currentInlinePanel = null;
        if (IsInstanceValid(_menuContainer))
            _menuContainer.Visible = true;
    }
}
