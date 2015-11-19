# =============================================================================================
#  Create The Resource Group
# =============================================================================================

    New-AzureResourceGroup -Name SHS-Demo-Legacy -Location "South Central US"

# =============================================================================================
#  Deploy Demo environment...
# =============================================================================================

   New-AzureResourceGroupDeployment -Name SHS-Demo-Legacy -ResourceGroupName SHS-Demo-Legacy -TemplateFile ./SHS-ResourceGroupTemplate.txt -siteName "DocFlock-LDemo" -hostingPlanName "Free" -siteLocation "South Central US" -serverName "shs-ldemodb" -serverLocation "South Central US" -administratorLogin "shs-admin"  -databaseName "SimplicityHealth" 

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

   Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "SHS-Demo-Legacy" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 