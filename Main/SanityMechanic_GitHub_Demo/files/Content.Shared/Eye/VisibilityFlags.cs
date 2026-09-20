using Robust.Shared.Serialization;

namespace Content.Shared.Eye
{
    [Flags]
    [FlagsFor(typeof(VisibilityMaskLayer))]
    public enum VisibilityFlags : int
    {
        None = 0,
        Normal = 1 << 0,
        Ghost = 1 << 1, // Observers and revenants.
        Subfloor = 1 << 2, // Pipes, disposal chutes, cables etc. while hidden under tiles. Can be revealed with a t-ray.
        Abductor  = 1 << 2, // Starlight-abductor
        Admin = 1 << 3, // Reserved for admins in stealth mode and admin tools.
        // Sunrise - base of an 8-bit pool (this value through 1 << 11) used to isolate each
        // hallucinating player's clones from every other player, including other hallucinating
        // players - a single shared flag here would mean anyone currently hallucinating could
        // also see everyone else's hallucinations. See HallucinationSystem's slot allocator.
        HallucinationSlotBase = 1 << 4,
    }
}
