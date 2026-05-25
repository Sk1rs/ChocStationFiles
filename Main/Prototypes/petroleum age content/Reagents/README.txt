Реагенты: Химикалс и Пиротехникс кидать по пути: \Lust Station\Resources\Prototypes\Reagents
Код:
Химикалс:
- type: reagent
  id: Polyacrylonitrile
  name: reagent-name-polyacrylonitrile
  desc: reagent-desc-polyacrylonitrile
  flavor: fiber
  color: "#FFFFFF"
  physicalDesc: reagent-physical-desc-fibrous

Пиротехникс (тут дохуя):
#CS14 CONTETNT  

#Виды нефти, а вы что хотели

- type: reagent
  id: CrudeOil
  name: reagent-name-crude-oil
  parent: BasePyrotechnic
  desc: reagent-desc-crude-oil
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4

- type: reagent
  id: DegassedOil
  name: reagent-name-degassed-oil
  parent: BasePyrotechnic
  desc: reagent-desc-degassed-oil
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4 

- type: reagent
  id: StabilizedOil
  name: reagent-name-stabilized-oil
  parent: BasePyrotechnic
  desc: reagent-desc-stabilized-oil
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4                

- type: reagent
  id: DieselResidue
  name: reagent-name-diesel-residue
  parent: BasePyrotechnic
  desc: reagent-desc-diesel-residue
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4    

- type: reagent
  id: DieselKeroseneResidue
  name: reagent-name-diesel-kerosene-residue
  parent: BasePyrotechnic
  desc: reagent-desc-diesel-kerosene-residue
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4         

- type: reagent
  id: FuelOil
  name: reagent-name-fuel-oil
  parent: BasePyrotechnic
  desc: reagent-desc-fuel-oil
  physicalDesc: reagent-physical-desc-oily
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#0B0B0A"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4        

#Нефтепродукты        

- type: reagent
  id: HeavyNaphtha
  name: reagent-name-heavy-naphtha
  parent: BasePyrotechnic
  desc: reagent-desc-heavy-naphtha
  physicalDesc: reagent-physical-desc-translucent
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#F9E79F"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4     

- type: reagent
  id: Kerosene
  name: reagent-name-kerosene
  parent: BasePyrotechnic
  desc: reagent-desc-kerosene
  physicalDesc: reagent-physical-desc-translucent
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#F6F5CD"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4 

- type: reagent
  id: Diesel
  name: reagent-name-diesel
  parent: BasePyrotechnic
  desc: reagent-desc-diesel
  physicalDesc: reagent-physical-desc-translucent
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#F7E9C6"
  recognizable: true
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4 

- type: reagent
  id: LightNaphtha
  name: reagent-name-light-naphtha
  parent: BasePyrotechnic
  desc: reagent-desc-light-naphtha
  physicalDesc: reagent-physical-desc-translucent
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#FFFFFF"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4 

- type: reagent
  id: LPG
  name: reagent-name-LPG
  parent: BasePyrotechnic
  desc: reagent-desc-LPG
  physicalDesc: reagent-physical-desc-gaseous
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#FFFFFF"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4  

- type: reagent
  id: Ethylene
  name: reagent-name-ethylene
  parent: BasePyrotechnic
  desc: reagent-desc-ethylene
  physicalDesc: reagent-physical-desc-gaseous
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#FFFFFF"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4
        
- type: reagent
  id: Propylene
  name: reagent-name-propylene
  parent: BasePyrotechnic
  desc: reagent-desc-propylene
  physicalDesc: reagent-physical-desc-gaseous
  slipData:
    requiredSlipSpeed: 3.5
  flavor: bitter
  flavorMinimum: 0.01
  color: "#FFFFFF"
  recognizable: false
  boilingPoint: -84.7 # Acetylene. Close enough.
  meltingPoint: -80.7
  friction: 0.4
  tileReactions:
  - !type:FlammableTileReaction {}
  metabolisms:
    Food:
      effects:
      - !type:SatiateThirst
        factor: 1
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
    Poison:
      effects:
      - !type:HealthChange
        conditions:
        - !type:MetabolizerTypeCondition
          type: [Vox]
          inverted: true
        damage:
          types:
            Poison: 1
      - !type:Flammable
        multiplier: 0.4
    