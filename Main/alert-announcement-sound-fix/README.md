
## Что меняет патч

- `Content.Server/_Sunrise/AnnouncementSpeaker/AnnouncementSpeakerSystem.cs` - звук теперь играет всегда, TTS-голос генерируется только как опция поверх него.
- `Content.Server/_Sunrise/TTS/TTSSystem.cs` - если рабочих колонок на станции нет, звук проигрывается всем глобально вместо тишины.

## Как применить

```bash
cd путь/к/триждыебаному/репозиторию
git apply alert-announcement-sound-fix.patch
```

Или ручками - открыть оба файла выше и повторить изменения из патча если ты тупой.

## После применения

Билд в конфигурации **Release**

```bash
dotnet build SpaceStation14.slnx -c Release
```
