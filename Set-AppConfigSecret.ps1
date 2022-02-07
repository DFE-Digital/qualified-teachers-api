#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [String]$EnvironmentName,
    [Parameter(Mandatory = $true)]
    [String]$AzureSubscription,
    [Parameter(Mandatory = $true)]
    [String]$ConfigKey,
    [Parameter(Mandatory = $true)]
    $ConfigValue,
    [Switch]$Commit
)

$ErrorActionPreference = "Stop"

function Invoke-NativeCommand() {
    $command = $args[0]
    $commandArgs = @()
    if ($args.Count -gt 1) {
        $commandArgs = $args[1..($args.Count - 1)]
    }

    & $command $commandArgs
    $result = $LASTEXITCODE

    if ($result -ne 0) {
        throw "$command $commandArgs exited with code $result."
    }
}

function Merge-Config($json, $path, $value) {
    $config = $json | ConvertFrom-Json -AsHashTable

    $pathComponents = $path -split '\.|:|__'
    $location = $config

    foreach ($component in $pathComponents) {
        if (!($location.ContainsKey($component))) {
            $location.Add($component, (ConvertFrom-Json '{}' -AsHashtable))
        }

        if ($component -eq $pathComponents[$pathComponents.Length - 1]) {
            $location."$component" = $value
        } else {
            $location = $location."$component"
        }
    }

    $config | ConvertTo-OrderedDictionary | ConvertTo-Json -Depth 100
}

Function ConvertTo-OrderedDictionary {
<#
.SYNOPSIS
    Converts a HashTable, Array, or an OrderedDictionary to an OrderedDictionary.

.DESCRIPTION
    ConvertTo-OrderedDictionary takes a HashTable, Array, or an OrderedDictionary
    and returns an ordered dictionary.

    If you enter a hash table, the keys in the hash table are ordered
    alphanumerically in the dictionary. If you enter an array, the keys
    are integers 0 - n.
.PARAMETER $Hash
    Specifies a hash table or an array. Enter the hash table or array,
    or enter a variable that contains a hash table or array. If the input
    is an OrderedDictionary the key order is the same in the copy.
.INPUTS
    System.Collections.Hashtable
    System.Array
    System.Collections.Specialized.OrderedDictionary
.OUTPUTS
    System.Collections.Specialized.OrderedDictionary
.NOTES
    source: https://gallery.technet.microsoft.com/scriptcenter/ConvertTo-OrderedDictionary-cf2404ba
    converted to function and added ability to copy OrderedDictionary

.EXAMPLE
    PS C:\> $myHash = @{a=1; b=2; c=3}
    PS C:\> .\ConvertTo-OrderedDictionary.ps1 -Hash $myHash

    Name Value
    ---- -----
    a 1
    b 2
    c 3
.EXAMPLE
    PS C:\> $myHash = @{a=1; b=2; c=3}
    PS C:\> $myHash = .\ConvertTo-OrderedDictionary.ps1 -Hash $myHash
    PS C:\> $myHash

    Name Value
    ---- -----
    a 1
    b 2
    c 3

    PS C:\> $myHash | Get-Member

       TypeName: System.Collections.Specialized.OrderedDictionary
       . . .

.EXAMPLE
    PS C:\> $colors = "red", "green", "blue"
    PS C:\> $colors = .\ConvertTo-OrderedDictionary.ps1 -Hash $colors
    PS C:\> $colors

    Name Value
    ---- -----
    0 red
    1 green
    2 blue
.LINK
    about_hash_tables
#>

    #Requires -Version 3

    [CmdletBinding(ConfirmImpact='None')]
    [OutputType('System.Collections.Specialized.OrderedDictionary')]
    Param (
        [parameter(Mandatory,HelpMessage='Add help message for user', ValueFromPipeline)]
        $Hash
    )

    begin {
        Write-Verbose -Message "Starting $($MyInvocation.Mycommand)"
    } #close begin block

    process {
        write-verbose -Message ($Hash.gettype())
        if ($Hash -is [System.Collections.Hashtable])
        {
            write-verbose -Message '$Hash is a HashTable'
            $dictionary = [ordered] @{}
            $keys = $Hash.keys | sort-object
            foreach ($key in $keys)
            {
                $dictionary.add($key, $Hash[$key])
            }
            $dictionary
        }
        elseif ($Hash -is [System.Array])
        {
            write-verbose -Message '$Hash is an Array'
            $dictionary = [ordered] @{}
            for ($i = 0; $i -lt $hash.count; $i++)
            {
                $dictionary.add($i, $hash[$i])
            }
            $dictionary
        }
        elseif ($Hash -is [System.Collections.Specialized.OrderedDictionary])
        {
            write-verbose -Message '$Hash is an OrderedDictionary'
            $dictionary = [ordered] @{}
            $keys = $Hash.keys
            foreach ($key in $keys)
            {
                $dictionary.add($key, $Hash[$key])
            }
            $dictionary
        }
        else
        {
            Write-Error -Message 'Enter a hash table, an array, or an ordered dictionary.'
        }
    }

    end {
        Write-Verbose -Message "Ending $($MyInvocation.Mycommand)"
    } #close end block

} #EndFunction ConvertTo-OrderedDictionary

$envConfigFile = Join-Path $PSScriptRoot "terraform" "${EnvironmentName}.tfvars.json"

if (!(Test-Path $envConfigFile)) {
    throw "Cannot find environment config file at '$envConfigFile'."
}

$envConfig = Get-Content $envConfigFile | ConvertFrom-Json
$keyVaultName = $envConfig.key_vault_name

$appConfig = Invoke-NativeCommand az keyvault secret show `
    --name "APP-CONFIG" `
    --vault-name $keyVaultName `
    --subscription $AzureSubscription `
    --query 'value' `
    --output tsv

$newConfig = Merge-Config $appConfig $ConfigKey $ConfigValue
$newConfig

if ($commit -eq $true) {
    $tempFile = New-TemporaryFile
    $newConfig | Out-File $tempFile

    try {
        Invoke-NativeCommand az keyvault secret set `
            --name "APP-CONFIG" `
            --file $tempFile `
            --vault-name $keyVaultName `
            --subscription $AzureSubscription `
            | Out-Null
    }
    finally {
        Remove-Item -Force $tempFile
    }

    Write-Host ""
    Write-Host "Successfully updated Key Vault secret." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Run again with the -Commit switch to update the Key Vault secret." -ForegroundColor Yellow
}
