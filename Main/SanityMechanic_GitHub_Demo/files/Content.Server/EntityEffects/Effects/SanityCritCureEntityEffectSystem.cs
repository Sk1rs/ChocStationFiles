using Content.Server._Sunrise.Sanity;
using Content.Shared._Sunrise.Sanity;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;

namespace Content.Server.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class SanityCritCureEntityEffectSystem : EntityEffectSystem<SanityComponent, SanityCritCure>
{
    [Dependency] private readonly PsychicCritSystem _psychicCrit = default!;

    protected override void Effect(Entity<SanityComponent> entity, ref EntityEffectEvent<SanityCritCure> args)
    {
        if (HasComp<PsychicCritComponent>(entity.Owner))
            _psychicCrit.ExitPsychicCrit(entity.Owner);
    }
}
