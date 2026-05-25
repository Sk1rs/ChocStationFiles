Реакции: Химикалс и пиротехникс YML кидать по пути: Lust Station\Resources\Prototypes\Recipes\Reactions
код оттуда :
Химикалс:
- type: reaction
  minTemp: 300
  id: CrudeOilDegassing
  reactants:
    CrudeOil:
      amount: 1
  products:
    LPG: 0.05  
    DegassedOil: 0.95

- type: reaction
  minTemp: 360
  id: CrudeOilStabilization
  reactants:
    DegassedOil:
      amount: 1
  products:
    LightNaphtha: 0.25  
    StabilizedOil: 0.75    

- type: reaction
  minTemp: 450
  id: CrudeOilRelief
  reactants:
    StabilizedOil:
      amount: 1
  products:
    HeavyNaphtha: 0.30  
    DieselKeroseneResidue: 0.70    

- type: reaction
  minTemp: 520
  id: KeroseneExtraction
  reactants:
    DieselKeroseneResidue:
      amount: 1
  products:
    Kerosene: 0.35 
    DieselResidue: 0.65    

- type: reaction
  minTemp: 620
  id: DieselExtraction
  reactants:
    DieselResidue:
      amount: 1
  products:
    Diesel: 0.40 
    FuelOil: 0.60  

- type: reaction
  minTemp: 1100
  id: PoluPlastic
  reactants:
    HeavyNaphtha:
      amount: 1
    Water:
      amount: 1 
  products: 
    Propylene: 0.5
    Ethylene: 0.5

- type: reaction
  minTemp: 350
  id: Plastic
  reactants:
    Ethylene:
      amount: 10
    Aluminium:
      amount: 1 
      catalyst: true
  effects:
    - !type:SpawnEntity
      entity: SheetPlastic1   

- type: reaction
  id: Polyacrylonitrile
  reactants:
    Propylene:
      amount: 2
    Ammonia:
      amount: 1 
    Carbon:
      amount: 1
  products:
    Polyacrylonitrile: 4

- type: reaction
  minTemp: 350
  id: Cloth
  reactants:
    Polyacrylonitrile:
      amount: 5
    Water:
      amount: 5
  effects:
    - !type:SpawnEntity
      entity: MaterialCloth1          

Пиротехникс:
- type: reaction
  id: WeldingFuel
  minTemp: 550
  reactants:
    Kerosene:
      amount: 1
    Hydrogen:
      amount: 1
    Iron:
      amount: 1
      catalyst: true    
  products:
    WeldingFuel: 2.8
    Sulfur: 0.2   