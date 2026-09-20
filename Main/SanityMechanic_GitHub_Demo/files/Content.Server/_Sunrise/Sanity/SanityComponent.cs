using Content.Shared._Sunrise.Sanity;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Hidden 0-100 sanity meter. 0 triggers a psychic crisis that only a Chief Medical
/// Officer or Psychologist can bring the player out of - see PsychicCritComponent.
/// Entities without this component (e.g. borgs) are immune to the mechanic entirely.
/// </summary>
[RegisterComponent]
public sealed partial class SanityComponent : Component
{
    [DataField]
    public float CurrentSanity = 100f;

    [DataField]
    public SanityThreshold CurrentThreshold = SanityThreshold.Normal;

    [DataField(customTypeSerializer: typeof(DictionarySerializer<SanityThreshold, float>))]
    public Dictionary<SanityThreshold, float> Thresholds = new()
    {
        { SanityThreshold.Normal, 85f },
        { SanityThreshold.Unstable, 65f },
        { SanityThreshold.NeedsHelp, 30f },
        { SanityThreshold.Breakdown, 10f },
        { SanityThreshold.Crisis, 0f },
    };

    /// <summary>
    /// Real-world time bookkeeping for the various passive drain sources - each ticks
    /// independently of round/game time, per the "real hour of shift" requirement.
    /// </summary>
    [DataField]
    public TimeSpan NextBaseDrainTime;

    [DataField]
    public TimeSpan NextSpaceDrainTime;

    [DataField]
    public TimeSpan NextDarknessDrainTime;

    [DataField]
    public TimeSpan NextDrunkDrainTime;

    [DataField]
    public TimeSpan NextSleepRecoveryTime;

    [DataField]
    public TimeSpan NextSatiationCheckTime;

    // Symptom timers (SanityBehaviorSystem) - game time, not real time, since these are
    // momentary flavour events rather than "per hour of shift" drain.
    [DataField]
    public TimeSpan NextShoutTime;

    [DataField]
    public TimeSpan NextDropItemTime;

    [DataField]
    public TimeSpan NextRareDamageTime;

    [DataField]
    public TimeSpan NextSelfHarmTime;

    /// <summary>
    /// Caps total sanity lost to drunkenness within one continuous drunk episode at 5%.
    /// Reset back to 0 once the drunk status effect ends.
    /// </summary>
    [DataField]
    public float DrunkSanityLostThisEpisode;

    /// <summary>
    /// The one-time "topped up hunger and thirst" sanity recovery only fires once per round.
    /// </summary>
    [DataField]
    public bool UsedSatiationRecoveryThisRound;

    /// <summary>
    /// Whether this character is immune to passive drain (and, symmetrically, passive
    /// recovery) because they spawned into the round with a Medical department job (other
    /// than Medical Intern). Snapshotted once, from the job actually held at spawn - see
    /// SanitySystem.OnPlayerSpawnComplete. A mid-round transfer into Medical does NOT flip
    /// this on retroactively, by design.
    /// </summary>
    [DataField]
    public bool IsMedicalImmune;
}
