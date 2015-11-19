# =============================================================================================
#  Create The Resource Group
# =============================================================================================

     New-AzureResourceGroup -Name Cinder-Test -Location "South Central US"

# =============================================================================================
#  Deploy test environment...
# =============================================================================================

     New-AzureResourceGroupDeployment -Name Cinder-Test-Legacy -ResourceGroupName Cinder-Test-Legacy -TemplateFile ./Cinder-Environment.json  -Cinder-StorageLocation "West US"  -Cinder-StorageName "Cinder-Test" -Cinder-StorageType "Standard_LRS" -siteName "DocFlock-LTest" -hostingPlanName "Free" -siteLocation "South Central US"

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

   Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "Cinder-Test" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 


 