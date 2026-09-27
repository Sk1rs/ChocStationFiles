# Языки — техническое описание

У каждого персонажа есть набор языков, которые он **знает**, и один **выбранный**, на котором он говорит.
Кто язык знает — видит реплику как есть, с подписью языка. Кто не знает — видит тарабарщину
(«ка'ши ра то», «Гав!», «0110101…») и слышит вместо голоса озвученную тарабарщину или звук языка (лай, мяуканье, щелчки).

Перенесено с **Dead Space 14** ([dead-space-server/dead-space-14](https://github.com/dead-space-server/dead-space-14), PR #629 и последующие),
метка в коде — `DS14-Languages`.

---

## Для игрока

- Кнопка «перевод» на верхней панели или **L** → радиальное меню языков. Текущий помечен «(выбран)».
- Язык действует везде: речь, шёпот, радио, динамики раций и интеркомов, TTS.
- Призраки и админ-призраки понимают всё (у них нет компонента языка).

| Кто | Знает |
|---|---|
| все расы (`BaseMobSpecies`) | Обще-галактический |
| унатхи, кобольды | + Синта'Унати |
| нианы (моли), мотыльковые тараканы | + Ткачий |
| арахниды, пауки | + Це'хисс |
| дионы, нимфы | + Шелестящий |
| слаймолюди, слаймы | + Пузырчатый |
| воксы | + Вокс-птичий |
| вульпканины | + Канилунц |
| фелиниды, таяры, кошки-питомцы | + Кошачий |
| демоны | + Демонический |
| дварфы | + Дварфийский |
| корги, Иан, МакГрифф, Уолтер | Собачий (+ общий у питомцев) |
| обезьяны | Обезьяний |
| карпы, космический дракон | Ваал'хурис |
| аргоциты | Эхоглосса |
| ИИ, **борги**, позитронный мозг, ревенант, ядро материнского корабля | почти все |
| зомби | **только** Зомбячий — при заражении всё остальное забывается |
| простые мобы (мыши, звери без своего языка) | ничего — не понимают речь, пока не обретут разум |

Резоми, свины, милира и прочие сунрайзовские расы без пары в Dead Space знают только общий.

### Как выучить язык

- **Языковые импланты** — протолат, технология «Простейшие языковые импланты» (сервис, T2, 10 000 очков).
  12 имплантеров, по одному на язык. Есть ещё админский «все языки» (`LearnAllLanguageImplanter`), в протолате его нет.
- **Когнизин** / любое «обретение разума» (`MakeSentient`) — существо учит общий и открывает свои заблокированные языки.
- **Полиморф** — языки переезжают в новое тело и обратно.

---

## Как это устроено

```
игрок говорит
  → ChatSystem: берёт LanguageComponent.SelectedLanguage у говорящего
  → LanguageSystem.TransformWord(текст, язык) → тарабарщина
  → каждому слушателю: KnowsLanguage(слушатель, язык)?
        да  → реплика как есть + «(Обще-галактический)»
        нет → тарабарщина, без подписи
  → EntitySpokeEvent (с LexiconMessage и LanguageId)
        → TTS: знающим — голос, остальным — голос тарабарщины или звук языка
```

Радио — то же самое: `RadioSystem` собирает два варианта сообщения (`ChatMsg` / `LexiconChatMsg`),
гарнитура и встроенные рации отдают получателю нужный через `RadioReceiveEvent.GetChatMsgFor`.
Динамик рации или интеркома повторяет вслух на языке говорящего.

### Режимы тарабарщины (`speechMode`)

Детерминированные: одно и то же слово на одном языке всегда превращается в одно и то же.

| Режим | Что делает | Пример |
|---|---|---|
| `Syllable` | каждое слово → `minSyllables..maxSyllables` слогов из `syllables` | общий, ткачий, синта'унати |
| `Lexicon` | **всё сообщение** → одно слово из `lexicon` | собачий («Гав!»), обезьяний |
| `Alphabet` | всё сообщение → `generateLength` символов из `alphabet` | двоичный |
| `Pattern` | регулярки `patterns[i]` → `replacements[i]` | дварфийский |

- `generateTTSForLexicon: true` — непонимающие слышат озвученную тарабарщину; `false` — звук `lexiconSound`.
- `canBeUnderstoodWithoutKnowledge: false` — язык не понимают даже существа без компонента языка (призраки и т.п.).

---

## Как добавить язык

1. Прототип в `Resources/Prototypes/_DeadSpace/Language/languages.yml`:
   ```yaml
   - type: language
     id: SkrellLanguage
     name: language-skrell-name
     icon: /Textures/_DeadSpace/LanguageIcons/default.png
     speechMode: Syllable
     minSyllables: 1
     maxSyllables: 3
     syllables: [ qr, kx, xr, vu, ka ]
     generateTTSForLexicon: false
     lexiconSound:
       collection: VoxLexicon
   ```
2. Название — в оба `Resources/Locale/{ru-RU,en-US}/_DeadSpace/languages/languages.ftl`.
3. Выдать расе:
   ```yaml
   - type: Language
     selectedLanguage: GeneralLanguage
     knownLanguages:
     - GeneralLanguage
     - SkrellLanguage
   ```
   - `cantSpeakLanguages` — понимает, но говорить не может (не будет в меню);
   - `unlockLanguagesAfterMakeSentient` — откроются после когнизина.

---

## Файлы

**Новые**

| | |
|---|---|
| `Content.Shared/_DeadSpace/Languages/` | компонент, прототип, событие выбора |
| `Content.Server/_DeadSpace/Languages/` | `LanguageSystem` (тарабарщина, понимание), импланты |
| `Content.Client/_DeadSpace/Languages/` | состояние компонента, **радиальное меню** |
| `Content.Server/_Sunrise/TTS/TTSSystem.Languages.cs` | TTS для непонимающих |
| `Resources/Prototypes/_DeadSpace/Language/` | 19 языков + звуковые коллекции |
| `Resources/Prototypes/_DeadSpace/Entities/Objects/Misc/language_impla*.yml` | импланты и имплантеры |
| `Resources/Prototypes/_DeadSpace/Recipes/Lathes/…`, `…/_DeadSpace/Research/language_implants.yml` | рецепты, пак, технология |
| `Resources/Locale/{ru-RU,en-US}/_DeadSpace/languages/languages.ftl` | названия, строки чата с подписью языка |
| `Resources/Textures/_DeadSpace/LanguageIcons/`, `…/Interface/language-translation-svgrepo*` | иконки |
| `Resources/Audio/_DeadSpace/{Languages,Voice/Felinid,Sponsor/Sound}/` | звуки языков |

**Правки** (помечены `// DS14-Languages`)

| | |
|---|---|
| `Content.Shared/Chat/SharedChatEvents.cs` | `EntitySpokeEvent` + `LexiconMessage`, `LanguageId` (опциональные — старый код не сломан) |
| `Content.Server/Chat/Systems/ChatSystem.cs` | речь, шёпот, `SendInVoiceRange`, `RadioSpokeEvent` |
| `Content.Server/Radio/RadioEvent.cs` | `RadioReceiveEvent` + `LexiconChatMsg`, `LanguageId`, `GetChatMsgFor` |
| `Content.Server/Radio/EntitySystems/{RadioSystem,HeadsetSystem,RadioDeviceSystem}.cs` | радио |
| `Content.Server/_Sunrise/TTS/TTSSystem.cs` | речь / шёпот / радио разделены по пониманию |
| `Content.Server/Zombies/ZombieSystem.Transform.cs` | зомби → зомбячий |
| `Content.Server/EntityEffects/Effects/MakeSentientEntityEffectSystem.cs` | когнизин |
| `ContentKeyFunctions.cs`, `ContentContexts.cs`, `KeyRebindTab.xaml.cs`, `Resources/keybinds.yml` | клавиша **L** |
| `MenuBar/Widgets/GameTopMenuBar.xaml`, `MenuBar/GameTopMenuBarUIController.cs` | кнопка на панели |
| `Resources/Prototypes/Entities/Structures/Machines/lathe.yml` | пак имплантов в протолат |
| 54 прототипа сущностей | `- type: Language` (размещение сверено с Dead Space скриптом по точным границам сущностей) |

---

## Отличия от Dead Space

| | Почему |
|---|---|
| Меню на `SimpleRadialMenu` | у Sunrise нет их `RadialUiController`; сделано как меню эмоций |
| Клавиша **L** по умолчанию | у них клавиши нет, только кнопка |
| Названия языков — ключи локали | у них захардкожены по-русски; теперь есть en-US |
| Клиент знает выбранный язык | у них `SelectedLanguage` не синхронизировался — в меню не видно, какой выбран |
| Реплики собираются из ключей локали | у них подменялся текст внутри готовой строки через `string.Replace` — ломается, если текст совпадает с куском разметки |
| Звук языка для непонимающих шлёт сервер | у них — через доработанный клиентский TTS; так не пришлось трогать `PlayTTSEvent` |
| Динамики раций через `EnsureComp` | у них компонент прописан вручную только на двух рациях; теперь работает и на интеркомах |
| Полиморф: обработчик по `ref`, списки копируются | у них по значению (падало бы в рантайме) и старое/новое тело делили один список |
| Кошки Runtime/Floppa/Exception говорят по-кошачьи | у них выбран собачий, которого они даже не знают |
| Нет некроморфского и тюремного языков | у нас нет некроморфов и их тюрьмы |
| Нет языка в админ-анонсах, консоли связи, телефонах, диктофоне | анонсы и так на общем; остальное — их отдельные системы |

## Известное

- `MobBandito` у Dead Space выбран собачий, но знает только общий — оставлено как у них.
- en-US строки имплантов — копия их русских, перевода у Dead Space нет.
