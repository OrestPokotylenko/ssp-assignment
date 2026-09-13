param name string
param location string
param tags object = {}

param startJobsQueueName string = 'start-jobs'
param imageJobsQueueName string = 'image-jobs'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: name
  location: location
  tags: tags

  sku: {
    name: 'Basic'
    tier: 'Basic'
  }

  properties: {
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }
}

resource startJobsQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: startJobsQueueName

  properties: {
    lockDuration: 'PT1M'
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P1D'
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

resource imageJobsQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: imageJobsQueueName

  properties: {
    lockDuration: 'PT1M'
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P1D'
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
  }
}

output id string = serviceBusNamespace.id
output name string = serviceBusNamespace.name

output fullyQualifiedNamespace string = '${serviceBusNamespace.name}.servicebus.windows.net'

output startJobsQueueName string = startJobsQueue.name
output imageJobsQueueName string = imageJobsQueue.name
