targetScope = 'subscription'

param name string
param location string
param tags object = {}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: name
  location: location
  tags: tags
}

output name string = resourceGroup.name
output id string = resourceGroup.id