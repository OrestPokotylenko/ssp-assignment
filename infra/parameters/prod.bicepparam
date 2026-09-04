using '../main.bicep'

param environment = 'prod'
param location = 'francecentral'

param functionInstanceMemoryMB = 2048
param functionMaximumInstanceCount = 40

param deploymentPrincipalId = readEnvironmentVariable('AZURE_PRINCIPAL_ID')
