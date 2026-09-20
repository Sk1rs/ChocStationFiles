using Content.Server.Sunrise.Eye;
using Content.Shared._Sunrise.Sanity;
using Content.Shared._Sunrise.SunriseCCVars;
using Content.Shared.Bed.Sleep;
using Content.Shared.Drunk;
using Content.Shared.GameTicking;
using Content.Shared.Sunrise.Eye;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.StatusEffectNew;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Drives the passive drain/recovery side of the sanity mechanic. Examine text and
/// treatment are handled by SanityExamineSystem/SanityTreatmentSystem respectively.
/// </summary>
public sealed class SanitySystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DarkenedVisionSystem _darkenedVision = default!;
    [Dependency] private readonly PsychicCritSystem _psychicCrit = default!;

    private static readonly TimeSpan SpaceDrainInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DarknessDrainInterval = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan DrunkDrainInterval = TimeSpan.FromSeconds(39);
    private static readonly TimeSpan SleepRecoveryInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SatiationCheckInterval = TimeSpan.FromSeconds(10);

    private const float SpaceDistance = 57f;
    private const float DarknessCheckRadius = 3f;
    private const float DrunkEpisodeCap = 5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnStartup(Entity<SanityComponent> ent, ref ComponentStartup args)
    {
        var now = _timing.RealTime;
        ent.Comp.NextBaseDrainTime = now + TimeSpan.FromHours(1);
        ent.Comp.NextSpaceDrainTime = now + SpaceDrainInterval;
        ent.Comp.NextDarknessDrainTime = now + DarknessDrainInterval;
        ent.Comp.NextDrunkDrainTime = now + DrunkDrainInterval;
        ent.Comp.NextSleepRecoveryTime = now + SleepRecoveryInterval;
        ent.Comp.NextSatiationCheckTime = now + SatiationCheckInterval;
        ent.Comp.CurrentThreshold = GetSanityThreshold(ent.Comp);
    }

    /// <summary>
    /// Snapshots medical-department immunity from the job the character actually spawned
    /// with. This fires after the mind/job is fully attached (unlike ComponentStartup, which
    /// runs mid-spawn before that's true), and only once - a mid-round transfer into Medical
    /// does NOT retroactively grant immunity. That's deliberate: immunity is meant to reflect
    /// training/preparation going into the shift, not whatever your current job title says.
    /// </summary>
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!TryComp<SanityComponent>(args.Mob, out var sanity))
            return;

        sanity.IsMedicalImmune = ComputeMedicalImmunity(args.JobId);
    }

    private bool ComputeMedicalImmunity(string? jobId)
    {
        if (jobId is null || jobId == "MedicalIntern")
            return false;

        return _jobs.TryGetDepartment(jobId, out var department) && department.ID == "Medical";
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_cfg.GetCVar(SunriseCCVars.SanityEnabled))
            return;

        var now = _timing.RealTime;
        var query = EntityQueryEnumerator<SanityComponent>();
        while (query.MoveNext(out var uid, out var sanity))
        {
            if (!_mobState.IsAlive(uid))
                continue;

            if (sanity.IsMedicalImmune)
            {
                // Still let their alertness threshold stay accurate for examine, but skip all drain.
                continue;
            }

            if (HasComp<PsychicCritComponent>(uid))
                continue;

            TickBaseDrain(uid, sanity, now);
            TickSpaceDrain(uid, sanity, now);
            TickDarknessDrain(uid, sanity, now);
            TickDrunkDrain(uid, sanity, now);
            TickSleepRecovery(uid, sanity, now);
            TickSatiationRecovery(uid, sanity, now);
        }
    }

    private void TickBaseDrain(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextBaseDrainTime)
            return;

        var severe = sanity.CurrentThreshold is SanityThreshold.NeedsHelp or SanityThreshold.Breakdown or SanityThreshold.Crisis;
        var interval = severe ? TimeSpan.FromHours(1.5) : TimeSpan.FromHours(1);
        var amount = severe ? _random.NextFloat(3f, 4f) : _random.NextFloat(1f, 2f);

        sanity.NextBaseDrainTime = now + interval;
        ChangeSanity(uid, sanity, -amount);
    }

    private void TickSpaceDrain(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextSpaceDrainTime)
            return;

        sanity.NextSpaceDrainTime = now + SpaceDrainInterval;

        var xform = Transform(uid);
        if (xform.GridUid != null)
            return;

        // No grid within range at all - counts as "far from the station/shuttle".
        if (_lookup.GetEntitiesInRange<MapGridComponent>(xform.Coordinates, SpaceDistance).Count > 0)
            return;

        ChangeSanity(uid, sanity, -3f);
    }

    private void TickDarknessDrain(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextDarknessDrainTime)
            return;

        sanity.NextDarknessDrainTime = now + DarknessDrainInterval;

        var xform = Transform(uid);
        // Approximation only - the engine has no server-side lit-tile query. We treat
        // "no enabled light source nearby" as darkness, which isn't perfectly accurate
        // but is the best available signal server-side.
        foreach (var light in _lookup.GetEntitiesInRange<PointLightComponent>(xform.Coordinates, DarknessCheckRadius))
        {
            if (light.Comp.Enabled)
                return;
        }

        ChangeSanity(uid, sanity, -2f);
    }

    private void TickDrunkDrain(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextDrunkDrainTime)
            return;

        sanity.NextDrunkDrainTime = now + DrunkDrainInterval;

        if (!_statusEffects.HasStatusEffect(uid, SharedDrunkSystem.Drunk))
        {
            sanity.DrunkSanityLostThisEpisode = 0f;
            return;
        }

        if (sanity.DrunkSanityLostThisEpisode >= DrunkEpisodeCap)
            return;

        var amount = MathF.Min(1f, DrunkEpisodeCap - sanity.DrunkSanityLostThisEpisode);
        sanity.DrunkSanityLostThisEpisode += amount;
        ChangeSanity(uid, sanity, -amount);
    }

    /// <summary>
    /// Small passive recovery while asleep. Only applies while sanity is still relatively fine.
    /// </summary>
    private void TickSleepRecovery(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextSleepRecoveryTime)
            return;

        sanity.NextSleepRecoveryTime = now + SleepRecoveryInterval;

        if (sanity.CurrentThreshold is not (SanityThreshold.Normal or SanityThreshold.Unstable))
            return;

        if (!HasComp<SleepingComponent>(uid))
            return;

        ChangeSanity(uid, sanity, _random.NextFloat(1f, 6f));
    }

    /// <summary>
    /// One-time-per-round recovery for topping up hunger and thirst to full.
    /// Only applies while sanity is still relatively fine.
    /// </summary>
    private void TickSatiationRecovery(EntityUid uid, SanityComponent sanity, TimeSpan now)
    {
        if (now < sanity.NextSatiationCheckTime)
            return;

        sanity.NextSatiationCheckTime = now + SatiationCheckInterval;

        if (sanity.UsedSatiationRecoveryThisRound)
            return;

        if (sanity.CurrentThreshold is not (SanityThreshold.Normal or SanityThreshold.Unstable))
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger) || hunger.CurrentThreshold != HungerThreshold.Overfed)
            return;

        if (!TryComp<ThirstComponent>(uid, out var thirst) || thirst.CurrentThirstThreshold != ThirstThreshold.OverHydrated)
            return;

        sanity.UsedSatiationRecoveryThisRound = true;
        ChangeSanity(uid, sanity, _random.NextFloat(1f, 3f));
    }

    public bool IsMedicalDepartmentImmune(SanityComponent component)
    {
        return component.IsMedicalImmune;
    }

    public SanityThreshold GetSanityThreshold(SanityComponent component, float? sanity = null)
    {
        sanity ??= component.CurrentSanity;
        var result = SanityThreshold.Crisis;

        foreach (var (threshold, cutoff) in component.Thresholds)
        {
            if (sanity >= cutoff && cutoff >= component.Thresholds[result])
                result = threshold;
        }

        return result;
    }

    public void ChangeSanity(EntityUid uid, SanityComponent component, float amount)
    {
        var oldThreshold = component.CurrentThreshold;
        component.CurrentSanity = Math.Clamp(component.CurrentSanity + amount, 0f, 100f);
        component.CurrentThreshold = GetSanityThreshold(component);

        if (component.CurrentThreshold != oldThreshold)
        {
            EnsureComp<DarkenedVisionComponent>(uid);
            _darkenedVision.UpdateVisionDarkening(uid);
        }

        if (component.CurrentSanity <= 0f)
            _psychicCrit.EnterPsychicCrit(uid);
    }

    private static readonly SanityThreshold[] ThresholdOrder =
    {
        SanityThreshold.Normal,
        SanityThreshold.Unstable,
        SanityThreshold.NeedsHelp,
        SanityThreshold.Breakdown,
        SanityThreshold.Crisis,
    };

    /// <summary>
    /// Admin tool: forces sanity down to the start of the next-worse tier. Does nothing if
    /// already at Crisis (that's as far as the tier ladder goes - see PsychicCritComponent
    /// for what's below it).
    /// </summary>
    /// <returns>True if a tier change was made.</returns>
    public bool LowerThresholdByOne(EntityUid uid, SanityComponent component)
    {
        var index = Array.IndexOf(ThresholdOrder, component.CurrentThreshold);
        if (index < 0 || index >= ThresholdOrder.Length - 1)
            return false;

        var target = ThresholdOrder[index + 1];
        component.CurrentSanity = component.Thresholds[target];
        component.CurrentThreshold = target;
        return true;
    }

    /// <summary>
    /// Admin tool: fully restores sanity to 100% and, if the target was in psychic crisis,
    /// brings them back out of it - same clean exit SanityTreatmentSystem uses after a
    /// completed CMO/Psychologist session.
    /// </summary>
    public void RestoreSanity(EntityUid uid, SanityComponent component)
    {
        component.CurrentSanity = 100f;
        component.CurrentThreshold = SanityThreshold.Normal;
        component.DrunkSanityLostThisEpisode = 0f;

        _darkenedVision.UpdateVisionDarkening(uid);

        if (HasComp<PsychicCritComponent>(uid))
            _psychicCrit.ExitPsychicCrit(uid);
    }
}
