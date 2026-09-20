using Content.Server.Chat.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Sunrise.Sanity;
using Content.Shared._Sunrise.SunriseCCVars;
using Content.Shared.Chat;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Sunrise.Eye;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Drives the visible "symptoms" tied to each sanity tier - random outbursts, dropped
/// items, disabled sprint/mumbling, refusing food/drink, and (at Crisis) self-harm.
/// Symptoms are cumulative: worse tiers keep everything from better tiers plus their own.
/// All periodic symptoms use explicit "next time" timers (like the passive drain in
/// SanitySystem) rather than per-tick probability rolls, so they reliably happen within a
/// predictable window instead of being theoretically-possible-but-basically-never.
/// </summary>
public sealed class SanityBehaviorSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SanitySystem _sanity = default!;

    // Outbursts only start at NeedsHelp ("third tier") and worse, and are several times
    // rarer than the other symptoms - a rare, unsettling event rather than background noise.
    private const float ShoutMinSeconds = 240f;
    private const float ShoutMaxSeconds = 480f;

    // Mumbling used to garble roughly every other line, which read as constant rather than
    // occasional - now a rare tic instead.
    private const float MumbleChance = 0.15f;

    private static readonly string[] ShoutMessages =
    {
        "sanity-shout-1",
        "sanity-shout-2",
        "sanity-shout-3",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<SanityComponent, AccentGetEvent>(OnAccentGet);
        SubscribeLocalEvent<SanityComponent, GetVisionDarkeningEvent>(OnGetVisionDarkening);
    }

    private void OnGetVisionDarkening(Entity<SanityComponent> ent, ref GetVisionDarkeningEvent args)
    {
        args.Strength += ent.Comp.CurrentThreshold switch
        {
            SanityThreshold.NeedsHelp => 1f,
            SanityThreshold.Breakdown => 3f,
            SanityThreshold.Crisis => 6f,
            _ => 0f,
        };
    }

    /// <summary>
    /// SanitySystem already owns ComponentStartup for SanityComponent (a component can only
    /// have one subscriber per event in this engine), so symptom timers are lazily
    /// initialized here on first sight instead of via their own ComponentStartup handler.
    /// </summary>
    private void EnsureTimersInitialized(SanityComponent sanity, TimeSpan now)
    {
        if (sanity.NextShoutTime != default)
            return;

        sanity.NextShoutTime = now + GetSymptomInterval(0, ShoutMinSeconds, ShoutMaxSeconds);
        sanity.NextDropItemTime = now + GetSymptomInterval(0, 15, 35);
        sanity.NextRareDamageTime = now + GetSymptomInterval(0, 40, 90);
        sanity.NextSelfHarmTime = now + TimeSpan.FromSeconds(_random.NextFloat(20, 40));
    }

    private void OnRefreshMoveSpeed(EntityUid uid, SanityComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        // Unstable and worse: sprinting gives no benefit over walking (same effect as
        // holding the walk key permanently, but the player doesn't get a choice).
        if ((int)component.CurrentThreshold >= (int)SanityThreshold.Unstable &&
            TryComp<MovementSpeedModifierComponent>(uid, out var moveSpeed) &&
            moveSpeed.BaseSprintSpeed > 0f)
        {
            args.ModifySpeed(1f, moveSpeed.BaseWalkSpeed / moveSpeed.BaseSprintSpeed);
        }
    }

    private void OnAccentGet(Entity<SanityComponent> ent, ref AccentGetEvent args)
    {
        if ((int)ent.Comp.CurrentThreshold < (int)SanityThreshold.Unstable)
            return;

        // Mumbles instead of speaking clearly - a rare tic, not most lines.
        if (_random.Prob(MumbleChance))
            args.Message = _replacement.ApplyReplacements(args.Message, "mumble");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_cfg.GetCVar(SunriseCCVars.SanityEnabled))
            return;

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<SanityComponent>();
        while (query.MoveNext(out var uid, out var sanity))
        {
            if (!_mobState.IsAlive(uid))
                continue;

            if (sanity.IsMedicalImmune)
                continue;

            // Psychic crisis (0% sanity) replaces all of these symptoms with a single
            // "lying down, unresponsive" state - see PsychicCritSystem. Pacifists can't hurt
            // themselves even now, so instead of going silent they cry out often while down.
            if (HasComp<PsychicCritComponent>(uid))
            {
                if (HasComp<PacifiedComponent>(uid) && now >= sanity.NextSelfHarmTime)
                {
                    sanity.NextSelfHarmTime = now + TimeSpan.FromSeconds(_random.NextFloat(8, 18));
                    _chat.TryEmoteWithChat(uid, "Scream", ignoreActionBlocker: true);
                }

                continue;
            }

            EnsureTimersInitialized(sanity, now);

            var rank = (int)sanity.CurrentThreshold;

            // Can't eat from NeedsHelp onward, can't drink either from Breakdown onward -
            // implemented as one blanket ingestion block (the underlying blocker component
            // doesn't distinguish food from drink) that kicks in at the earlier tier.
            var shouldBlockIngestion = rank >= (int)SanityThreshold.NeedsHelp;
            if (shouldBlockIngestion != HasComp<IngestionBlockerComponent>(uid))
            {
                if (shouldBlockIngestion)
                    AddComp<IngestionBlockerComponent>(uid);
                else
                    RemComp<IngestionBlockerComponent>(uid);
            }

            // Random outburst - NeedsHelp ("third tier") and worse only, much rarer than the
            // other symptoms.
            if (rank >= (int)SanityThreshold.NeedsHelp && now >= sanity.NextShoutTime)
            {
                var shoutRank = rank - (int)SanityThreshold.NeedsHelp;
                sanity.NextShoutTime = now + GetSymptomInterval(shoutRank, ShoutMinSeconds, ShoutMaxSeconds);
                _chat.TrySendInGameICMessage(uid, Loc.GetString(_random.Pick(ShoutMessages)), InGameICChatType.Speak, hideChat: false);
            }

            // Drops something held - Unstable and worse.
            if (rank >= (int)SanityThreshold.Unstable && now >= sanity.NextDropItemTime)
            {
                sanity.NextDropItemTime = now + GetSymptomInterval(rank, 15, 35);
                var held = new List<EntityUid>(_hands.EnumerateHeld(uid));
                if (held.Count > 0)
                    _hands.TryDrop((uid, null), _random.Pick(held));
            }

            var isPacifist = HasComp<PacifiedComponent>(uid);

            // Rare self-inflicted damage - NeedsHelp and worse. Pacifists get a harmless
            // breakdown moment instead of hurting themselves.
            if (rank >= (int)SanityThreshold.NeedsHelp && now >= sanity.NextRareDamageTime)
            {
                sanity.NextRareDamageTime = now + GetSymptomInterval(rank, 40, 90);

                if (isPacifist)
                {
                    _sanity.ChangeSanity(uid, sanity, -1f);
                    _popup.PopupEntity(Loc.GetString("sanity-pacifist-distress"), uid, uid);
                }
                else
                {
                    var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Blunt"), FixedPoint2.New(2));
                    _damageable.TryChangeDamage((uid, null), damage, true, false);
                }
            }

            // Involuntary self-harm - Crisis only, worse than the above. Plays a species-
            // appropriate pain sound via the Scream emote either way. Pacifists still cry
            // out and lose extra sanity, but never actually hurt themselves.
            if (rank >= (int)SanityThreshold.Crisis && now >= sanity.NextSelfHarmTime)
            {
                sanity.NextSelfHarmTime = now + TimeSpan.FromSeconds(_random.NextFloat(20, 40));
                _chat.TryEmoteWithChat(uid, "Scream", ignoreActionBlocker: true);

                if (isPacifist)
                {
                    _sanity.ChangeSanity(uid, sanity, -2f);
                    _popup.PopupEntity(Loc.GetString("sanity-pacifist-self-harm"), uid, uid, PopupType.LargeCaution);
                }
                else
                {
                    var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(4));
                    _damageable.TryChangeDamage((uid, null), damage, true, false);
                    _popup.PopupEntity(Loc.GetString("sanity-self-harm"), uid, uid, PopupType.LargeCaution);
                }
            }
        }
    }

    /// <summary>
    /// Interval range for a periodic symptom, shrinking as the tier worsens - e.g. at
    /// Normal (rank 0) you get the full [min, max] range, at Crisis (rank 4) it's divided
    /// by 5.
    /// </summary>
    private TimeSpan GetSymptomInterval(int rank, float minSeconds, float maxSeconds)
    {
        var divisor = rank + 1;
        return TimeSpan.FromSeconds(_random.NextFloat(minSeconds, maxSeconds) / divisor);
    }
}
