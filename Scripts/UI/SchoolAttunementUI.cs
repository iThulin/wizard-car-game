using Godot;
using System;
using System.Collections.Generic;

// ============================================================
// SchoolAttunementUI — Swappable HUD for school class mechanics
//
// Sits in CombatUI. When the player selects a different wizard,
// call ShowForUnit(unit) and it rebuilds to show that wizard's
// school mechanic (Elementalist attunement, Necromancer corpse
// count, etc.) or hides if the unit has no mechanic.
//
// Currently implements: Elementalist (4-element charge bars)
// Other schools: stub "Coming soon" until implemented
// ============================================================

public partial class SchoolAttunementUI : PanelContainer
{
	// ── State ───────────────────────────────────────────────────────
	private Unit _currentUnit;
	private ElementalAttunement _boundAttunement;
	private CardSchool _currentSchool = CardSchool.Generic;

	// ── UI refs (rebuilt on school switch) ───────────────────────────
	private VBoxContainer _container;
	private Label _titleLabel;
	private Label _stubLabel; // for unimplemented schools

	// Elementalist-specific
	private readonly Dictionary<ElementTag, ElementBar> _elementBars = new();

	// ── Colors ──────────────────────────────────────────────────────
	private static readonly Dictionary<ElementTag, Color> ElementColors = new()
	{
		{ ElementTag.Fire,  new Color(1.0f, 0.35f, 0.1f) },
		{ ElementTag.Ice,   new Color(0.4f, 0.75f, 1.0f) },
		{ ElementTag.Storm, new Color(0.95f, 0.9f, 0.2f) },
		{ ElementTag.Earth, new Color(0.6f, 0.45f, 0.25f) }
	};

	private static readonly Dictionary<ElementTag, string> ElementNames = new()
	{
		{ ElementTag.Fire,  "Fire" },
		{ ElementTag.Ice,   "Ice" },
		{ ElementTag.Storm, "Storm" },
		{ ElementTag.Earth, "Earth" }
	};

	private static readonly string[] TierLabels = { "", "+1 dmg", "imbue", "enhanced", "BURST!" };

	// Opposing pair labels for the UI
	private static readonly Dictionary<ElementTag, ElementTag> OppositionDisplay = new()
	{
		{ ElementTag.Fire,  ElementTag.Ice },
		{ ElementTag.Ice,   ElementTag.Fire },
		{ ElementTag.Storm, ElementTag.Earth },
		{ ElementTag.Earth, ElementTag.Storm }
	};

