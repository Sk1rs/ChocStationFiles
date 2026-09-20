using System.Numerics;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunrise.Sanity;

/// <summary>
/// Marks an entity as being in "psychic crisis" - sanity hit 0%. The entity lies down and
/// can't move or stand back up on their own; unlike a normal HP crit, they take no damage
/// and their health doesn't matter. Only removed by SanityTreatmentSystem when a Chief
/// Medical Officer or Psychologist successfully completes a treatment session on them.
/// Networked (rather than living alongside the hidden SanityComponent) because - like a
/// normal HP crit - collapsing and shaking is meant to be visible to everyone, not just the
/// entity itself.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PsychicCritComponent : Component
{
    /// <summary>
    /// Client-side bookkeeping for the sprite-shake animation - the sprite offset before the
    /// animation started, so it can be restored exactly on exit.
    /// </summary>
    public Vector2 StartOffset = Vector2.Zero;

    /// <summary>
    /// Client-side bookkeeping for the sprite-shake animation - avoids jittering into the
    /// same quadrant twice in a row.
    /// </summary>
    public Vector2 LastJitter;

    /// <summary>
    /// Server-side bookkeeping: each stomach organ's special-digestion settings from before
    /// entering crisis, so PsychicCritSystem can restrict digestion to pills (haloperidol
    /// included) for the duration and restore the originals exactly on exit.
    /// </summary>
    public List<(EntityUid Stomach, EntityWhitelist? PreviousWhitelist, bool PreviousExclusive)>? RestrictedStomachs;
}
