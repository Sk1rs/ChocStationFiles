ланг (фульминат ртути фтл) : Lust Station\Resources\Prototypes\Entities\Objects\Misc

путь : Resources\Prototypes\Reagents
пиротехник yml (который не в подпапке) идёт вместе с нефтью, так что если хочешь без неё вот код :
- type: reagent
  id: FulminatedMercury
  name: reagent-name-fulminated-mercury
  group: Elements
  desc: reagent-desc-fulminated-mercury
  physicalDesc: reagent-physical-desc-crystalline
  flavor: metallic
  color: "#D3D3D3"
  meltingPoint: -38.83
  boilingPoint: 356.73
  metabolisms:
    Poison:
      effects:
      - !type:HealthChange
        damage:
          types:
            Poison: 7

файл фульминат ртути (юмл) икидать: Lust Station\Resources\Prototypes\Entities\Objects\Misc

пиротехник фтл кидать: Lust Station\Resources\Locale\ru-RU\_strings\reagents\meta
та же тема что и с Yml пиротехникой (то есть там тоже идёт нефть вместе с файлом):

reagent-name-fulminated-mercury = фульминат ртути
reagent-desc-fulminated-mercury = Нестабильное вещество которое в любой момент готово взорваться.

Пиротехник ФТЛ (который в подпапке, это рецепты пиротехники) кидать : \Resources\Prototypes\Recipes\Reactions
там тоже идёт нефть так что вот код:
- type: reaction
  id: FulminatedMercury
  reactants:
    DexalinPlus:
      amount: 1
    Bicaridine:
      amount: 1
    Nitrogen:
      amount: 1  
    Mercury:
      amount: 1
  products:
    FulminatedMercury: 2

- type: reaction
  id: FulminatedMercuryCrystal
  reactants:
    FulminatedMercury:
      amount: 20
    Frezon:
      amount: 1
      catalyst: true 
  effects:
    - !type:SpawnEntity
      entity: FulminatedMercuryCrystal
