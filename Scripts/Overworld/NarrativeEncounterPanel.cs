using Godot;
using System;

/// <summary>
/// Modal panel for narrative encounters, displayed over the overworld.
/// Shows flavor text, choice buttons, then result text on choice.
/// Fires OnCompleted callback after the player dismisses the result.
/// </summary>
public partial class NarrativeEncounterPanel : Control
{
    public Action<EncounterChoice> OnCompleted;

    private Panel _backdrop;
    private Panel _panel;
    private Label _titleLabel;
    private Label _bodyLabel;
    private VBoxContainer _choiceContainer;
    private Panel _resultPanel;
    private Label _resultLabel;
    private Button _continueButton;

    private NarrativeEncounterData _encounter;
    private EncounterChoice _chosenResult;

    public override void _Ready()
    {
        // Cover the full viewport
        AnchorRight = 1f;
        AnchorBottom = 1f;
        MouseFilter = MouseFilterEnum.Stop; // block input to overworld behind us

        // Dim backdrop
        _backdrop = new Panel
        {
            AnchorRight = 1f, AnchorBottom = 1f,
            MouseFilter = MouseFilterEnum.Stop,
        };
        var backdropStyle = new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.65f)
        };
        _backdrop.AddThemeStyleboxOverride("panel", backdropStyle);
        AddChild(_backdrop);

        // Main encounter panel
        _panel = new Panel
        {
            AnchorLeft = 0.5f, AnchorTop = 0.5f,
            AnchorRight = 0.5f, AnchorBottom = 0.5f,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            OffsetLeft = -320, OffsetTop = -280,
            OffsetRight = 320, OffsetBottom = 280,
        };
        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.12f, 0.10f, 0.18f),
            BorderColor = new Color(0.45f, 0.38f, 0.65f),
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
        };
        _panel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(_panel);

        var layout = new VBoxContainer
        {
            AnchorRight = 1f, AnchorBottom = 1f,
            OffsetLeft = 24, OffsetTop = 24,
            OffsetRight = -24, OffsetBottom = -24,
        };
        layout.AddThemeConstantOverride("separation", 14);
        _panel.AddChild(layout);

        // Title
        _titleLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 22);
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 1.0f));
        layout.AddChild(_titleLabel);

        layout.AddChild(new HSeparator());

        // Body
        _bodyLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        _bodyLabel.AddThemeFontSizeOverride("font_size", 16);
        _bodyLabel.AddThemeColorOverride("font_color", new Color(0.88f, 0.85f, 0.90f));
        layout.AddChild(_bodyLabel);

        layout.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

        // Choices
        _choiceContainer = new VBoxContainer();
        _choiceContainer.AddThemeConstantOverride("separation", 8);
        layout.AddChild(_choiceContainer);

        // Result panel (shown after choice)
        _resultPanel = new Panel { Visible = false };
        var resultStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.18f, 0.15f, 0.25f),
            BorderColor = new Color(0.55f, 0.45f, 0.75f),
            BorderWidthTop = 1, BorderWidthBottom = 1,
            BorderWidthLeft = 1, BorderWidthRight = 1,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
            ContentMarginLeft = 12, ContentMarginRight = 12,
            ContentMarginTop = 10, ContentMarginBottom = 10,
        };
        _resultPanel.AddThemeStyleboxOverride("panel", resultStyle);
        _resultPanel.CustomMinimumSize = new Vector2(0, 70);
        layout.AddChild(_resultPanel);

        _resultLabel = new Label
        {
            AnchorRight = 1f, AnchorBottom = 1f,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _resultLabel.AddThemeFontSizeOverride("font_size", 15);
        _resultLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.90f, 0.75f));
        _resultPanel.AddChild(_resultLabel);

        // Continue button
        _continueButton = new Button
        {
            Text = "Continue",
            Visible = false,
            CustomMinimumSize = new Vector2(160, 40),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        _continueButton.AddThemeFontSizeOverride("font_size", 16);
        _continueButton.Pressed += OnContinuePressed;
        layout.AddChild(_continueButton);
    }

    public void ShowEncounter(NarrativeEncounterData encounter)
    {
        _encounter = encounter;
        _chosenResult = null;

        _titleLabel.Text = encounter.Title;
        _bodyLabel.Text = encounter.Body;

        // Clear old buttons
        foreach (var child in _choiceContainer.GetChildren())
            child.QueueFree();

        // Build choice buttons
        foreach (var choice in encounter.Choices)
        {
            var btn = new Button
            {
                Text = choice.Label,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(0, 44),
            };
            btn.AddThemeFontSizeOverride("font_size", 15);
            var capturedChoice = choice;
            btn.Pressed += () => OnChoicePressed(capturedChoice);
            _choiceContainer.AddChild(btn);
        }

        _resultPanel.Visible = false;
        _continueButton.Visible = false;
        Visible = true;
    }

    private void OnChoicePressed(EncounterChoice choice)
    {
        _chosenResult = choice;

        // Disable choice buttons
        foreach (var child in _choiceContainer.GetChildren())
            if (child is Button b) b.Disabled = true;

        // Build result text + outcome summary
        string resultText = choice.ResultText;
        var outcomes = new System.Collections.Generic.List<string>();
        if (choice.GoldDelta > 0)  outcomes.Add($"+{choice.GoldDelta} gold");
        if (choice.GoldDelta < 0)  outcomes.Add($"{choice.GoldDelta} gold");
        if (choice.HPDelta > 0)    outcomes.Add($"+{choice.HPDelta} HP");
        if (choice.HPDelta < 0)    outcomes.Add($"{choice.HPDelta} HP");
        if (choice.StepDelta > 0)  outcomes.Add($"+{choice.StepDelta} steps");
        if (choice.StepDelta < 0)  outcomes.Add($"{choice.StepDelta} steps");

        if (outcomes.Count > 0)
            resultText += $"\n\n{string.Join("  |  ", outcomes)}";

        _resultLabel.Text = resultText;
        _resultPanel.Visible = true;
        _continueButton.Visible = true;
    }

    private void OnContinuePressed()
    {
        Visible = false;
        OnCompleted?.Invoke(_chosenResult);
    }
}