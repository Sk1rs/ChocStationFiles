using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Cloning;
using Content.Shared._Sunrise.Sanity;
using Content.Shared._Sunrise.SunriseCCVars;
using Content.Shared.Chat;
using Content.Shared.Eye;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Pointing;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Physics;
using Content.Shared.SSDIndicator;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Spawns fake crewmember clones ("hallucinations") near players whose sanity has dropped
/// far enough. Only the hallucinating player can see/interact with each clone - each player
/// gets their own exclusive VisibilityFlags slot bit (a small pool, not one shared flag, so
/// different players' hallucinations never bleed into each other) - see
/// HallucinatingComponent/HallucinationClonedComponent and the slot allocator below.
/// </summary>
public sealed class HallucinationSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly SharedVisibilitySystem _visibility = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SanitySystem _sanity = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private const int MaxHallucinationsPerPlayer = 5;
    private const float SpawnRadius = 8f;
    private const int SpawnSpotAttempts = 20;
    private const float DismissSanityCost = 5f;

    // Keys must match actual DepartmentPrototype ids (Resources/Prototypes/Roles/Jobs/departments.yml) -
    // "Service" was never a real department id here (it's "Civilian"), so every civilian-job
    // hallucination (passengers, bartenders, chefs, botanists, clowns, mimes, chaplains...) was
    // silently missing this lookup and always falling back to the default sound. Engineering's
    // sound file also didn't exist on disk, so it likewise always fell back silently.
    private static readonly Dictionary<string, string> DepartmentDismissSounds = new()
    {
        { "Security", "/Audio/Effects/explosion_small1.ogg" },
        { "Medical", "/Audio/Items/Defib/defib_zap.ogg" },
        { "Engineering", "/Audio/Effects/metal_scrape1.ogg" },
        { "Civilian", "/Audio/Effects/glass_break1.ogg" },
        { "Science", "/Audio/Items/Artifact/artifact1.ogg" },
        { "Cargo", "/Audio/Items/Mining/pickaxe.ogg" },
        { "Command", "/Audio/Announcements/attention.ogg" },
    };

    private const string DefaultDismissSound = "/Audio/Effects/glass_step.ogg";

    // Each concurrently-hallucinating player gets their own exclusive bit from this pool
    // (see VisibilityFlags.HallucinationSlotBase) so their clones are only ever visible to
    // them, never to another hallucinating player. 8 concurrent slots is a lot of headroom
    // for a mechanic that requires being below "NeedsHelp" sanity; if it's ever exhausted,
    // allocation falls back to sharing slot 0 rather than breaking outright.
    private const int SlotCount = 8;
    private readonly bool[] _slotsInUse = new bool[SlotCount];

    private int AllocateVisibilitySlot()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            if (_slotsInUse[i])
                continue;

            _slotsInUse[i] = true;
            return (int)VisibilityFlags.HallucinationSlotBase << i;
        }

        return (int)VisibilityFlags.HallucinationSlotBase;
    }

    private void ReleaseVisibilitySlot(int bit)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            if (((int)VisibilityFlags.HallucinationSlotBase << i) != bit)
                continue;

            _slotsInUse[i] = false;
            return;
        }
    }

    /// <summary>
    /// Gets (or creates, allocating a fresh visibility slot) the HallucinatingComponent for a
    /// viewer. Shared by the passive per-tick spawner and SpawnHallucinationFor's direct/admin
    /// entry point, so both allocate a slot the same way.
    /// </summary>
    private HallucinatingComponent EnsureHallucinating(EntityUid viewer)
    {
        if (TryComp<HallucinatingComponent>(viewer, out var hallucinating))
            return hallucinating;

        hallucinating = AddComp<HallucinatingComponent>(viewer);
        hallucinating.VisibilitySlotBit = AllocateVisibilitySlot();
        _eye.RefreshVisibilityMask((viewer, null));
        return hallucinating;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HallucinatingComponent, GetVisMaskEvent>(OnHallucinatingVis);
        SubscribeLocalEvent<HallucinatingComponent, ComponentShutdown>(OnHallucinatingShutdown);

        SubscribeLocalEvent<HallucinationClonedComponent, InteractHandEvent>(OnCloneInteractHand);
        SubscribeLocalEvent<HallucinationClonedComponent, AfterGotPointedAtEvent>(OnCloneGotPointedAt);

        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
    }

    /// <summary>
    /// Live speech mirroring: whatever a crewmember actually says out loud is immediately
    /// echoed by every hallucination clone currently impersonating them (there can be more
    /// than one, seen by different hallucinating viewers). Whispers/radio are excluded - only
    /// plain local speech carries over. This is intentionally NOT tied to interacting with the
    /// clone - it happens continuously, in real time, whether or not anyone is looking at it.
    ///
    /// Delivered only to the one hallucinating player, bypassing ChatSystem's normal
    /// range-based broadcast entirely (via IChatManager.ChatMessageToOne) - a real bystander
    /// standing near the clone must NOT see/hear a message from someone they can't see. The
    /// clone is only ever visible to its one viewer (VisibilityFlags.HallucinationSlotBase -
    /// see the per-viewer slot allocator below), so its "speech" has to be equally private.
    /// </summary>
    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        if (args.Channel != null || args.ObfuscatedMessage != null)
            return;

        var query = EntityQueryEnumerator<HallucinationClonedComponent>();
        while (query.MoveNext(out var cloneUid, out var cloned))
        {
            if (cloned.Source != args.Source)
                continue;

            if (!_playerManager.TryGetSessionByEntity(cloned.HallucinatingPlayer, out var session))
                continue;

            var speech = _chat.GetSpeechVerb(cloneUid, args.Message);
            var name = FormattedMessage.EscapeText(Name(cloneUid));
            var message = FormattedMessage.EscapeText(args.Message);
            var wrappedMessage = Loc.GetString(speech.Bold ? "chat-manager-entity-say-bold-wrap-message" : "chat-manager-entity-say-wrap-message",
                ("entityName", name),
                ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
                ("fontType", speech.FontId),
                ("fontSize", speech.FontSize),
                ("message", message));

            _chatManager.ChatMessageToOne(ChatChannel.Local, args.Message, wrappedMessage, cloneUid, false, session.Channel);
        }
    }

    private void OnHallucinatingShutdown(Entity<HallucinatingComponent> ent, ref ComponentShutdown args)
    {
        foreach (var hallucination in ent.Comp.ActiveHallucinations)
        {
            QueueDel(hallucination);
        }

        ReleaseVisibilitySlot(ent.Comp.VisibilitySlotBit);
    }

    private void OnHallucinatingVis(Entity<HallucinatingComponent> ent, ref GetVisMaskEvent args)
    {
        args.VisibilityMask |= ent.Comp.VisibilitySlotBit;
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
            if (sanity.CurrentThreshold is SanityThreshold.Normal or SanityThreshold.Unstable)
            {
                // Recovered - release their slot (and delete any lingering clones) instead of
                // holding onto it, unused, for the rest of the round. See OnHallucinatingShutdown.
                if (HasComp<HallucinatingComponent>(uid))
                {
                    RemComp<HallucinatingComponent>(uid);
                    _eye.RefreshVisibilityMask((uid, null));
                }

                continue;
            }

            if (!_mobState.IsAlive(uid))
                continue;

            if (HasComp<PsychicCritComponent>(uid))
                continue;

            var hallucinating = EnsureHallucinating(uid);

            if (now < hallucinating.NextSpawnTime)
                continue;

            // Cull dead/deleted references before checking the cap.
            hallucinating.ActiveHallucinations.RemoveAll(h => !Exists(h));

            hallucinating.NextSpawnTime = now + GetSpawnInterval(sanity.CurrentThreshold);

            if (hallucinating.ActiveHallucinations.Count >= MaxHallucinationsPerPlayer)
                continue;

            TrySpawnHallucination(uid, hallucinating);
        }
    }

    private static TimeSpan GetSpawnInterval(SanityThreshold threshold)
    {
        return threshold switch
        {
            SanityThreshold.NeedsHelp => TimeSpan.FromMinutes(6),
            SanityThreshold.Breakdown => TimeSpan.FromMinutes(3),
            _ => TimeSpan.FromMinutes(1.5),
        };
    }

    private void TrySpawnHallucination(EntityUid viewer, HallucinatingComponent hallucinating)
    {
        if (!TryPickRandomCrewmember(out var sourceUid))
            return;

        SpawnHallucinationFor(viewer, sourceUid, hallucinating);
    }

    /// <summary>
    /// Spawns a hallucination clone of <paramref name="source"/>, visible only to
    /// <paramref name="viewer"/>. Used both by the periodic passive spawner and by the
    /// admin "summon hallucination" smite (which bypasses the tier/cooldown/cap gating).
    /// </summary>
    public bool SpawnHallucinationFor(EntityUid viewer, EntityUid source, HallucinatingComponent? hallucinating = null)
    {
        hallucinating ??= EnsureHallucinating(viewer);

        if (!TryComp<HumanoidAppearanceComponent>(source, out var sourceAppearance))
            return false;

        if (!_proto.TryIndex<SpeciesPrototype>(sourceAppearance.Species, out var species))
            return false;

        if (!TryFindSpawnSpot(viewer, out var coords))
            return false;

        var clone = Spawn(species.Prototype, coords);
        _humanoidAppearance.CloneAppearance(source, clone);
        _metaData.SetEntityName(clone, Name(source));

        if (HasComp<InventoryComponent>(source))
            _cloning.CopyEquipment((source, null), (clone, null), SlotFlags.All);

        // The species prototype gives every spawned humanoid a real, ticking Sanity/Hunger/
        // Thirst - none of which make sense for a fake illusion that isn't a real character.
        RemComp<SanityComponent>(clone);
        RemComp<HungerComponent>(clone);
        RemComp<ThirstComponent>(clone);
        // Without a mind this would otherwise show the "SSD" (disconnected) sleep icon.
        RemComp<SSDIndicatorComponent>(clone);

        var department = GetDepartmentId(source);

        var cloned = AddComp<HallucinationClonedComponent>(clone);
        cloned.HallucinatingPlayer = viewer;
        cloned.DepartmentId = department;
        cloned.Source = source;

        _visibility.AddLayer((clone, null), (ushort)hallucinating.VisibilitySlotBit);

        hallucinating.ActiveHallucinations.Add(clone);
        return true;
    }

    private bool TryPickRandomCrewmember(out EntityUid uid)
    {
        uid = default;

        var candidates = new List<EntityUid>();
        foreach (var player in _playerManager.Sessions)
        {
            if (player.AttachedEntity is not { } attached)
                continue;

            if (!HasComp<HumanoidAppearanceComponent>(attached))
                continue;

            candidates.Add(attached);
        }

        if (candidates.Count == 0)
            return false;

        uid = _random.Pick(candidates);
        return true;
    }

    private bool TryFindSpawnSpot(EntityUid viewer, out EntityCoordinates coords)
    {
        coords = default;
        var xform = Transform(viewer);

        if (xform.GridUid == null)
            return false;

        // Approximation of "at the end of a nearby corridor": pick a random reachable,
        // unobstructed point some distance away on the same grid. Not real corridor/LOS
        // detection, but at least it won't land inside a wall - see ScramOnTriggerSystem
        // for the pattern this is based on.
        for (var i = 0; i < SpawnSpotAttempts; i++)
        {
            var candidate = xform.Coordinates.Offset(_random.NextVector2(4f, SpawnRadius));

            if (!_turf.TryGetTileRef(candidate, out var tileRef))
                continue;

            if (_turf.IsSpace(tileRef.Value) || _turf.IsTileBlocked(tileRef.Value, CollisionGroup.MobMask))
                continue;

            coords = candidate;
            return true;
        }

        return false;
    }

    private string? GetDepartmentId(EntityUid uid)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return null;

        if (!_jobs.MindTryGetJobId(mindId, out var jobId) || jobId is null)
            return null;

        return _jobs.TryGetDepartment(jobId.Value.Id, out var department) ? department.ID : null;
    }

    private void OnCloneInteractHand(Entity<HallucinationClonedComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Dismiss(ent);
    }

    private void OnCloneGotPointedAt(Entity<HallucinationClonedComponent> ent, ref AfterGotPointedAtEvent args)
    {
        // Only the hallucinating player themselves can even see this entity to point at it in
        // the first place (each player's clones use their own exclusive visibility slot bit),
        // so no extra ownership check needed.
        Dismiss(ent);
    }

    private void Dismiss(Entity<HallucinationClonedComponent> ent)
    {
        var owner = ent.Comp.HallucinatingPlayer;

        if (TryComp<SanityComponent>(owner, out var sanity))
            _sanity.ChangeSanity(owner, sanity, -DismissSanityCost);

        if (TryComp<HallucinatingComponent>(owner, out var hallucinating))
            hallucinating.ActiveHallucinations.Remove(ent.Owner);

        var soundPath = ent.Comp.DepartmentId != null && DepartmentDismissSounds.TryGetValue(ent.Comp.DepartmentId, out var mapped)
            ? mapped
            : DefaultDismissSound;

        if (_playerManager.TryGetSessionByEntity(owner, out var session))
        {
            _audio.PlayGlobal(new SoundPathSpecifier(soundPath), Filter.SinglePlayer(session), false);
        }

        QueueDel(ent.Owner);
    }
}
