param name string
param location string
param tags object = {}

param generatedImagesContainerName string = 'generated-images'
param deploymentContainerName string = 'deployments'
param jobStatusTableName string = 'jobstatus'

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: name
  location: location
  tags: tags

  sku: {
    name: 'Standard_LRS'
  }

  kind: 'StorageV2'

  properties: {
    accessTier: 'Hot'

    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'

    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    defaultToOAuthAuthentication: true

    publicNetworkAccess: 'Enabled'

    sasPolicy: {
      sasExpirationPeriod: '00.00:15:00'
      expirationAction: 'Block'
    }
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource generatedImagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobService
  name: generatedImagesContainerName

  properties: {
    publicAccess: 'None'
  }
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobService
  name: deploymentContainerName

  properties: {
    publicAccess: 'None'
  }
}

resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2024-01-01' = {
  parent: storageAccount
  name: 'default'

  properties: {
    policy: {
      rules: [
        {
          name: 'generated-images-lifecycle'
          enabled: true
          type: 'Lifecycle'

          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]

              prefixMatch: [
                '${generatedImagesContainerName}/'
              ]
            }

            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 1
                }

                delete: {
                  daysAfterModificationGreaterThan: 7
                }
              }
            }
          }
        }
      ]
    }
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2024-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource jobStatusTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2024-01-01' = {
  parent: tableService
  name: jobStatusTableName
}

output id string = storageAccount.id
output name string = storageAccount.name

output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output tableEndpoint string = storageAccount.properties.primaryEndpoints.table

output generatedImagesContainerName string = generatedImagesContainer.name
output deploymentContainerName string = deploymentContainer.name
output jobStatusTableName string = jobStatusTable.name
