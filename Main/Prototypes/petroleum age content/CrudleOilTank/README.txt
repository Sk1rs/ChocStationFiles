Танкс YML кидать по пути: Lust Station\Resources\Prototypes\Entities\Structures\Storage\Tanks
код:
#Oil

- type: entity
  id: CrudeOilTank
  parent: [StorageTank, StructureWheeled]
  name: crude oil tank
  description: description.
  suffix: Empty
  components:
  - type: StaticPrice
    price: 750
  - type: Sprite
    sprite: Structures/Storage/tanks.rsi
    layers:
      - state: oiltank-2
      - state: oiltank-2-1
        map: ["enum.SolutionContainerLayers.Fill"]
        visible: false
  - type: Appearance
  - type: SolutionContainerVisuals
    maxFillLevels: 3
    fillBaseName: oiltank-2-
  - type: ExaminableSolution
    solution: tank
  - type: ReagentTank
    tankType: Fuel
  - type: PacifismDangerousAttack
  - type: Explosive
    explosionType: Default
    totalIntensity: 60 # Mediocre explosion. Not enough to do any meaningful structural damage to anything other then windows, provided you're only using one tank.

- type: entity
  id: CrudeOilTankFull
  parent: CrudeOilTank
  suffix: Full
  components:
  - type: SolutionContainerManager
    solutions:
      tank:
        reagents:
        - ReagentId: CrudeOil
          Quantity: 1500    

Танкс FTL кидать : \Lust Station\Resources\Locale\ru-RU\_prototypes\entities\structures\storage\tanks
Код:
ent-CrudeOilTank = резервуар сырой нефти 
    .suffix = Пустой
    .desc = Резервуар используемый для хранения и удобной транспортировки сырой нефти на продажу.
ent-CrudeOilTankFull = { ent-CrudeOilTank }
    .suffix = Заполненный
    .desc = { ent-CrudeOilTank.desc }   

Список заказов карго кидать: \Lust Station\Resources\Prototypes\Catalog\Cargo
код:
- type: cargoProduct
  id: MaterialCrudeOilTank
  icon:
    sprite: Structures/Storage/tanks.rsi
    state: oiltank
  product: CrudeOilTankFull
  cost: 1125
  category: cargoproduct-category-name-materials
  group: market 