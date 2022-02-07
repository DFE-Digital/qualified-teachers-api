#Requires -Version 7.0
$ErrorActionPreference = "Stop"

$userSecretsId = "DqtApi"

function Get-UserSecret {
    [CmdletBinding()]
    param (
        [string]$Key
    )

    $secrets = dotnet user-secrets --id $userSecretsId list

    $value = $secrets -match "^${Key}" -split " = " | Select-Object -Skip 1

    if ($null -eq $value) {
        Write-Error "No secret found with key ${Key}"
    }

    return $value
}

$crmUrl = Get-UserSecret "CrmUrl"
$crmClientId = Get-UserSecret "CrmClientId"
$crmClientSecret = Get-UserSecret "CrmClientSecret"

$connectionString = "AuthType=ClientSecret;url=${crmUrl};ClientId=${crmClientId};ClientSecret=${crmClientSecret}"

$coreToolsFolder = (Join-Path $PSScriptRoot tools coretools)

$namespace = "DqtApi.DataStore.Crm.Models"
$entitiesOutput = Join-Path -Path $PSScriptRoot -ChildPath src DqtApi DataStore Crm Models GeneratedCode.cs
mkdir (Split-Path $entitiesOutput) -Force | Out-Null

# entities
$crmSvcUtil = Join-Path -Path $coreToolsFolder -ChildPath "CrmSvcUtil.exe"
& $crmSvcUtil `
    /connectionstring:${connectionString} `
    /out:${entitiesOutput} `
    /namespace:${namespace} `
    /emitfieldsclasses `
    /SuppressGeneratedCodeAttribute `
    /codecustomization:"DLaB.CrmSvcUtilExtensions.Entity.CustomizeCodeDomService,DLaB.CrmSvcUtilExtensions" `
    /codegenerationservice:"DLaB.CrmSvcUtilExtensions.Entity.CustomCodeGenerationService,DLaB.CrmSvcUtilExtensions" `
    /codewriterfilter:"DLaB.CrmSvcUtilExtensions.Entity.CodeWriterFilterService,DLaB.CrmSvcUtilExtensions" `
    /namingservice:"DLaB.CrmSvcUtilExtensions.NamingService,DLaB.CrmSvcUtilExtensions" `
    /metadataproviderservice:"DLaB.CrmSvcUtilExtensions.Entity.MetadataProviderService,DLaB.CrmSvcUtilExtensions"

# option sets
$optionSetsOutput = Join-Path -Path $PSScriptRoot -ChildPath src DqtApi DataStore Crm Models GeneratedOptionSets.cs
& $crmSvcUtil `
    /connectionstring:${connectionString} `
    /out:${optionSetsOutput} `
    /namespace:${namespace} `
    /SuppressGeneratedCodeAttribute `
    /codecustomization:"DLaB.CrmSvcUtilExtensions.OptionSet.CustomizeCodeDomService,DLaB.CrmSvcUtilExtensions" `
    /codegenerationservice:"DLaB.CrmSvcUtilExtensions.OptionSet.CustomCodeGenerationService,DLaB.CrmSvcUtilExtensions" `
    /codewriterfilter:"DLaB.CrmSvcUtilExtensions.OptionSet.CodeWriterFilterService,DLaB.CrmSvcUtilExtensions" `
    /namingservice:"DLaB.CrmSvcUtilExtensions.NamingService,DLaB.CrmSvcUtilExtensions" `
    /metadataproviderservice:"DLaB.CrmSvcUtilExtensions.BaseMetadataProviderService,DLaB.CrmSvcUtilExtensions"
