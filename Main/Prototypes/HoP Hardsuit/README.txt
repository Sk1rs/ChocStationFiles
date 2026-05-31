helmet : \Lust Station\Resources\Prototypes\Entities\Clothing\Head
code: 
- type: entity
  parent: [ ClothingHeadHardsuitBase, ClothingHeadSuitWithLightBase ]
  id: ClothingHeadHelmetHardsuitHoP
  name: HoP hardsuit helmet
  description: desc.
  components:
  - type: Sprite
    sprite: Clothing/Head/Hardsuits/hop.rsi
    layers:
    - state: icon
    - state: icon-unshaded
      shader: unshaded
    - state: light-overlay
      visible: false
      shader: unshaded
      map: [ "light" ]
  - type: HandheldLight
    addPrefix: false
  - type: ToggleableVisuals
    spriteLayer: light
    clothingVisuals:
      head:
      - state: equipped-head-light
        shader: unshaded
  - type: Clothing
    clothingVisuals:
      head:
      - state: equipped-head
      - state: equipped-head-unshaded
        shader: unshaded
  - type: PointLight
    color: "#ececbf"
  - type: PressureProtection
    highPressureMultiplier: 0.08
    lowPressureMultiplier: 1000
  - type: TemperatureProtection
    heatingCoefficient: 0.005
    coolingCoefficient: 0.005
  - type: FireProtection
    reduction: 0.2
  - type: Tag
    tags:
    - WhitelistChameleon  
heads : Lust Station\Resources\Prototypes\Catalog\Fills\Lockers
code:
118 строчка
- type: entityTable
  id: LockerFillHeadOfPersonnel
  table: !type:AllSelector
    children:
    - id: AccessConfigurator
    - id: BoxEncryptionKeyPassenger
    - id: BoxEncryptionKeyService
    - id: BoxHeadset
    - id: BoxHoPCircuitboards
    - id: BoxHoPStamps
    - id: BoxID
    - id: BoxPDA
    - id: CigarGoldCase
      prob: 0.25
      # Fuck the HoP they don't deserve fucking cigars.
      # Yes they do fuck you.
    - id: ClothingBackpackIan
      prob: 0.5
    - id: ClothingHeadsetCommand
    - id: ClothingNeckGoldmedal
    - id: DoorRemoteService
    - id: HoPIDCard
    - id: WeaponDisabler
    - id: ClothingEyesHudCommand
    # Sunrise-start
    - id: FlippoLighterSunriseHop
    - id: DrinkInc
    - id: SheetOfficePaper
    - id: WeaponMiniEnergyGun
    - id: PlantAnalyzerCartridge
    # Sunrise-end
    - id: ClothingOuterHardsuitHoP

hardsuit : \Lust Station\Resources\Prototypes\Entities\Clothing\OuterClothing
code:
- type: entity
  parent: [ClothingOuterHardsuitBase, BaseCommandContraband]
  id: ClothingOuterHardsuitHoP
  name: HoP hardsuit
  description: DESC.
  components:
  - type: Sprite
    sprite: Clothing/OuterClothing/Hardsuits/hop.rsi
  - type: Clothing
    sprite: Clothing/OuterClothing/Hardsuits/hop.rsi
  - type: PressureProtection
    highPressureMultiplier: 0.02
    lowPressureMultiplier: 1000
  - type: TemperatureProtection
    heatingCoefficient: 0.001
    coolingCoefficient: 0.001
  - type: FireProtection
    reduction: 0.8
  - type: ExplosionResistance
    damageCoefficient: 0.5
  - type: Pierceable
    level: Metal
  - type: Armor
    modifiers:
      coefficients:
        Blunt: 0.9
        Slash: 0.9
        Piercing: 0.9
        Heat: 0.9
        Radiation: 0.9
  - type: ClothingSpeedModifier
    walkModifier: 0.9
    sprintModifier: 0.9
  - type: HeldSpeedModifier
  - type: ToggleableClothing
    clothingPrototype: ClothingHeadHelmetHardsuitHoP
  - type: Tag
    tags:
    - Hardsuit
    - WhitelistChameleon    

Hop helmet texture: 

