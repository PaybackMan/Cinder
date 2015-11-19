 
# Author      : Travis Plummer
# Date        : 5/16/2015
# Description : This script will associate your local powershell environment to a specific Azure
#               subscription. This association is enforced with a certificate that is specific to 
#               the logged in user..That's you dude :)
#               Reference : https://azure.microsoft.com/en-us/documentation/articles/powershell-install-configure/
 

# =============================================================================================
# Switch to Azure Resource Manager mode 
# =============================================================================================

     Switch-AzureMode AzureResourceManager
 
# =============================================================================================
# Sign in to add our account to our PS profile 
# =============================================================================================
 
     Add-AzureAccount # -environment "MSDN Dev/Test Pay-As-You-Go"

# =============================================================================================
#  Pull Azure Subscriptions File - Sign in then re-run this line if need be..
# =============================================================================================

     Get-AzurePubliCinderettingsFile

# =============================================================================================
# Import Subscription Cert into local certificate store..Modify path to suite your needs..
# Note the date included in the .publiCinderettings file, change this to the date you downloaded
# the cert on.
# =============================================================================================

    Import-AzurePubliCinderettingsFile -PubliCinderettingsFile "D:\Downloads\Visual Studio Ultimate with MSDN-6-5-2015-credentials.publiCinderettings" 

# =============================================================================================
# Recursively show all certs in the local certificate store..
# =============================================================================================

    Get-ChildItem -Recurse cert:\ | Where-Object {$_.Issuer -like 'Azure*'} | select friendlyname, subject, thumbprint   

# ================================================================================================
# List Subscriptions and set the current one to our Cinder subscription (MSDN Dev-Test Pay-As-You-Go)
# ================================================================================================

    Get-AzureSubscription | Select SubscriptionName

    Select-AzureSubscription -SubscriptionName 'MSDN Dev/Test Pay-As-You-Go'

# =============================================================================================
# Set the Current Subscription to the working Cinder account. This will be the default for every 
# Powershell session.
# =============================================================================================

    Select-AzureSubscription -SubscriptionName 'MSDN Dev/Test Pay-As-You-Go' -Default

# =============================================================================================
# Get available Storage Accounts for your Subscriptions
# =============================================================================================

    Get-AzureStorageAccount 

# ===============================================================================================
# Set up your default Storage Account as the one associated with the Cinder (BizSpark) subscription.
# ===============================================================================================

   Set-AzureSubscription -SubscriptionName "MSDN Dev/Test Pay-As-You-Go" -CurrentStorageAccountName "Cinder-Storage"

