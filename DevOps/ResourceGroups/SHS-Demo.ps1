# =============================================================================================
#  Create The Resource Group
# =============================================================================================

    New-AzureResourceGroup -Name Cinder-Demo -Location "South Central US"

# =============================================================================================
#  Deploy Demo environment...
# =============================================================================================

     New-AzureResourceGroupDeployment -Name Cinder-Demo-Legacy -ResourceGroupName Cinder-Demo-Legacy -TemplateFile ./Cinder-Environment.json  -Cinder-StorageLocation "West US"  -Cinder-StorageName "Cinder-Demo" -Cinder-StorageType "Standard_LRS" -siteName "DocFlock-LDemo" -hostingPlanName "Free" -siteLocation "South Central US"

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

   Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "Cinder-Demo" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 