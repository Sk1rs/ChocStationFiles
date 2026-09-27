# Правки существующих файлов

Всё, что поменялось в уже существующих файлах Sunrise. Каждая правка помечена `DS14-Languages`
(`// DS14-Languages`, `# DS14-Languages`, `<!-- DS14-Languages -->`, блоки `-start` / `-end`).
Вокруг - пара строк без метки, чтобы было видно, куда вставлять.

Прототипы сущностей (54 шт., `- type: Language`) - в [entities.md](entities.md).

## `Content.Shared/Chat/SharedChatEvents.cs`

EntitySpokeEvent: + LexiconMessage и LanguageId (опциональные - старый код не ломается)

Строка ~67:
```csharp
    public RadioChannelPrototype? Channel;

    // DS14-Languages-start
    /// <summary>
    /// The message as heard by someone who does not know <see cref="LanguageId"/>. Equals <see cref="Message"/> when no language is set.
    /// </summary>
    public readonly string LexiconMessage;

    /// <summary>
    /// The language the entity spoke in, or empty if it has none.
    /// </summary>
    public readonly string LanguageId;
    // DS14-Languages-end

    public EntitySpokeEvent(EntityUid source, string message, RadioChannelPrototype? channel, string? obfuscatedMessage,
        string? lexiconMessage = null, string? languageId = null) // DS14-Languages
    {
        Source = source;
```

Строка ~86:
```csharp
        Channel = channel;
        ObfuscatedMessage = obfuscatedMessage;
        LexiconMessage = lexiconMessage ?? message; // DS14-Languages
        LanguageId = languageId ?? string.Empty; // DS14-Languages
    }
}
```

## `Content.Server/Chat/Systems/ChatSystem.cs`

речь, шёпот, SendInVoiceRange, RadioSpokeEvent

Строка ~42:
```csharp
using Robust.Shared.Utility;
using Content.Shared._Sunrise.TTS;
using Content.Server.DeadSpace.Languages; // DS14-Languages
using Content.Shared.DeadSpace.Languages.Components; // DS14-Languages

namespace Content.Server.Chat.Systems;
```

Строка ~71:
```csharp
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly AnnouncementSpeakerSystem _announcementSpeaker = default!;
    [Dependency] private readonly LanguageSystem _language = default!; // DS14-Languages

    public const string DefaultAnnouncementSound = "/Audio/_Sunrise/Announcements/announce_dig.ogg"; // Sunrise-edit
```

Строка ~627:
```csharp
        if (HasComp<CanFormatMessagesComponent>(source)) isFormatted = true; //sunrise-edit

        // DS14-Languages-start
        var verb = Loc.GetString(_random.Pick(speech.SpeechVerbStrings));
        var languageId = GetSpokenLanguage(source);
        var hasLanguage = languageId != string.Empty;
        var sayKey = speech.Bold ? "chat-manager-entity-say-bold-wrap-message" : "chat-manager-entity-say-wrap-message";

        // Listeners who know the language get the real text, tagged with the language name.
        var wrappedMessage = Loc.GetString(hasLanguage ? sayKey + "-lang" : sayKey,
            ("entityName", name),
            ("verb", verb),
            ("language", _language.GetLangName(languageId)),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("message", isFormatted ? message : FormattedMessage.EscapeText(message))); //sunrise-edit

        // Everyone else gets the same line in gibberish, without the language tag.
        var lexiconMessage = hasLanguage ? _language.TransformWord(message, languageId) : message;
        var lexiconWrappedMessage = Loc.GetString(sayKey,
            ("entityName", name),
            ("verb", verb),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("message", FormattedMessage.EscapeText(lexiconMessage)));

        SendInVoiceRange(ChatChannel.Local, message, wrappedMessage, source, range,
            lexiconMessage: lexiconMessage, lexiconWrappedMessage: lexiconWrappedMessage, languageId: languageId);

        var ev = new EntitySpokeEvent(source, message, null, null, lexiconMessage, languageId);
        RaiseLocalEvent(source, ev, true);
        // DS14-Languages-end

        // To avoid logging any messages sent by entities that are not players, like vendors, cloning, etc.
```

