using Godot;

/// <summary>
/// Data transfer object passed from CombatManager to CombatUI.ShowEnemyIntel()
/// during the deployment phase, before enemy units are actually spawned.
/// Lives in its own file so both CombatManager and CombatUI can reference it
/// without a circular dependency.
/// </summary>
public struct EnemyIntelEntry
{
    public string ThreatLabel; // "Soldier", "Brute", "Defender", "Skirmisher"
    public int MaxHealth;
    public int BaseSpeed;
    public int Armor;
    public Color BodyColor;
}
