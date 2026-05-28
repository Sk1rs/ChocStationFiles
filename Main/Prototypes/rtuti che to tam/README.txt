ланг : Lust Station\Resources\Prototypes\Entities\Objects\Misc

пиротехник yml идёт вместе с нефтью, так что если хочешь без неё вот код :
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
 
и путь: Lust Station\Resources\Prototypes\Entities\Objects\Misc

файл фульминат ртути икидать: Lust Station\Resources\Prototypes\Entities\Objects\Misc

пиротехник фтл кидать: Lust Station\Resources\Locale\ru-RU\_strings\reagents\meta
та же тема что и с Yml пиротехникой

reagent-name-fulminated-mercury = фульминат ртути
reagent-desc-fulminated-mercury = Нестабильное вещество которое в любой момент готово взорваться.