Строка ~718:
```csharp
        if (HasComp<CanFormatMessagesComponent>(source)) isFormatted = true; //sunrise-edit

        // DS14-Languages-start
        var languageId = GetSpokenLanguage(source);
        var hasLanguage = languageId != string.Empty;
        var whisperKey = hasLanguage ? "chat-manager-entity-whisper-wrap-message-lang" : "chat-manager-entity-whisper-wrap-message";
        var langName = _language.GetLangName(languageId);
        // DS14-Languages-end

        var wrappedMessage = Loc.GetString(whisperKey, ("language", langName), // DS14-Languages
            ("entityName", name), ("message", isFormatted ? message : FormattedMessage.EscapeText(message))); //sunrise-edit

        var wrappedobfuscatedMessage = Loc.GetString(whisperKey, ("language", langName), // DS14-Languages
            ("entityName", nameIdentity), ("message", isFormatted ? obfuscatedMessage : FormattedMessage.EscapeText(obfuscatedMessage))); //sunrise-edit

        var wrappedUnknownMessage = Loc.GetString("chat-manager-entity-whisper-unknown-wrap-message",
            ("message", isFormatted ? obfuscatedMessage : FormattedMessage.EscapeText(obfuscatedMessage))); //sunrise-edit

        // DS14-Languages-start: the same three variants in gibberish, for listeners who do not know the language.
        var lexiconMessage = hasLanguage ? _language.TransformWord(message, languageId) : message;
        var lexiconObfuscatedMessage = ObfuscateMessageReadability(lexiconMessage, 0.2f);
        var lexiconWrappedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message",
            ("entityName", name), ("message", FormattedMessage.EscapeText(lexiconMessage)));
        var lexiconWrappedObfuscatedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message",
            ("entityName", nameIdentity), ("message", FormattedMessage.EscapeText(lexiconObfuscatedMessage)));
        var lexiconWrappedUnknownMessage = Loc.GetString("chat-manager-entity-whisper-unknown-wrap-message",
            ("message", FormattedMessage.EscapeText(lexiconObfuscatedMessage)));
        // DS14-Languages-end


```

Строка ~757:
```csharp
                continue; // Won't get logged to chat, and ghosts are too far away to see the pop-up, so we just won't send it to them.

            // DS14-Languages-start
            if (hasLanguage && !_language.KnowsLanguage(listener, languageId))
            {
                if (data.Range <= WhisperClearRange || data.Observer)
                    _chatManager.ChatMessageToOne(ChatChannel.Whisper, lexiconMessage, lexiconWrappedMessage, source, false, session.Channel);
                else if (_examineSystem.InRangeUnOccluded(source, listener, WhisperMuffledRange))
                    _chatManager.ChatMessageToOne(ChatChannel.Whisper, lexiconObfuscatedMessage, lexiconWrappedObfuscatedMessage, source, false, session.Channel);
                else
                    _chatManager.ChatMessageToOne(ChatChannel.Whisper, lexiconObfuscatedMessage, lexiconWrappedUnknownMessage, source, false, session.Channel);
                continue;
            }
            // DS14-Languages-end

            if (data.Range <= WhisperClearRange || data.Observer)
```

Строка ~782:
```csharp
        _replay.RecordServerMessage(new ChatMessage(ChatChannel.Whisper, message, wrappedMessage, GetNetEntity(source), null, MessageRangeHideChatForReplay(range)));

        var ev = new EntitySpokeEvent(source, message, channel, obfuscatedMessage, lexiconMessage, languageId); // DS14-Languages
        RaiseLocalEvent(source, ev, true);
        if (!hideLog)
```

