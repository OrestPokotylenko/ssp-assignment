using '../main.bicep'

param functionsEnvironment = 'Production'

param environment = 'prod'
param location = 'francecentral'

param functionInstanceMemoryMB = 2048
param functionMaximumInstanceCount = 40

param weatherStationCount = 50
