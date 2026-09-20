namespace Content.Shared.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffect"/>
/// <remarks>
/// Used by Haloperidol/Psicodine: unlike <see cref="SanityDrain"/> (a 50/50 gamble), this
/// effect has no probability of its own - metabolizing either drug guarantees pulling the
/// entity out of a psychic crisis, if they're in one. Does nothing otherwise.
/// </remarks>
public sealed partial class SanityCritCure : EntityEffectBase<SanityCritCure>;