Строка ~1003:
```csharp
    }

    // DS14-Languages-start
    /// <summary>
    ///     The language <paramref name="source"/> is currently speaking, or empty if it has none.
    /// </summary>
    private string GetSpokenLanguage(EntityUid source)
    {
        return TryComp<LanguageComponent>(source, out var language) ? language.SelectedLanguage : string.Empty;
    }
    // DS14-Languages-end

    /// <summary>
```

Строка ~1017:
```csharp
    /// </summary>
    public void SendInVoiceRange(ChatChannel channel, string message, string wrappedMessage, EntityUid source, ChatTransmitRange range, NetUserId? author = null, Color? color = null,
        string? lexiconMessage = null, string? lexiconWrappedMessage = null, string? languageId = null) // DS14-Languages
    {
        foreach (var (session, data) in GetRecipients(source, VoiceRange))
```

Строка ~1026:
```csharp
            var entHideChat = entRange == MessageRangeCheckResult.HideChat;

            // DS14-Languages-start
            if (lexiconMessage != null && lexiconWrappedMessage != null && !string.IsNullOrEmpty(languageId)
                && session.AttachedEntity is { } listener && !_language.KnowsLanguage(listener, languageId))
            {
                _chatManager.ChatMessageToOne(channel, lexiconMessage, lexiconWrappedMessage, source, entHideChat, session.Channel, author: author, colorOverride: color);
                continue;
            }
            // DS14-Languages-end

            _chatManager.ChatMessageToOne(channel, message, wrappedMessage, source, entHideChat, session.Channel, author: author, colorOverride: color);
```

Строка ~1235:
```csharp
// Sunrise-TTS-Start
public sealed class RadioSpokeEvent(EntityUid source, string message, EntityUid[] receivers,
    string? lexiconMessage = null, string? languageId = null) : EntityEventArgs // DS14-Languages
{
    public readonly EntityUid Source = source;
    public readonly string Message = message;
    public readonly EntityUid[] Receivers = receivers;
    public readonly string LexiconMessage = lexiconMessage ?? message; // DS14-Languages
    public readonly string LanguageId = languageId ?? string.Empty; // DS14-Languages
}

```

## `Content.Server/Radio/RadioEvent.cs`

RadioReceiveEvent: + LexiconChatMsg, LanguageId, GetChatMsgFor

Строка ~6:
```csharp
[ByRefEvent]
public readonly record struct RadioReceiveEvent(string Message, EntityUid MessageSource, RadioChannelPrototype Channel, EntityUid RadioSource, MsgChatMessage ChatMsg, List<EntityUid> Receivers, // Sunrise-TTS
    MsgChatMessage? LexiconChatMsg = null, string? LanguageId = null) // DS14-Languages: what listeners who don't know LanguageId receive instead of ChatMsg
{
    // DS14-Languages-start
    /// <summary>
    ///     The chat message <paramref name="listener"/> should get: the real one, or gibberish if they don't know the language.
    /// </summary>
    public MsgChatMessage GetChatMsgFor(EntityUid listener, Content.Server.DeadSpace.Languages.LanguageSystem language)
    {
        if (LexiconChatMsg == null || string.IsNullOrEmpty(LanguageId) || language.KnowsLanguage(listener, LanguageId))
            return ChatMsg;

        return LexiconChatMsg;
    }
    // DS14-Languages-end
}

```

## `Content.Server/Radio/EntitySystems/RadioSystem.cs`

сборка «понятной» и «тарабарской» версии радиосообщения

Строка ~22:
```csharp
using Robust.Shared.Replays;
using Robust.Shared.Utility;
using Content.Server.DeadSpace.Languages; // DS14-Languages
using Content.Shared.DeadSpace.Languages.Components; // DS14-Languages

namespace Content.Server.Radio.EntitySystems;
```

Строка ~39:
```csharp
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly LanguageSystem _language = default!; // DS14-Languages

    // set used to prevent radio feedback loops.
```

Строка ~75:
```csharp
        if (TryComp(uid, out ActorComponent? actor))
        {
            _netMan.ServerSendMessage(args.GetChatMsgFor(uid, _language), actor.PlayerSession.Channel); // DS14-Languages
            if (uid != args.MessageSource && HasComp<TTSComponent>(args.MessageSource))
            {
```