	public override void _Ready()
	{
		// Panel styling
		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.08f, 0.08f, 0.12f, 0.85f),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
			ContentMarginLeft = 8,
			ContentMarginRight = 8,
			ContentMarginTop = 6,
			ContentMarginBottom = 6
		};
		AddThemeStyleboxOverride("panel", style);
		CustomMinimumSize = new Vector2(210, 0);

		_container = new VBoxContainer();
		_container.AddThemeConstantOverride("separation", 3);
		AddChild(_container);

		// Start hidden
		Visible = false;
	}

	// ════════════════════════════════════════════════════════════════
	// PUBLIC API — called by GameRunner on unit selection change
	// ════════════════════════════════════════════════════════════════

	/// <summary>
	/// Show the attunement UI for this unit. Pass null to hide.
	/// </summary>
	public void ShowForUnit(Unit unit)
	{
		// Unbind old attunement events
		UnbindAttunement();

		_currentUnit = unit;

		if (unit == null || unit.Attunement == null)
		{
			Visible = false;
			return;
		}

		var school = unit.School;

		// Only rebuild if the school changed
		if (school != _currentSchool)
		{
			_currentSchool = school;
			RebuildForSchool(school);
		}

		// Bind to this unit's attunement
		if (school == CardSchool.Elementalist && unit.Attunement is ElementalAttunement elemAtt)
		{
			BindElementalist(elemAtt);
		}

		Visible = true;
	}

	/// <summary>
	/// Force refresh all bars (call after burst, cast, decay).
	/// </summary>
	public void Refresh()
	{
		if (_boundAttunement != null)
			RefreshElementalistBars();
	}

	// ════════════════════════════════════════════════════════════════
	// REBUILD — clears everything and creates layout for a school
	// ════════════════════════════════════════════════════════════════

	private void RebuildForSchool(CardSchool school)
	{
		// Clear all children
		foreach (Node child in _container.GetChildren())
			child.QueueFree();
		_elementBars.Clear();
		_stubLabel = null;

		// Title
		_titleLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Center
		};
		_titleLabel.AddThemeFontSizeOverride("font_size", 12);
		_titleLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.85f));
		_container.AddChild(_titleLabel);

		switch (school)
		{
			case CardSchool.Elementalist:
				_titleLabel.Text = "Elemental Attunement";
				BuildElementalistUI();
				break;

			// ── Future schools ──────────────────────────────────────
			case CardSchool.Necromancer:
				_titleLabel.Text = "Necromantic Binding";
				BuildStubUI("Corpse & soul mechanics coming soon.");
				break;

			case CardSchool.Arcanist:
				_titleLabel.Text = "Arcane Focus";
				BuildStubUI("Spell amplification coming soon.");
				break;

			case CardSchool.Enchanter:
				_titleLabel.Text = "Enchantment Weave";
				BuildStubUI("Buff/debuff stacking coming soon.");
				break;

			case CardSchool.Tinker:
				_titleLabel.Text = "Contraption Assembly";
				BuildStubUI("Trap & turret grid coming soon.");
				break;

			default:
				_titleLabel.Text = "Class Mechanic";
				BuildStubUI("No special mechanic for this school.");
				break;
		}
	}

	// ════════════════════════════════════════════════════════════════
	// ELEMENTALIST — 4 charge bars with opposition indicators
	// ════════════════════════════════════════════════════════════════

	private void BuildElementalistUI()
	{
		// Pair labels
		var pairLabel1 = new Label
		{
			Text = "Fire ←→ Ice",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		pairLabel1.AddThemeFontSizeOverride("font_size", 9);
		pairLabel1.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.55f));
		_container.AddChild(pairLabel1);

		CreateElementBar(ElementTag.Fire);
		CreateElementBar(ElementTag.Ice);

		// Separator
		var sep = new HSeparator();
		sep.CustomMinimumSize = new Vector2(0, 4);
		_container.AddChild(sep);

		var pairLabel2 = new Label
		{
			Text = "Storm ←→ Earth",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		pairLabel2.AddThemeFontSizeOverride("font_size", 9);
		pairLabel2.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.55f));
		_container.AddChild(pairLabel2);

		CreateElementBar(ElementTag.Storm);
		CreateElementBar(ElementTag.Earth);
	}

	private void CreateElementBar(ElementTag element)
	{
		var bar = new ElementBar { Element = element, BaseColor = ElementColors[element] };

		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 4);
		_container.AddChild(row);

		// Element name
		bar.NameLabel = new Label
		{
			Text = ElementNames[element],
			CustomMinimumSize = new Vector2(40, 0)
		};
		bar.NameLabel.AddThemeFontSizeOverride("font_size", 11);
		bar.NameLabel.AddThemeColorOverride("font_color", ElementColors[element]);
		row.AddChild(bar.NameLabel);

		// Pips
		var pipBox = new HBoxContainer();
		pipBox.AddThemeConstantOverride("separation", 2);
		row.AddChild(pipBox);

		bar.Pips = new ColorRect[4];
		for (int i = 0; i < 4; i++)
		{
			var pip = new ColorRect
			{
				CustomMinimumSize = new Vector2(18, 12),
				Color = new Color(0.15f, 0.15f, 0.2f)
			};
			pipBox.AddChild(pip);
			bar.Pips[i] = pip;
		}

		// Tier label
		bar.TierLabel = new Label
		{
			Text = "",
			CustomMinimumSize = new Vector2(60, 0)
		};
		bar.TierLabel.AddThemeFontSizeOverride("font_size", 10);
		bar.TierLabel.AddThemeColorOverride("font_color", ElementColors[element]);
		row.AddChild(bar.TierLabel);

		_elementBars[element] = bar;
	}

	private void BindElementalist(ElementalAttunement att)
	{
		_boundAttunement = att;
		att.OnChargeChanged += OnElementChargeChanged;
		att.OnBurstTriggered += OnElementBurst;
		RefreshElementalistBars();
	}

	private void UnbindAttunement()
	{
		if (_boundAttunement != null)
		{
			_boundAttunement.OnChargeChanged -= OnElementChargeChanged;
			_boundAttunement.OnBurstTriggered -= OnElementBurst;
			_boundAttunement = null;
		}
	}

	private void OnElementChargeChanged(ElementTag element, int newValue)
	{
		if (_elementBars.TryGetValue(element, out var bar))
			UpdateElementBar(bar, newValue);
	}

	private void OnElementBurst(ElementTag element)
	{
		if (!_elementBars.TryGetValue(element, out var bar)) return;

		// Flash white
		for (int i = 0; i < 4; i++)
			bar.Pips[i].Color = new Color(1, 1, 1);
		bar.TierLabel.Text = "BURST!";
		bar.TierLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));

		// Tween back
		var tween = CreateTween();
		tween.TweenInterval(0.5f);
		tween.TweenCallback(Callable.From(() =>
		{
			if (_boundAttunement != null)
				UpdateElementBar(bar, _boundAttunement.Charges[element]);
		}));
	}

	private void RefreshElementalistBars()
	{
		if (_boundAttunement == null) return;
		foreach (var kvp in _elementBars)
			UpdateElementBar(kvp.Value, _boundAttunement.Charges[kvp.Key]);
	}

	private void UpdateElementBar(ElementBar bar, int charges)
	{
		charges = Math.Clamp(charges, 0, 4);
		Color empty = new Color(0.15f, 0.15f, 0.2f);

		float brightness = charges >= 3 ? 1.3f : charges >= 2 ? 1.1f : 1.0f;
		Color bright = new Color(
			Math.Min(1f, bar.BaseColor.R * brightness),
			Math.Min(1f, bar.BaseColor.G * brightness),
			Math.Min(1f, bar.BaseColor.B * brightness)
		);

		for (int i = 0; i < 4; i++)
			bar.Pips[i].Color = i < charges ? bright : empty;

		int tierIdx = charges >= 4 ? 4 : charges >= 3 ? 3 : charges >= 2 ? 2 : charges >= 1 ? 1 : 0;
		bar.TierLabel.Text = TierLabels[tierIdx];
		bar.TierLabel.AddThemeColorOverride("font_color",
			charges >= 3 ? new Color(1, 1, 1) : bar.BaseColor);
	}

	// ════════════════════════════════════════════════════════════════
	// STUB — placeholder for unimplemented schools
	// ════════════════════════════════════════════════════════════════

	private void BuildStubUI(string message)
	{
		_stubLabel = new Label
		{
			Text = message,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		_stubLabel.AddThemeFontSizeOverride("font_size", 10);
		_stubLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.55f));
		_container.AddChild(_stubLabel);
	}

	// ════════════════════════════════════════════════════════════════
	// Internal data
	// ════════════════════════════════════════════════════════════════

	private class ElementBar
	{
		public ElementTag Element;
		public Color BaseColor;
		public Label NameLabel;
		public ColorRect[] Pips;
		public Label TierLabel;
	}
}
