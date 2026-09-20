namespace Content.Shared.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffect"/>
/// <remarks>
/// Used by Haloperidol/Psicodine: metabolizing the drug carries a chance (set via
/// <see cref="EntityEffect.Probability"/> in yaml) of knocking a flat amount off sanity,
/// regardless of the drinker's current sanity tier.
/// </remarks>
public sealed partial class SanityDrain : EntityEffectBase<SanityDrain>
{
    [DataField(required: true)]
    public float Amount = 10f;
}