Строка ~147:
```csharp
        // Sunrise-End

        // DS14-Languages-start
        var verb = Loc.GetString(_random.Pick(speech.SpeechVerbStrings));
        var languageId = TryComp<LanguageComponent>(messageSource, out var language) ? language.SelectedLanguage : string.Empty;
        var hasLanguage = languageId != string.Empty;
        var radioKey = speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap";
        // DS14-Languages-end

        var wrappedMessage = Loc.GetString(hasLanguage ? radioKey + "-lang" : radioKey, // DS14-Languages
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("verb", verb), // DS14-Languages
            ("language", _language.GetLangName(languageId)), // DS14-Languages
            ("channel", $"\\[{channel.LocalizedName}\\]"), // Sunrise-Edit
            ("name", formattedName),
            ("message", content));

        // most radios are relayed to chat, so lets parse the chat message beforehand
```

Строка ~173:
```csharp
        var chatMsg = new MsgChatMessage { Message = chat };

        // DS14-Languages-start: the gibberish variant for receivers who don't know the language.
        var lexiconMessage = hasLanguage ? _language.TransformWord(message, languageId) : message;
        MsgChatMessage? lexiconChatMsg = null;
        if (hasLanguage)
        {
            var lexiconContent = FormattedMessage.EscapeText(lexiconMessage);
            if (GetIdCardIsBold(messageSource))
                lexiconContent = $"[bold]{lexiconContent}[/bold]";

            var lexiconWrappedMessage = Loc.GetString(radioKey,
                ("color", channel.Color),
                ("fontType", speech.FontId),
                ("fontSize", speech.FontSize),
                ("verb", verb),
                ("channel", $"\\[{channel.LocalizedName}\\]"),
                ("name", formattedName),
                ("message", lexiconContent));

            lexiconChatMsg = new MsgChatMessage
            {
                Message = new ChatMessage(ChatChannel.Radio, lexiconMessage, lexiconWrappedMessage, NetEntity.Invalid, null),
            };
        }

        var ev = new RadioReceiveEvent(message, messageSource, channel, radioSource, chatMsg, [], lexiconChatMsg, languageId);
        // DS14-Languages-end

        var sendAttemptEv = new RadioSendAttemptEvent(channel, radioSource);
```

Строка ~238:
```csharp
        }

        RaiseLocalEvent(new RadioSpokeEvent(messageSource, FormattedMessage.RemoveMarkupPermissive(message), ev.Receivers.ToArray(), lexiconMessage, languageId)); // Sunrise-TTS, DS14-Languages

        if (name != Name(messageSource))
```

## `Content.Server/Radio/EntitySystems/HeadsetSystem.cs`

гарнитура отдаёт носителю нужную версию

Строка ~7:
```csharp
using Robust.Shared.Network;
using Robust.Shared.Player;
using Content.Server.DeadSpace.Languages; // DS14-Languages

namespace Content.Server.Radio.EntitySystems;
```

Строка ~15:
```csharp
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly LanguageSystem _language = default!; // DS14-Languages

    public override void Initialize()
```

Строка ~115:
```csharp
        if (TryComp(parent, out ActorComponent? actor))
        {
            _netMan.ServerSendMessage(args.GetChatMsgFor(parent, _language), actor.PlayerSession.Channel); // DS14-Languages
            if (parent != args.MessageSource && HasComp<TTSComponent>(args.MessageSource))
            {
```

## `Content.Server/Radio/EntitySystems/RadioDeviceSystem.cs`

динамики раций/интеркомов повторяют на языке говорящего

Строка ~14:
```csharp
using Content.Shared.Speech.Components;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Languages.Components; // DS14-Languages

namespace Content.Server.Radio.EntitySystems;
```

