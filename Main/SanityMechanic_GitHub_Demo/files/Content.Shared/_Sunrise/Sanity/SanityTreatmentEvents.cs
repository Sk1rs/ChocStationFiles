using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunrise.Sanity;

public sealed partial class SanityTreatmentActionEvent : EntityTargetActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class SanityTreatmentDoAfterEvent : DoAfterEvent
{
    [DataField]
    public float HealAmount;

    public override DoAfterEvent Clone() => this;
}
