using System.Numerics;
using Content.Client.Stunnable;
using Content.Shared._Sunrise.Sanity;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client._Sunrise.Sanity;

/// <summary>
/// Makes an entity in psychic crisis (see PsychicCritComponent) visibly shake/jitter, the
/// same "fatigue" sprite animation used for stun/stamina-crit - without actually applying a
/// stun status effect.
/// </summary>
public sealed class PsychicCritVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    private const string AnimationKey = "psychic-crit-shake";

    // Deliberately gentler than the vanilla stun/stamina-crit shake (which itself peaks around
    // a 0.04 amplitude at full jitter multiplier) - this plays continuously for as long as the
    // crisis lasts, so a strong shake reads as far too intense over that length of time.
    private const float Frequency = 5f;
    private const int Jitters = 4;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsychicCritComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PsychicCritComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsychicCritComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnStartup(Entity<PsychicCritComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        ent.Comp.StartOffset = sprite.Offset;
        PlayAnimation((ent, ent.Comp, sprite));
    }

    private void OnShutdown(Entity<PsychicCritComponent> ent, ref ComponentShutdown args)
    {
        _animation.Stop(ent.Owner, AnimationKey);

        if (TryComp<SpriteComponent>(ent, out var sprite))
            _sprite.SetOffset((ent.Owner, sprite), ent.Comp.StartOffset);
    }

    private void OnAnimationCompleted(Entity<PsychicCritComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != AnimationKey || !args.Finished)
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        PlayAnimation((ent, ent.Comp, sprite));
    }

    private void PlayAnimation(Entity<PsychicCritComponent, SpriteComponent> ent)
    {
        var (_, crit, sprite) = ent;

        var animation = _stun.GetFatigueAnimation(
            sprite,
            Frequency,
            Jitters,
            new Vector2(0.012f, 0.012f),
            new Vector2(0.03f, 0.03f),
            breathing: 0f,
            crit.StartOffset,
            ref crit.LastJitter);

        _animation.Play(ent.Owner, animation, AnimationKey);
    }
}
