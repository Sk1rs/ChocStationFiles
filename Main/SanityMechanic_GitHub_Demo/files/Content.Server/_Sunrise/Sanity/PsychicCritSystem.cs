using System.Text;
using Content.Shared._Sunrise.Sanity;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Drives the "psychic crisis" state entered at 0% sanity (see PsychicCritComponent) -
/// forces the entity down and immobile without touching health/MobState at all. Entered by
/// SanitySystem.ChangeSanity, exited only by SanityTreatmentSystem.
/// </summary>
public sealed class PsychicCritSystem : EntitySystem
{
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    // While in crisis, someone can only administer pills (haloperidol included) - normal food
    // and drink are refused, since the patient is unconscious and can't eat/drink normally.
    private static readonly EntityWhitelist PillWhitelist = new() { Components = new[] { "Pill" } };

    // Garbled "anomaly-effect"-style speech while in crisis - keeps word/space rhythm so it
    // still reads as speech, but every character becomes noise.
    private const string GarbleSymbols = "!@#$%&?*Ø×÷§";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsychicCritComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<PsychicCritComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<PsychicCritComponent, StandAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<PsychicCritComponent, AccentGetEvent>(OnAccentGet);
    }

    private void OnAccentGet(Entity<PsychicCritComponent> ent, ref AccentGetEvent args)
    {
        var sb = new StringBuilder(args.Message.Length);
        foreach (var ch in args.Message)
            sb.Append(char.IsWhiteSpace(ch) ? ch : GarbleSymbols[_random.Next(GarbleSymbols.Length)]);

        args.Message = sb.ToString();
    }

    private void OnUpdateCanMove(Entity<PsychicCritComponent> ent, ref UpdateCanMoveEvent args)
    {
        args.Cancel();
    }

    private void OnRefreshMoveSpeed(Entity<PsychicCritComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(0f, 0f);
    }

    private void OnStandAttempt(Entity<PsychicCritComponent> ent, ref StandAttemptEvent args)
    {
        args.Cancel();
    }

    /// <summary>
    /// Puts the entity into psychic crisis: down, unable to stand or move, but otherwise
    /// completely unaffected (no damage, no MobState change). No-op if already in crisis.
    /// </summary>
    public void EnterPsychicCrit(EntityUid uid)
    {
        if (HasComp<PsychicCritComponent>(uid))
            return;

        var crit = AddComp<PsychicCritComponent>(uid);
        _standing.Down(uid, playSound: false, dropHeldItems: true, force: true);
        _popup.PopupEntity(Loc.GetString("sanity-psychic-crit-enter"), uid, uid, PopupType.LargeCaution);

        RestrictDigestionToPills(uid, crit);
    }

    /// <summary>
    /// Brings the entity back out of psychic crisis. Should only be called by
    /// SanityTreatmentSystem after a completed CMO/Psychologist treatment session.
    /// </summary>
    public void ExitPsychicCrit(EntityUid uid)
    {
        if (!TryComp<PsychicCritComponent>(uid, out var crit))
            return;

        RestoreDigestion(crit);
        RemComp<PsychicCritComponent>(uid);

        _standing.Stand(uid, force: true);
        _popup.PopupEntity(Loc.GetString("sanity-psychic-crit-exit"), uid, uid, PopupType.Medium);
    }

    /// <summary>
    /// While unconscious in crisis, the patient can only be given pills (haloperidol
    /// included) - regular food and drink require being conscious enough to eat/drink
    /// normally. Implemented via the same "special digestible" whitelist mechanism used for
    /// restricted diets, so it goes through the normal digestibility checks rather than
    /// touching the shared ingestion-blocker pipeline used by gas masks etc.
    /// </summary>
    private void RestrictDigestionToPills(EntityUid uid, PsychicCritComponent crit)
    {
        if (!_body.TryGetBodyOrganEntityComps<StomachComponent>((uid, null), out var stomachs))
            return;

        var restricted = new List<(EntityUid, EntityWhitelist?, bool)>();
        foreach (var stomach in stomachs)
        {
            restricted.Add((stomach.Owner, stomach.Comp1.SpecialDigestible, stomach.Comp1.IsSpecialDigestibleExclusive));
            _stomach.SetSpecialDigestible(stomach.Comp1, PillWhitelist);
            _stomach.SetSpecialDigestibleExclusive(stomach.Comp1, true);
        }

        crit.RestrictedStomachs = restricted;
    }

    private void RestoreDigestion(PsychicCritComponent crit)
    {
        if (crit.RestrictedStomachs is not { } restricted)
            return;

        foreach (var (stomachUid, previousWhitelist, previousExclusive) in restricted)
        {
            if (!TryComp<StomachComponent>(stomachUid, out var stomach))
                continue;

            _stomach.SetSpecialDigestible(stomach, previousWhitelist);
            _stomach.SetSpecialDigestibleExclusive(stomach, previousExclusive);
        }

        crit.RestrictedStomachs = null;
    }
}
