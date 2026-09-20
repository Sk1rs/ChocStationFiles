namespace Content.Server._Sunrise.Sanity;

/// <summary>
/// Marks an entity as a hallucination clone: only visible/interactable by <see cref="Owner"/>,
/// disappears (with a department-flavoured sound) on any interaction.
/// </summary>
[RegisterComponent]
public sealed partial class HallucinationClonedComponent : Component
{
    /// <summary>
    /// The player hallucinating this clone. Only their eye can see it.
    /// </summary>
    [DataField]
    public EntityUid HallucinatingPlayer;

    /// <summary>
    /// The real crewmember this clone is impersonating. Whatever they actually say out loud
    /// is echoed live by this clone too - see HallucinationSystem.OnEntitySpoke.
    /// </summary>
    [DataField]
    public EntityUid Source;

    /// <summary>
    /// Department of the crewmember this clone is impersonating - picks the dismissal sound.
    /// </summary>
    [DataField]
    public string? DepartmentId;
}
