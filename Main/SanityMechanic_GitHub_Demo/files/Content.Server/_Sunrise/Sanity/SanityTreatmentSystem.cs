using Content.Shared._Sunrise.Sanity;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Handles the CMO/Psychologist "counseling session" ability: a 3-minute channeled
/// treatment on a single target that heals 3-15% sanity, on a 1.5 minute cooldown
/// (the cooldown itself lives on the action's UseDelay).
/// </summary>
public sealed class SanityTreatmentSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SanitySystem _sanity = default!;
    [Dependency] private readonly PsychicCritSystem _psychicCrit = default!;

    private static readonly TimeSpan TreatmentDuration = TimeSpan.FromMinutes(3);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityTreatmentCapableComponent, ComponentStartup>(OnCapableStartup);
        SubscribeLocalEvent<SanityTreatmentCapableComponent, SanityTreatmentActionEvent>(OnTreatmentAction);
        SubscribeLocalEvent<SanityTreatmentCapableComponent, SanityTreatmentDoAfterEvent>(OnTreatmentDoAfter);
    }

    private void OnCapableStartup(Entity<SanityTreatmentCapableComponent> ent, ref ComponentStartup args)
    {
        _actions.AddAction(ent.Owner, "ActionSanityTreatment");
    }

    private void OnTreatmentAction(Entity<SanityTreatmentCapableComponent> ent, ref SanityTreatmentActionEvent args)
    {
        if (args.Handled || args.Target == EntityUid.Invalid)
            return;

        var target = args.Target;

        if (!TryComp<SanityComponent>(target, out _))
            return;

        // Roll the heal amount once up-front, applied once the full session completes.
        var healAmount = _random.NextFloat(3f, 15f);

        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, TreatmentDuration,
            new SanityTreatmentDoAfterEvent { HealAmount = healAmount }, ent.Owner, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
        {
            _popup.PopupEntity(Loc.GetString("sanity-treatment-start", ("user", ent.Owner), ("target", target)), ent.Owner);
            args.Handled = true;
        }
    }

    private void OnTreatmentDoAfter(Entity<SanityTreatmentCapableComponent> ent, ref SanityTreatmentDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        if (!TryComp<SanityComponent>(target, out var sanityComp))
            return;

        var wasInCrisis = HasComp<PsychicCritComponent>(target);

        _sanity.ChangeSanity(target, sanityComp, args.HealAmount);

        if (wasInCrisis)
            _psychicCrit.ExitPsychicCrit(target);

        _popup.PopupEntity(Loc.GetString("sanity-treatment-success", ("user", ent.Owner), ("target", target)), ent.Owner);
        args.Handled = true;
    }
}
