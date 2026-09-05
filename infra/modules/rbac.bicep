param principalId string
param deploymentPrincipalId string = ''

param storageAccountName string
param serviceBusNamespaceName string
param keyVaultName string

// Storage
var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

// Service Bus
var serviceBusDataSenderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
var serviceBusDataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'

// Key Vault
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultSecretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

var shouldCreateDeploymentRbac = !empty(deploymentPrincipalId)

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {
  name: storageAccountName
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusNamespaceName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Key Vault
resource deploymentKeyVaultSecretsOfficer 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (shouldCreateDeploymentRbac) {
  name: guid(keyVault.id, deploymentPrincipalId, keyVaultSecretsOfficerRoleId)
  scope: keyVault

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsOfficerRoleId)

    principalId: deploymentPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Function runtime + Blob data
resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, storageBlobDataContributorRoleId)
  scope: storageAccount

  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageBlobDataContributorRoleId
    )

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

// Function host internally uses Queue Storage
resource storageQueueDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, storageQueueDataContributorRoleId)
  scope: storageAccount

  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageQueueDataContributorRoleId
    )

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, storageTableDataContributorRoleId)
  scope: storageAccount

  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageTableDataContributorRoleId
    )

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, principalId, serviceBusDataSenderRoleId)
  scope: serviceBusNamespace

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataSenderRoleId)

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusReceiver 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, principalId, serviceBusDataReceiverRoleId)
  scope: serviceBusNamespace

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiverRoleId)

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
  scope: keyVault

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)

    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