Строка ~182:
```csharp
            ("originalName", nameEv.VoiceName));

        // DS14-Languages-start: the speaker repeats the message in whatever language it was spoken in,
        // so bystanders who don't know it hear gibberish just like everyone else.
        if (TryComp<LanguageComponent>(args.MessageSource, out var sourceLanguage))
            EnsureComp<LanguageComponent>(uid).SelectedLanguage = sourceLanguage.SelectedLanguage;
        else if (TryComp<LanguageComponent>(uid, out var speakerLanguage))
            speakerLanguage.SelectedLanguage = string.Empty;
        // DS14-Languages-end

        // log to chat so people can identity the speaker/source, but avoid clogging ghost chat if there are many radios
```

## `Content.Server/_Sunrise/TTS/TTSSystem.cs`

TTS: знающим - голос, остальным - тарабарщина или звук языка

Строка ~127:
```csharp
        var message = accentEvent.Text;

        HandleRadio(args.Receivers, message, protoVoice, voiceEv.Effect, args.Source, args.LexiconMessage, args.LanguageId); // DS14-Languages
    }

```

Строка ~278:
```csharp
        if (args.ObfuscatedMessage != null)
        {
            HandleWhisper(uid, message, protoVoice, args.LexiconMessage, args.LanguageId); // DS14-Languages
            return;
        }

        HandleSay(uid, message, protoVoice, voiceEv.Effect, args.LexiconMessage, args.LanguageId); // DS14-Languages
    }

    private async void HandleSay(EntityUid uid, string message, TTSVoicePrototype voicePrototype, string? effect,
        string lexiconMessage = "", string languageId = "") // DS14-Languages
    {
        var recipients = Filter.Pvs(uid, 1F).RemovePlayers(_ignoredRecipients);
```

Строка ~296:
```csharp
        var netEntity = GetNetEntity(uid);

        // DS14-Languages-start
        var (known, unknown) = SplitByLanguage(recipients.Recipients, languageId);

        await SendLexicon(uid, lexiconMessage, languageId, voicePrototype, effect,
            Filter.Empty().AddPlayers(unknown), data => new PlayTTSEvent(data, netEntity));

        if (known.Count == 0)
            return;
        // DS14-Languages-end

        var soundData = await GenerateTTS(message, voicePrototype, effect);
```

Строка ~311:
```csharp
            return;

        RaiseNetworkEvent(new PlayTTSEvent(soundData, netEntity), Filter.Empty().AddPlayers(known)); // DS14-Languages
    }

    private async void HandleWhisper(EntityUid uid, string message, TTSVoicePrototype voicePrototype,
        string lexiconMessage = "", string languageId = "") // DS14-Languages
    {
        // DS14-Languages-start: whisperers in range who don't know the language get the gibberish variant.
        var (_, unknown) = SplitByLanguage(Filter.Pvs(uid).Recipients, languageId);
        var unknownInRange = new List<ICommonSession>();
        var whisperQuery = GetEntityQuery<TransformComponent>();
        var whisperPos = _xforms.GetWorldPosition(whisperQuery.GetComponent(uid), whisperQuery);
        foreach (var session in unknown)
        {
            if (_ignoredRecipients.Contains(session) || session.AttachedEntity is not { } listener)
                continue;

            var distance = (whisperPos - _xforms.GetWorldPosition(whisperQuery.GetComponent(listener), whisperQuery)).LengthSquared();
            if (distance <= WhisperVoiceRange)
                unknownInRange.Add(session);
        }

        var whisperNet = GetNetEntity(uid);
        await SendLexicon(uid, lexiconMessage, languageId, voicePrototype, null,
            Filter.Empty().AddPlayers(unknownInRange),
            data => new PlayTTSEvent(data, whisperNet, false, WhisperVoiceVolumeModifier),
            AudioParams.Default.WithVolume(-4f));
        // DS14-Languages-end

        // If it's a whisper into a radio, generate speech without whisper
```

Строка ~357:
```csharp
                return;

            // DS14-Languages: already handled above.
            if (unknown.Contains(session))
                continue;
```

