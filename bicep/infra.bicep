param location string = resourceGroup().location
@secure()
param authClientSecret string
@secure()
param authClientId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  location: location
  name: 'log-analytics'
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  location: location
  name: 'app-environment'
  properties: {
    appLogsConfiguration:{
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-12-01-preview' existing {
  name: 'authcontainerappexample'
}

resource securedApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: 'secured-app'
  location: location
  properties: {
    managedEnvironmentId: appEnvironment.id
    configuration: {
      ingress: {
        targetPort: 80
        external: true
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistry.name
          username: containerRegistry.properties.loginServer
          passwordSecretRef: 'container-registry-password'
        }
      ]
      activeRevisionsMode: 'single'
      secrets: [
        {
          name: 'auth-client-secret'
          value: authClientSecret
        }
        {
          name: 'container-registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'secured-app-container'
          image: 'mcr.microsoft.com/secured-app:latest'
        }
      ]
    }
  }
  resource auth0 'authConfigs@2022-03-01' = {
    name: 'current'
    properties: {
      globalValidation: {
        redirectToProvider: 'auth0'
        unauthenticatedClientAction: 'RedirectToLoginPage'
      }
      identityProviders: {
        customOpenIdConnectProviders: {
          auth0: {
            registration: {
              clientCredential: {
                clientSecretSettingName: 'auth-client-secret'
              }
              clientId: authClientId
              openIdConnectConfiguration: {
                wellKnownOpenIdConfiguration: 'https://container-app-auth.eu.auth0.com/.well-known/openid-configuration'
              }
            }
            login: {
              nameClaimType: 'name'
            }
          }
          platform: {
            enabled: true
          }
        }
      }
    }
  }
}
