# =============================================================================================
#  Create The Resource Group
# =============================================================================================

     New-AzureResourceGroup -Name SHS-Test-Legacy -Location "South Central US"

# =============================================================================================
#  Deploy test environment...
# =============================================================================================

   New-AzureResourceGroupDeployment -Name SHS-Test-Legacy -ResourceGroupName SHS-Test-Legacy -TemplateFile ./SHS-ResourceGroupTemplate.txt -siteName "DocFlock-LTest" -hostingPlanName "Free" -siteLocation "South Central US" -serverName "shs-ltestdb" -serverLocation "South Central US" -administratorLogin "shs-admin"  -databaseName "SimplicityHealth" 

# ==============================================================================================
#  Tag the groups resources to ID the individual components and flag for auto-shut down scripts.
# ==============================================================================================

   Get-AzureResourceGroup | ? { $_.ResourceGroupName -match "SHS-Test-Legacy" } | Set-AzureResourceGroup -Tag @( @{ Name="autoShutdown"; Value=$true } ) 


 