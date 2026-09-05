param name string
param planName string

param location string
param tags object = {}

param storageAccountName string
param storageBlobEndpoint string
param deploymentContainerName string

param serviceBusFullyQualifiedNamespace string
param startJobsQueueName string
param imageJobsQueueName string

param generatedImagesContainerName string
param jobStatusTableName string

param keyVaultUri string
param pexelsSecretName string

@allowed([
  512
  2048
  4096
])
param instanceMemoryMB int = 2048

param maximumInstanceCount int = 40

resource hostingPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  tags: tags

  kind: 'functionapp'

  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }

  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: name
  location: location
  tags: tags

  kind: 'functionapp,linux'

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true

    siteConfig: {
      minTlsVersion: '1.2'
    }

    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageBlobEndpoint}${deploymentContainerName}'

          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }

      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }

      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
  }
}

resource appSettings 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: functionApp
  name: 'appsettings'

  properties: {
    AzureWebJobsStorage__accountName: storageAccountName
    AzureWebJobsStorage__credential: 'managedidentity'

    ServiceBus__fullyQualifiedNamespace: serviceBusFullyQualifiedNamespace
    ServiceBus__credential: 'managedidentity'

    StartJobsQueueName: startJobsQueueName
    ImageJobsQueueName: imageJobsQueueName

    Storage__BlobServiceUri: 'https://${storageAccountName}.blob.${environment().suffixes.storage}'
    Storage__TableServiceUri: 'https://${storageAccountName}.table.${environment().suffixes.storage}'

    Storage__GeneratedImagesContainerName: generatedImagesContainerName
    Storage__JobStatusTableName: jobStatusTableName

    KeyVault__VaultUri: keyVaultUri
    KeyVault__PexelsSecretName: pexelsSecretName
  }
}

output id string = functionApp.id
output name string = functionApp.name
output principalId string = functionApp.identity.principalId
output defaultHostname string = functionApp.properties.defaultHostName
