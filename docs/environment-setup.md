# Environment setup

## Azure

Each environment requires two resources in Azure; a storage account where Terraform state files are stored and a Key Vault where secrets are stored.

In addition, a service principal that has read access to the Key Vault needs to be set up. This is used by GitHub Actions to retrieve secrets.

### Create a service principal

Using the Azure CLI create a service principal in the appropriate subscription:

```
az account set --subscription <subscription name>
az ad sp create-for-rbac --name <principal name> --skip-assignment --sdk-auth
```

#### Example
```
az account set --subscription s165-teachingqualificationsservice-development
az ad sp create-for-rbac --name s165d01-keyvault-readonly-access --skip-assignment --sdk-auth
```

The final command should output a JSON object containing `clientId`, `clientSecret`, `subscriptionId` and `tenantId`.

In GitHub navigate to Settings, Environments. Click 'New environment' and add the new environment.
Navigate to the newly created environment and under Environment secrets click 'Add Secret'.
Add a secret named `AZURE_CREDENTIALS`. Use the JSON object from above for the value.


### Create the resource group

Create a parameters file for the new environment within the `azure` folder e.g. `azuredeploy.dev.parameters.json`.
Specify values for the `keyVaultName` and `storageAccountName` parameters.

#### Example **`azuredeploy.dev.parameters.json`**
```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "keyVaultName": {
      "value": "s165d01-kv"
    },
    "storageAccountName": {
      "value": "s165d01tfstate"
    }
  }
}
```

Run the PowerShell script at `azure/Set-ResourceGroup.ps1`. This command will create the resource group with a storage account and Key Vault and assign the relevant Key Vault permissions.

```
Set-ResourceGroup.ps1 -ResourceGroupName <resource group> -EnvironmentName <environment name> -Subscription <subscription name> -ParametersFile <parameters file> -ServicePrincipalName <service principal>
```

#### Example
```
Set-ResourceGroup.ps1 -ResourceGroupName s165d01-rg -EnvironmentName dev -Subscription s165-teachingqualificationsservice-development -ParametersFile azuredeploy.dev.parameters.json -ServicePrincipalName s165d01-keyvault-readonly-access
```


### Assign Key Vault permissions to Service Principal

Send a 'CIP Request' in ServiceNow to have the 'Key Vault Reader' role assigned to the service principal created above for the Key Vault.


### Add storage account key to Key Vault

In the Azure portal navigate to the storage account created above and retrieve an Access key. In the Key Vault created above add a new Secret named `TFSTATE-CONTAINER-ACCESS-KEY` with the storage access key as the value.


## Terraform

Within the `terraform` folder create two parameter files for the new environment; one named `<environment>.tfvars` and one `<environment>.backend.tfvars`.

The `.backend.tfvars` file needs the storage account name from the resource group deployment above. The `key` is `<environment name>.tfstate`.

#### Example **`dev.backend.tfvars`**
```hcl
storage_account_name = "s165d01tfstate"
key                  = "dev.tfstate"
```

The main `.tfvars.json` file needs the resource group and Key Vault names from the resource group deployment above as well as the PAAS space and application name where it should be deployed.

#### Example **`dev.tfvars.json`**
```hcl
{
  "key_vault_name": "s165d01-kv",
  "resource_group_name": "s165d01-rg",
  "api_app_name": "qualified-teachers-api-dev",
  "paas_space": "tra-dev"
}
```

## Secrets

In addition to the `TFSTATE-CONTAINER-ACCESS-KEY` secret, `PAAS-USER` and `PAAS-PASSWORD` secrets need to be added to the Key Vault. The user should have the Space Developer role within the target space.

### Application secrets

Application config is stored in a JSON-encoded Key Vault secret named 'APP-CONFIG'.

Our `AddJsonEnvironmentVariable` configuration extension is used to read the environment variable, decode its contents then set configuration keys appropriately.
