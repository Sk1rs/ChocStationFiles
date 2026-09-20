using Content.Server._Sunrise.Sanity;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;

namespace Content.Server.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class SanityDrainEntityEffectSystem : EntityEffectSystem<SanityComponent, SanityDrain>
{
    [Dependency] private readonly SanitySystem _sanity = default!;

    protected override void Effect(Entity<SanityComponent> entity, ref EntityEffectEvent<SanityDrain> args)
    {
        _sanity.ChangeSanity(entity.Owner, entity.Comp, -args.Effect.Amount);
    }
}
