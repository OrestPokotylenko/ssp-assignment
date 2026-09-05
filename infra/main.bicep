targetScope = 'subscription'

@allowed([
  'dev'
  'prod'
])
param environment string

param location string = 'francecentral'
param projectName string = 'weatherpix'
param pexelsSecretName string = 'pexels-api-key'

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

    storageAccountName: storage.outputs.name
    storageBlobEndpoint: storage.outputs.blobEndpoint
    deploymentContainerName: storage.outputs.deploymentContainerName

    serviceBusFullyQualifiedNamespace: serviceBus.outputs.fullyQualifiedNamespace
    startJobsQueueName: serviceBus.outputs.startJobsQueueName
    imageJobsQueueName: serviceBus.outputs.imageJobsQueueName

    generatedImagesContainerName: storage.outputs.generatedImagesContainerName
    jobStatusTableName: storage.outputs.jobStatusTableName

    keyVaultUri: keyVault.outputs.uri
    pexelsSecretName: pexelsSecretName

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
