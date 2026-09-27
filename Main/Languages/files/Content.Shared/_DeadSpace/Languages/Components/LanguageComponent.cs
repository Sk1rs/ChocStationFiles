// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Languages.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Languages.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LanguageComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<LanguagePrototype>> KnownLanguages = new();

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<LanguagePrototype>> CantSpeakLanguages = new();

    /// <summary>
    ///     Языки, требующие разблокировки для возможности выбора после получения разума в EntityEffectEvent.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<LanguagePrototype>> UnlockLanguagesAfterMakeSentient = new();

    [DataField]
    public string SelectedLanguage = String.Empty;
    public void CopyFrom(LanguageComponent other)
    {
        // Sunrise: copy the sets, not the references, so the two bodies don't share (and mutate) one list.
        KnownLanguages = new(other.KnownLanguages);
        CantSpeakLanguages = new(other.CantSpeakLanguages);
        UnlockLanguagesAfterMakeSentient = new(other.UnlockLanguagesAfterMakeSentient);
        SelectedLanguage = other.SelectedLanguage;
    }

}

[Serializable, NetSerializable]
public sealed class LanguageComponentState : ComponentState
{
    public readonly HashSet<ProtoId<LanguagePrototype>> KnownLanguages = new();
    public readonly HashSet<ProtoId<LanguagePrototype>> CantSpeakLanguages = new();
    public readonly string SelectedLanguage; // Sunrise: so the client menu can show which language is active
    public LanguageComponentState(HashSet<ProtoId<LanguagePrototype>> knownLanguages, HashSet<ProtoId<LanguagePrototype>> cantSpeakLanguages, string selectedLanguage)
    {
        KnownLanguages = knownLanguages;
        CantSpeakLanguages = cantSpeakLanguages;
        SelectedLanguage = selectedLanguage;
    }
}
