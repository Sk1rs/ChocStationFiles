namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Tracks the hallucination clones currently haunting this player, and when the next one
/// is due to spawn. Also used as a subscription target so this player's eye picks up their
/// assigned visibility slot bit (see HallucinationSystem's slot allocator) - this is what
/// makes their clones visible only to them, not to other hallucinating players too.
/// </summary>
[RegisterComponent]
public sealed partial class HallucinatingComponent : Component
{
    [DataField]
    public List<EntityUid> ActiveHallucinations = new();

    [DataField]
    public TimeSpan NextSpawnTime;

    /// <summary>
    /// This player's exclusive VisibilityFlags bit, assigned from HallucinationSystem's slot
    /// pool when they start hallucinating and released back to the pool once they recover.
    /// </summary>
    public int VisibilitySlotBit;
}
