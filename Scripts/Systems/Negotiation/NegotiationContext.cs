/// <summary>
/// Static context carrier for negotiation encounters.
/// Set by RunManager before swapping to the negotiation scene.
/// Read by NegotiationScene to know what encounter to run.
/// Results written back here and read by RunManager on return.
/// </summary>
public static class NegotiationContext
{
    // ── Input (set before scene swap) ───────────────────────────────────
    public static string EncounterId = "";
    public static string HexCoordKey = "";          // "q,r" for the triggering hex

    // ── Output (set by NegotiationScene on completion) ──────────────────
    public static bool HasResult = false;
    public static bool DealAccepted = false;
    public static int GoldDelta = 0;
    public static int ReputationDelta = 0;
    public static string FactionId = "";

    public static void SetResult(bool accepted, int gold, int rep, string factionId)
    {
        HasResult = true;
        DealAccepted = accepted;
        GoldDelta = gold;
        ReputationDelta = rep;
        FactionId = factionId;
    }

    public static void Clear()
    {
        HasResult = false;
        DealAccepted = false;
        GoldDelta = 0;
        ReputationDelta = 0;
        FactionId = "";
        EncounterId = "";
        HexCoordKey = "";
    }
}