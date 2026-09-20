using Content.Shared._Sunrise.Sanity;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Lets the Chief Medical Officer, Senior Physician and Psychologist see a patient's sanity
/// tier via an "Examine" verb - the raw number is never shown, only which of the 5 tiers
/// they're currently in.
/// </summary>
public sealed class SanityExamineSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;

    private static readonly string[] AllowedExaminerJobs =
    {
        "ChiefMedicalOfficer",
        "SeniorPhysician",
        "Psychologist",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnGetExamineVerbs(Entity<SanityComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!CanExamineSanity(args.User))
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, ent.Owner);
        var user = args.User;

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = FormattedMessage.FromMarkupOrThrow(
                    Loc.GetString($"sanity-examine-{ent.Comp.CurrentThreshold.ToString().ToLowerInvariant()}"));
                _examineSystem.SendExamineTooltip(user, ent.Owner, markup, false, false);
            },
            Text = Loc.GetString("sanity-examine-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("sanity-examine-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rejuvenate.svg.192dpi.png")),
        };

        args.Verbs.Add(verb);
    }

    private bool CanExamineSanity(EntityUid examiner)
    {
        if (!_mind.TryGetMind(examiner, out var mindId, out _))
            return false;

        if (!_jobs.MindTryGetJobId(mindId, out var jobId) || jobId is null)
            return false;

        foreach (var allowed in AllowedExaminerJobs)
        {
            if (jobId.Value.Id == allowed)
                return true;
        }

        return false;
    }
}