Строка ~377:
```csharp

    private async void HandleRadio(EntityUid[] uids, string message, TTSVoicePrototype voicePrototype, string? effect = null,
        EntityUid? source = null, string lexiconMessage = "", string languageId = "") // DS14-Languages
    {
        // DS14-Languages-start
        var recipients = Filter.Entities(uids).RemovePlayers(_ignoredRecipients);
        var (known, unknown) = SplitByLanguage(recipients.Recipients, languageId);

        // Radio is heard "in the ear", so the language sound plays globally rather than at the speaker.
        await SendLexicon(null, lexiconMessage, languageId, voicePrototype, _radioEffect,
            Filter.Empty().AddPlayers(unknown), data => new PlayTTSEvent(data, null, true));

        if (known.Count == 0)
            return;
        // DS14-Languages-end

        var soundData = await GenerateTTS(message, voicePrototype, _radioEffect);
```

Строка ~395:
```csharp
            return;

        RaiseNetworkEvent(new PlayTTSEvent(soundData, null, true), Filter.Empty().AddPlayers(known)); // DS14-Languages
    }

```

## `Content.Server/Zombies/ZombieSystem.Transform.cs`

зомби забывают всё кроме зомбячьего

Строка ~48:
```csharp
using Content.Shared.Roles;
using Content.Shared.Temperature.Components;
using Content.Server.DeadSpace.Languages; // DS14-Languages
using Content.Shared.DeadSpace.Languages.Prototypes; // DS14-Languages

namespace Content.Server.Zombies;
```

Строка ~77:
```csharp
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly LanguageSystem _language = default!; // DS14-Languages

    private static readonly ProtoId<TagPrototype> InvalidForGlobalSpawnSpellTag = "InvalidForGlobalSpawnSpell";
    private static readonly ProtoId<TagPrototype> CannotSuicideTag = "CannotSuicide";
    private static readonly ProtoId<LanguagePrototype> ZombieLanguage = "ZombieLanguage"; // DS14-Languages
    private static readonly ProtoId<NpcFactionPrototype> ZombieFaction = "Zombie";
    private static readonly string MindRoleZombie = "MindRoleZombie";
```

Строка ~159:
```csharp
        EnsureComp<ReplacementAccentComponent>(target).Accent = accentType;

        // DS14-Languages: zombies forget everything they knew and only speak zombie.
        _language.SetExclusiveLanguage(target, ZombieLanguage);

```

## `Content.Server/EntityEffects/Effects/MakeSentientEntityEffectSystem.cs`

когнизин учит общему

Строка ~4:
```csharp
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Mind.Components;
using Content.Server.DeadSpace.Languages; // DS14-Languages
using Content.Shared.DeadSpace.Languages.Components; // DS14-Languages

namespace Content.Server.EntityEffects.Effects;
```

Строка ~27:
```csharp
            RemComp<MonkeyAccentComponent>(entity);

            // DS14-Languages-start: a newly sentient creature learns galactic common and unlocks its locked languages.
            var language = EnsureComp<LanguageComponent>(entity);
            language.KnownLanguages.Add(LanguageSystem.DefaultLanguageId);
            language.KnownLanguages.UnionWith(language.UnlockLanguagesAfterMakeSentient);
            language.CantSpeakLanguages.ExceptWith(language.UnlockLanguagesAfterMakeSentient);
            language.CantSpeakLanguages.Remove(LanguageSystem.DefaultLanguageId);

            if (string.IsNullOrEmpty(language.SelectedLanguage))
                language.SelectedLanguage = LanguageSystem.DefaultLanguageId;

            Dirty(entity, language);
            // DS14-Languages-end
        }

```

## `Content.Shared/Input/ContentKeyFunctions.cs`

клавиша OpenLanguageMenu

