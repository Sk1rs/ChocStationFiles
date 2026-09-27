using System.Linq;
using System.Threading.Tasks;
using Content.Server.DeadSpace.Languages;
using Content.Shared._Sunrise.TTS;
using Content.Shared.DeadSpace.Languages.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server._Sunrise.TTS;

// DS14-Languages: listeners who don't know the spoken language must not hear the real text through TTS.
public sealed partial class TTSSystem
{
    [Dependency] private readonly LanguageSystem _language = default!;

    /// <summary>
    ///     Splits <paramref name="sessions"/> into those who understand <paramref name="languageId"/> and those who don't.
    ///     Everyone understands when no language is set.
    /// </summary>
    private (List<ICommonSession> Known, List<ICommonSession> Unknown) SplitByLanguage(
        IEnumerable<ICommonSession> sessions,
        string languageId)
    {
        var known = new List<ICommonSession>();
        var unknown = new List<ICommonSession>();

        foreach (var session in sessions)
        {
            if (string.IsNullOrEmpty(languageId)
                || session.AttachedEntity is not { } listener
                || _language.KnowsLanguage(listener, languageId))
            {
                known.Add(session);
            }
            else
            {
                unknown.Add(session);
            }
        }

        return (known, unknown);
    }

    /// <summary>
    ///     Makes <paramref name="recipients"/> hear gibberish instead of <paramref name="lexiconMessage"/>'s original:
    ///     a voiced version of the gibberish if the language allows it, otherwise the language's own sound (barks, clicks...).
    /// </summary>
    private async Task SendLexicon(
        EntityUid? source,
        string lexiconMessage,
        string languageId,
        TTSVoicePrototype voicePrototype,
        string? effect,
        Filter recipients,
        Func<byte[], PlayTTSEvent> makeEvent,
        AudioParams? soundParams = null)
    {
        if (!recipients.Recipients.Any() || !_prototypeManager.TryIndex<LanguagePrototype>(languageId, out var language))
            return;

        if (language.GenerateTTSForLexicon)
        {
            var sanitize = new TTSSanitizeEvent(lexiconMessage);
            if (source != null)
                RaiseLocalEvent(source.Value, sanitize);

            var lexiconData = await GenerateTTS(sanitize.Text, voicePrototype, effect);
            if (lexiconData != null)
                RaiseNetworkEvent(makeEvent(lexiconData), recipients);

            return;
        }

        if (language.LexiconSound == null)
            return;

        if (source != null)
            _audioSystem.PlayEntity(language.LexiconSound, recipients, source.Value, false, soundParams);
        else
            _audioSystem.PlayGlobal(language.LexiconSound, recipients, false, soundParams);
    }
}
