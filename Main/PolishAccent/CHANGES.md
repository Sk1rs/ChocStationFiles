# Что трогаем - список файлов

Польский акцент = замена испанского. Ниже всё, что меняется, и что именно там делать.
Готовые куски лежат рядом в [`files/`](files/) - можно копировать as is.

## Легенда

- 🆕 новый файл
- ✏️ правим существующий
- 🗑️ удаляем

---

## Ресурсы (данные) - это и есть вся механика

| | Файл | Что делаем | Готовый кусок |
|---|---|---|---|
| 🆕 | `Resources/Locale/ru-RU/_strings/accent/polish.ftl` | Словарь: 47 пар «искать» → «заменить», русская сторона | [`files/Resources/Locale/ru-RU/.../polish.ftl`](files/Resources/Locale/ru-RU/_strings/accent/polish.ftl) |
| 🆕 | `Resources/Locale/en-US/_strings/accent/polish.ftl` | То же, английская сторона. **Ключи обязаны совпадать 1-в-1 с ru** | [`files/Resources/Locale/en-US/.../polish.ftl`](files/Resources/Locale/en-US/_strings/accent/polish.ftl) |
| ✏️ | `Resources/Prototypes/_Sunrise/Accents/word_replacements.yml` | В конец дописываем прототип `- type: accent / id: polish` с 47 строками | [`files/snippets/accent-prototype.yml`](files/snippets/accent-prototype.yml) |
| ✏️ | `Resources/Prototypes/Traits/speech.yml` | Блок `id: SpanishAccent` → `id: PolishAccent`. Компонент меняется: был свой `SpanishAccent`, стал `ReplacementAccent` с `accent: polish` | [`files/snippets/trait.yml`](files/snippets/trait.yml) |
| ✏️ | `Resources/Locale/ru-RU/_strings/traits/traits.ftl` | `trait-spanish-*` → `trait-polish-*` | [`files/snippets/trait-locale-ru.ftl`](files/snippets/trait-locale-ru.ftl) |
| ✏️ | `Resources/Locale/en-US/_strings/traits/traits.ftl` | То же | [`files/snippets/trait-locale-en.ftl`](files/snippets/trait-locale-en.ftl) |
| ✏️ | `Resources/Prototypes/Entities/Clothing/Head/hats.yml` | `ClothingHeadHatSombrero`: в `AddAccentClothing` меняем `replacement: spanish` → `polish` | [`files/snippets/clothing.yml`](files/snippets/clothing.yml) |
| ✏️ | `Resources/Prototypes/Entities/Clothing/OuterClothing/misc.yml` | `ClothingOuterPonchoClassic` и `ClothingOuterPoncho` - то же самое, два места | [`files/snippets/clothing.yml`](files/snippets/clothing.yml) |

---

## Код - НЕ трогаем

Своего C# у механики нет, всё едет на существующем движке. Эти файлы упомянуты
только чтобы было понятно, куда смотреть при отладке - **менять их не надо**:

| Файл | Роль |
|---|---|
| `Content.Server/Speech/EntitySystems/ReplacementAccentSystem.cs` | Применяет замены, собирает регулярки, держит кэш |
| `Content.Server/Speech/Prototypes/ReplacementAccentPrototype.cs` | Схема `- type: accent` |
| `Content.Server/Speech/Components/ReplacementAccentComponent.cs` | Вешается трейтом/шмоткой, хранит id акцента |
| `Content.Server/Speech/EntitySystems/AddAccentClothingSystem.cs` | Навешивает акцент на время ношения |

---

## Удалить (остатки испанского)

| | Файл | Почему |
|---|---|---|
| 🗑️ | `Content.Server/Speech/Components/SpanishAccentComponent.cs` | Испанскому нужен был свой компонент, польскому - нет |
| 🗑️ | `Content.Server/Speech/EntitySystems/SpanishAccentSystem.cs` | Логика «вставить `e` перед `s`» и `¿¡`. Польскому не нужна |
| 🗑️ | `Resources/Locale/*/_strings/accent/spanish.ftl` | Если был |

### ⚠️ В текущем рабочем дереве это сделано криво

Файлы **переименованы**, а не удалены, и внутри до сих пор испанский код
(класс так и называется `SpanishAccentComponent`). `git status` показывает `RD`:
в индексе они есть, на диске их нет. Если закоммитить как есть - в репу уедет
мёртвый файл с чужим именем класса.

Чинится так:

```bash
git rm --cached Content.Server/Speech/Components/PolishAccentComponent.cs Content.Server/Speech/EntitySystems/PolishAccentSystem.cs
```

---

## Ещё одно перед пушем

В `Resources/Prototypes/Accents/word_replacements.yml` (это **общий** файл, не
сунрайзовский) в конец насрано **124 строки ASCII-арта, ~37 КБ** комментариев
`#@@@@@...`. К польскому акценту отношения не имеет, полезной нагрузки ноль.
Весь diff этого файла - только арт. Убрать.

---

## Порядок применения

1. Положить два `polish.ftl`.
2. Дописать прототип `id: polish`.
3. Переписать трейт + обе строки локали трейта.
4. Поправить три места в одежде.
5. Удалить испанский код.
6. Собрать и проверить: трейт «Польский акцент» в редакторе персонажа, либо надеть сомбреро и написать в чат «я поляк, бля».
   Ожидаемо: `Jestem Polakiem, kurwa`.

Подробности по устройству и по добавлению новых слов - в [README.md](README.md).
