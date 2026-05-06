файл .ftl по этому пути Lust Station\Resources\Locale\ru-RU\_prototypes\entities\clothing\neck - это ланг
файл .rsi по этому пути Lust Station\Resources\Textures\Clothing\Neck\Cloaks
файл .yml по этому пути Lust Station\Resources\Prototypes\Entities\Clothing\Neck

КОД ПЛАЩА (ЭТО ТО ЧТО ВНУТРИ .yml)

- type: entity
  parent: [ClothingNeckBase, BaseCommandContraband]
  id: HoPsEliteCloak
  name: Head of Personnel's Elite Cloak
  description: A purple cloak with red shoulders and gold buttons, proving you are the gatekeeper to any airlock on the station.
  components:
  - type: Sprite
    sprite: Clothing/Neck/Cloaks/hop2.rsi
  - type: StealTarget
    stealGroup: HeadCloak   

Ланг: 

ent-HoPsEliteCloak = элитный плащ главы персонала
    .desc = Плащ выдающийся главе персонала способному писать заявление на капитанские доступы клоуну, отменять заявление на доступ в бриг главы безопасности. и гладить корги.    


Код автолата по пути : Resources\Prototypes\Recipes\Lathes\cothing.yml (кидать к другим вещам хопа)

- type: latheRecipe
  parent: BaseCommandJumpsuitRecipe
  id: ClothingNeckCloakHop
  result: ClothingNeckCloakHop

- type: latheRecipe
  parent: BaseCommandJumpsuitRecipe
  id: HoPsEliteCloak
  result: HoPsEliteCloak 

Строчку кидать к остальным вещам хопа по пути : Resources\Prototypes\Recipes\Lathes\Packs\cothing.yml

- ClothingNeckCloakHop
- HoPsEliteCloak