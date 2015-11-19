using System;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;
using Cinder.Core.Interfaces;
using Cinder.Platform.RestClients;
using Cinder.Clients.Bindings.Providers;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;

namespace Cinder.Clients.Bindings.Organizations
{
    public class OrganizationMobileService : IOrganizationService
    {
        private readonly OfflineDataStore _dataStore;
        private static IMobileServiceSyncTable<Organization> _controller = null;

//==============================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="dataStore"></param>
//==============================================================================================
        public OrganizationMobileService(OfflineDataStore dataStore)
        {
            _dataStore = dataStore;
        }
//==============================================================================================
/// <summary>
/// Initialize the _controller table.
/// </summary>
/// <returns></returns>
//==============================================================================================
        private async Task InitializeAsync()
        {
            if (_controller == null)
            {             
                _controller = _dataStore.CloudService.GetSyncTable<Organization>();                
            }
        }
//==============================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="OrganizationId"></param>
/// <returns></returns>
//==============================================================================================
        public async Task<ApiResponse<bool>> DeleteOrganization(int OrganizationId)
        {
            throw new NotImplementedException();
        }
//==============================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="OrganizationId"></param>
/// <returns></returns>
//==============================================================================================
        public async Task<ApiResponse<Organization>> GetOrganization(int OrganizationId)
        {
            throw new NotImplementedException();
        }
//==============================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="organization"></param>
/// <returns></returns>
//==============================================================================================
        public async Task<ApiResponse<Organization>> SaveOrganization(Organization organization)
        {
            if (organization.Id == null)
            {
                // Add the item to the sync table

                await InitializeAsync();
                await _controller.InsertAsync(organization);
            }
            else
            {
                await InitializeAsync();
                await _controller.UpdateAsync(organization);
            }

            return null;         
        }
//==============================================================================================
/// <summary>
/// Refresh the async table from the cloud
/// </summary>
/// <param name="row"></param>
/// <returns></returns>
//==============================================================================================
        public async Task RefreshAsync()
        {
           await InitializeAsync();
          
            if (_dataStore.IsAuthenticated)
            {
                try
                {
                    // Do the Pushes
                    await _dataStore.CloudService.SyncContext.PushAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("EXCEPTION:{0}", ex.Message));
                }

                // Do the pulls
                await _controller.PullAsync("tablequery", _controller.CreateQuery());
            }
        }
    }
}
