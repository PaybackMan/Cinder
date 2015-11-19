# =============================================================================================
# Switch to Azure Resource Manager Mode (Make sure that you've selected the Cinder\BizSpark
# subscription or this will be on your dime!
# =============================================================================================
   
    Switch-AzureMode AzureResourceManager 

# =============================================================================================
#  Create The Resource Group
# =============================================================================================
     
     New-AzureResourceGroup -Name Cinder-testRGonly -Location "South Central US"

# =============================================================================================
#  Deploy Dev environment...
# =============================================================================================

     New-AzureResourceGroupDeployment -Name Cinder-Development-Legacy -ResourceGroupName Cinder-Development-Legacy -TemplateFile ./Cinder-Environment.json  -StorageLocation "West US"  -StorageName "Cinder-LDev" -StorageType "Standard_LRS" -siteName "DocFlock-LDev" -hostingPlanName "Free" -siteLocation "South Central US"

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

    Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "Cinder-Development" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 
