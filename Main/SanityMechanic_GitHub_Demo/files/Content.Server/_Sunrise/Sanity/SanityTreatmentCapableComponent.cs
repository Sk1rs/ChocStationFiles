namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Marker granted via job special (Chief Medical Officer, Psychologist) that grants the
/// sanity treatment action on spawn. Senior Physician does NOT get this - they can only
/// examine sanity, not treat it.
/// </summary>
[RegisterComponent]
public sealed partial class SanityTreatmentCapableComponent : Component
{
}