Строка ~29:
```csharp
        public static readonly BoundKeyFunction OpenEmotesMenu = "OpenEmotesMenu";
        public static readonly BoundKeyFunction OfferItem = "OfferItem"; // Corvax-Wega-Offer
        public static readonly BoundKeyFunction OpenLanguageMenu = "OpenLanguageMenu"; // DS14-Languages
        public static readonly BoundKeyFunction OpenCraftingMenu = "OpenCraftingMenu";
        public static readonly BoundKeyFunction OpenGuidebook = "OpenGuidebook";
```

## `Content.Client/Input/ContentContexts.cs`

клавиша в контексте human

Строка ~73:
```csharp
            human.AddFunction(ContentKeyFunctions.OpenEmotesMenu);
            human.AddFunction(ContentKeyFunctions.OfferItem); // Corvax-Wega-Offer
            human.AddFunction(ContentKeyFunctions.OpenLanguageMenu); // DS14-Languages
            human.AddFunction(ContentKeyFunctions.ActivateItemInWorld);
            human.AddFunction(ContentKeyFunctions.ThrowItemInHand);
```

## `Content.Client/Options/UI/Tabs/KeyRebindTab.xaml.cs`

кнопка в настройках управления

Строка ~210:
```csharp
            AddButton(ContentKeyFunctions.SaveItemLocation);
            AddButton(ContentKeyFunctions.OfferItem); // Corvax-Wega-Offer
            AddButton(ContentKeyFunctions.OpenLanguageMenu); // DS14-Languages

            AddHeader("ui-options-header-interaction-adv");
```

## `Content.Client/UserInterface/Systems/MenuBar/Widgets/GameTopMenuBar.xaml`

кнопка на верхней панели

Строка ~54:
```xml
        AppendStyleClass="{x:Static style:StyleClass.ButtonSquare}"
        />
    <!-- DS14-Languages-start -->
    <ui:MenuButton
        Name="LanguageButton"
        Access="Internal"
        Icon="{xe:Tex '/Textures/_DeadSpace/Interface/language-translation-svgrepo.svg.192dpi.png'}"
        ToolTip="{Loc 'game-hud-open-language-menu-button-tooltip'}"
        BoundKey = "{x:Static is:ContentKeyFunctions.OpenLanguageMenu}"
        MinSize="42 64"
        HorizontalExpand="True"
        AppendStyleClass="{x:Static style:StyleClass.ButtonSquare}"
        />
    <!-- DS14-Languages-end -->
    <ui:MenuButton
        Name="CraftingButton"
```

## `Content.Client/UserInterface/Systems/MenuBar/GameTopMenuBarUIController.cs`

подключение кнопки

Строка ~10:
```csharp
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Client.UserInterface.Systems.Sandbox;
using Content.Client.DeadSpace.Languages; // DS14-Languages
using Robust.Client.UserInterface.Controllers;

```

Строка ~26:
```csharp
    [Dependency] private readonly GuidebookUIController _guidebook = default!;
    [Dependency] private readonly EmotesUIController _emotes = default!;
    [Dependency] private readonly LanguageUIController _language = default!; // DS14-Languages

    private GameTopMenuBar? GameTopMenuBar => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>();
```

Строка ~50:
```csharp
        _sandbox.UnloadButton();
        _emotes.UnloadButton();
        _language.UnloadButton(); // DS14-Languages
    }

```

Строка ~64:
```csharp
        _sandbox.LoadButton();
        _emotes.LoadButton();
        _language.LoadButton(); // DS14-Languages
    }
}
```

## `Resources/keybinds.yml`

L по умолчанию

Строка ~199:
```yaml
  type: State
  key: J
# DS14-Languages
- function: OpenLanguageMenu
  type: State
  key: L
- function: TextCursorSelect
  # TextCursorSelect HAS to be above ExamineEntity
```

## `Resources/Prototypes/Entities/Structures/Machines/lathe.yml`

пак имплантов в протолат

Строка ~203:
```yaml
    - SurgeryStatic
    dynamicPacks:
    - LanguageImplanters # DS14-Languages
    - AdvancedTools
    - ScienceEquipment
```
