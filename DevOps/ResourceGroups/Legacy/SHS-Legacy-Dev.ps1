# =============================================================================================
#  Switch to Azure Resource Manager Mode (Make sure that you've selected the SHS\BizSpark
# subscription or this will be on your dime!
# =============================================================================================
   
    Switch-AzureMode AzureResourceManager 

# =============================================================================================
#  Create The Resource Group
# =============================================================================================
     
     New-AzureResourceGroup -Name SHS-Development-Legacy -Location "South Central US"

# =============================================================================================
#  Deploy Dev environment...
# =============================================================================================
	   
	 New-AzureResourceGroupDeployment -Name SHS-Development-Legacy -ResourceGroupName SHS-Development-Legacy -TemplateFile ./SHS-Environment.json  -storageName "shsdevl" -storageType "Standard_LRS" -siteName "DocFlock-LDev" -hostingPlanName "Free" -siteLocation "South Central US" -DBServerName "shs-ldevdb" -DBServerLocation "South Central US" -DBServerAdminLogin "shs-admin" 

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

    Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "SHS-Development-Legacy" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 
