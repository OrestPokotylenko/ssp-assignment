using '../main.bicep'

param functionsEnvironment = 'Staging'

param environment = 'dev'
param location = 'francecentral'

param functionInstanceMemoryMB = 2048
param functionMaximumInstanceCount = 10

param weatherStationCount = 1
