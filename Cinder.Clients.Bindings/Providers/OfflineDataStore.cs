using System;
using Cinder.Core.Domain.Administrative;
using Cinder.Platform.Configuration;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cinder.Clients.Bindings.Providers
{
    public class OfflineDataStore
    {           
        private bool _initialized = false;
        private ConfigurationSettings ConfigurationSettings { get;  set; }
        private static OfflineDataStore _instance = null;

//==============================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="settings"></param>
//==============================================================================================
        public OfflineDataStore(ConfigurationSettings settings)
        {
            this.ConfigurationSettings = settings;           
        }
//==============================================================================================
/// <summary>
/// Initialize the Data Store asynchronously
/// </summary>
/// <returns></returns>
//==============================================================================================
        public async Task InitializeAsync()
        {
          // Create the two sides of the service


            try
            {
             

                CloudService = new MobileServiceClient(this.ConfigurationSettings.PaasUri);
                LocalCacheService = new MobileServiceSQLiteStore(this.ConfigurationSettings.LocalCache);

                // Define the table structure
                ((MobileServiceSQLiteStore)LocalCacheService).DefineTable<Organization>();

                // Initialize the local store
                await CloudService.SyncContext.InitializeAsync(LocalCacheService);
            }
            catch (Exception ex)
            {

                int g = 8;
            }


         
        }
//==============================================================================================
/// <summary>
/// Provide an async authentication mechanism
/// </summary>
/// <returns>Task (async)</returns>
//==============================================================================================
        public async Task AuthenticateAsync()
        {
            if (!IsAuthenticated)
            {
                try
                {
                    User = await CloudService.LoginAsync(MobileServiceAuthenticationProvider.Google, null);
                    Debug.WriteLine("AUTH:{0}:{1}", User.UserId, User.MobileServiceAuthenticationToken);
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    Debug.WriteLine("EXCEPTION:{0}:{1}:{2}:{3}:{4}", ex.GetType(),
                        ex.HResult, ex.Message, ex.Request.RequestUri, ex.Response.ReasonPhrase);
                    throw new LoginDeniedException();
                }
            }
        }
        #region Properties
//==============================================================================================
/// <summary>
/// True if the stores are initialized
/// </summary>
//==============================================================================================
        public bool IsInitialized
        {
            get { return _initialized; }
        }
//==============================================================================================
/// <summary>
/// True if the cloud service is authenticated
/// </summary>
//==============================================================================================
        public bool IsAuthenticated
        {
            get { return (User != null); }
        }
//==============================================================================================
/// <summary>
/// 
/// </summary>
//==============================================================================================
        public MobileServiceClient CloudService { get; private set; }
//==============================================================================================
/// <summary>
/// 
/// </summary>
//==============================================================================================
        public MobileServiceLocalStore LocalCacheService { get; private set; }
//==============================================================================================
/// <summary>
///         
/// </summary>
//==============================================================================================
        public MobileServiceUser User { get; private set; }
        #endregion       
    }
}
