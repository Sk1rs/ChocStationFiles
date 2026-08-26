
Файл: `Content.Shared/_Sunrise/SunriseCCVars/SunriseCCVars.cs`

- `flavor_text.sponsor_only`: `true` → `false` - описание видно всем, не только спонсорам.
- `flavor_text.length`: `512` → `550` - максимальная длина описания.

## Как применить

**Вариант 1 - патч:**

```bash
cd путь/к/сука/репозиторию
git apply character-description-fix.patch
```

**Вариант 2 - руками:**
Открой сука `Content.Shared/_Sunrise/SunriseCCVars/SunriseCCVars.cs`, найди блок `FlavorTextSponsorOnly` и `FlavorTextBaseLength`, поменяй блять значения на `false` и `512`.

## После применения

Перебилд в конфигурации **Release** 


