targetScope = 'subscription'

@allowed([
  'dev'
  'prod'
])
param environment string

@allowed([
  'Staging'
  'Production'
])
param functionsEnvironment string

param location string = 'francecentral'
param projectName string = 'weatherpix'
param pexelsSecretName string = 'pexels-api-key'

param sasExpirationMinutes int = 30
param sasClockSkewMinutes int = 1

param weatherStationCount int = 50
param buienradarBaseUrl string = 'https://data.buienradar.nl/2.0/feed/json'
param pexelsBaseUrl string = 'https://api.pexels.com/'

param authDomain string = 'dev-8rkdghl0g37kgzzx.us.auth0.com'
param authAudience string = 'api://weatherpix'
param authReadPermission string = 'jobs:read'
param authCreatePermission string = 'jobs:create'

param deploymentPrincipalId string = ''

@allowed([
  512
  2048
  4096
])
param functionInstanceMemoryMB int = 2048

param functionMaximumInstanceCount int = 40

var uniqueSuffix = take(uniqueString(subscription().id, projectName, environment), 5)

var resourceGroupName = 'rg-${projectName}-${environment}'
var storageAccountName = 'st${projectName}${environment}${uniqueSuffix}'
var serviceBusNamespaceName = 'sb-${projectName}-${environment}-${uniqueSuffix}'
var keyVaultName = 'kv-${projectName}-${environment}-${uniqueSuffix}'
var functionPlanName = 'plan-${projectName}-${environment}'
var functionAppName = 'func-${projectName}-${environment}-${uniqueSuffix}'

var tags = {
  project: projectName
  environment: environment
  managedBy: 'bicep'
}

module resourceGroupModule './modules/resource-group.bicep' = {
  name: 'resource-group-${environment}'

  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

module storage './modules/storage.bicep' = {
  name: 'storage-${environment}'
  scope: az.resourceGroup(resourceGroupName)

  params: {
    name: storageAccountName
    location: location
    tags: tags
  }

  dependsOn: [
    resourceGroupModule
  ]
}

module serviceBus './modules/service-bus.bicep' = {
  name: 'service-bus-${environment}'
  scope: az.resourceGroup(resourceGroupName)

  params: {
    name: serviceBusNamespaceName
    location: location
    tags: tags
  }

  dependsOn: [
    resourceGroupModule
  ]
}

module keyVault './modules/key-vault.bicep' = {
  name: 'key-vault-${environment}'
  scope: az.resourceGroup(resourceGroupName)

  params: {
    name: keyVaultName
    location: location
    tags: tags
  }

  dependsOn: [
    resourceGroupModule
  ]
}

module functionApp './modules/function-app.bicep' = {
  name: 'function-app-${environment}'
  scope: az.resourceGroup(resourceGroupName)

  params: {
    name: functionAppName
    planName: functionPlanName

    location: location
    tags: tags

    functionsEnvironment: functionsEnvironment
    storageAccountName: storage.outputs.name
    storageBlobEndpoint: storage.outputs.blobEndpoint
    deploymentContainerName: storage.outputs.deploymentContainerName

    serviceBusFullyQualifiedNamespace: serviceBus.outputs.fullyQualifiedNamespace
    startJobsQueueName: serviceBus.outputs.startJobsQueueName
    imageJobsQueueName: serviceBus.outputs.imageJobsQueueName
    sasExpirationMinutes: sasExpirationMinutes
    sasClockSkewMinutes: sasClockSkewMinutes

    generatedImagesContainerName: storage.outputs.generatedImagesContainerName
    jobStatusTableName: storage.outputs.jobStatusTableName

    keyVaultUri: keyVault.outputs.uri
    pexelsSecretName: pexelsSecretName

    weatherStationCount: weatherStationCount
    buienradarBaseUrl: buienradarBaseUrl
    pexelsBaseUrl: pexelsBaseUrl

    authDomain: authDomain
    authAudience: authAudience
    authReadPermission: authReadPermission
    authCreatePermission: authCreatePermission

    instanceMemoryMB: functionInstanceMemoryMB
    maximumInstanceCount: functionMaximumInstanceCount
  }

  dependsOn: [
    resourceGroupModule
  ]
}

module rbac './modules/rbac.bicep' = {
  name: 'rbac-${environment}'
  scope: az.resourceGroup(resourceGroupName)

  params: {
    principalId: functionApp.outputs.principalId
    deploymentPrincipalId: deploymentPrincipalId

    storageAccountName: storage.outputs.name
    serviceBusNamespaceName: serviceBus.outputs.name
    keyVaultName: keyVault.outputs.name
  }

  dependsOn: [
    resourceGroupModule
  ]
}

output resourceGroupName string = resourceGroupName

output functionAppName string = functionApp.outputs.name
output functionAppHostname string = functionApp.outputs.defaultHostname

output storageAccountName string = storage.outputs.name
output serviceBusNamespaceName string = serviceBus.outputs.name
output keyVaultName string = keyVault.outputs.name
