Building Data ScriptableObjects go here.

Create one BuildingData asset per building type via the Unity context menu:
  Assets > Create > Settlers > Building Data

Required buildings to create (matching BuildingType enum values):
  - Headquarters    (isStorage=true, capacity=200, buildTime=0)
  - Warehouse       (isStorage=true, capacity=100)
  - Woodcutter      (recipe=LogRecipe, workers=1)
  - Sawmill         (recipe=PlankRecipe, workers=1)
  - Quarry          (recipe=StoneRecipe, workers=2)
  - CoalMine        (recipe=CoalRecipe, workers=2)
  - IronMine        (recipe=IronOreRecipe, workers=2)
  - GoldMine        (recipe=GoldOreRecipe, workers=2)
  - Smelter         (recipe=IronRecipe, workers=1)
  - Farm            (recipe=GrainRecipe, workers=2)
  - Mill            (recipe=FlourRecipe, workers=1)
  - Bakery          (recipe=BreadRecipe, workers=1)
  - FishingHut      (recipe=FishRecipe, workers=1)
  - Toolsmith       (recipe=ToolsRecipe, workers=1)
  - Armory          (recipe=SwordRecipe, workers=1)
  - Barracks        (isMilitary=true, maxSoldiers=8, maxGarrison=8)
  - Watchtower      (isMilitary=true, territoryRadius=5, maxGarrison=2)
  - Fortress        (isMilitary=true, territoryRadius=10, maxGarrison=16)

Production Recipes (create via Assets > Create > Settlers > Production Recipe):
  LogRecipe:      output=Log(1),    inputs=[],         cycleTime=20
  PlankRecipe:    output=Plank(1),  inputs=[Log(1)],   cycleTime=15
  StoneRecipe:    output=Stone(1),  inputs=[],         cycleTime=25
  CoalRecipe:     output=Coal(1),   inputs=[],         cycleTime=20
  IronOreRecipe:  output=IronOre(1),inputs=[],         cycleTime=20
  GoldOreRecipe:  output=GoldOre(1),inputs=[],         cycleTime=30
  IronRecipe:     output=Iron(1),   inputs=[IronOre(1),Coal(1)], cycleTime=20
  GrainRecipe:    output=Grain(1),  inputs=[],         cycleTime=30
  FlourRecipe:    output=Flour(1),  inputs=[Grain(1)], cycleTime=15
  BreadRecipe:    output=Bread(1),  inputs=[Flour(1)], cycleTime=10
  FishRecipe:     output=Fish(1),   inputs=[],         cycleTime=18
  ToolsRecipe:    output=Tools(1),  inputs=[Iron(1),Plank(1)], cycleTime=25
  SwordRecipe:    output=Sword(1),  inputs=[Iron(1),Coal(1)],  cycleTime=30
