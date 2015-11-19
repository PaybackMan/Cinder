using Prism.Commands;
using Prism.Mvvm;
using Cinder.Platform.Security.AuthProviders.DocFlock;
using Cinder.Platform.Security.AuthProviders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Windows.Navigation;
using Cinder.Platform.RestClients;
using Cinder.Platform.Configuration;

namespace Cinder.Windows.Core.Views
{
    public class SignInPageViewModel : BindableBase, INavigationAware
    {
        private string _title = "Sign In";
        private bool _oAuthElementsVisible = false;
        private bool _signInElementsVisible = true;
        private string _userName;
        private string _password;
        private IUnityContainer Container { get; set; }
        private IRestClient Client { get; set; }
        private ConfigurationSettings Settings { get; set; }
        public delegate void AuthenticationFailed();
        public event AuthenticationFailed AuthenticationFailedHandler;

//=================================================================================================================
/// <summary>
/// Ctor for view model
/// </summary>
//=================================================================================================================
        public SignInPageViewModel(INavigationService navigationService, IAuthenticationProvider authProvider, IUnityContainer container, IRestClient client, ConfigurationSettings settings)
        {
            this.NavigationService        = navigationService;
            this.AuthenticationProvider   = authProvider;
            this.NavigateCommand          = new DelegateCommand(GoBack);
            this.PickOAuthProviderCommand = new DelegateCommand(OnPickOAuthProvider);
            this.SignInCommand            = new DelegateCommand(OnSignInClicked);
            this.Settings                 = settings;
            this.Container                = container;
            this.Client                   = client;
        }
//=================================================================================================================
/// <summary>
/// Instance of MVVM Frameworks Navigation service.
/// </summary>
//=================================================================================================================
        private INavigationService NavigationService { get; set; }
//=================================================================================================================
/// <summary>
/// Instance of Cinder Authentication Service service.
/// </summary>
//=================================================================================================================
        private IAuthenticationProvider AuthenticationProvider { get; set; }
//=================================================================================================================
/// <summary>
/// Main Title of the concept..
/// </summary>
//=================================================================================================================
        public string Username
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value, "Username"); }
        }
//=================================================================================================================
/// <summary>
/// Main Title of the concept..
/// </summary>
//=================================================================================================================
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, "Password"); }
        }
//=================================================================================================================
/// <summary>
/// Main Title of the concept..
/// </summary>
//=================================================================================================================
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
//=================================================================================================================
/// <summary>
/// Delegate declarations for Navigation and UI acxtions.
/// </summary>
//=================================================================================================================
        public DelegateCommand PickOAuthProviderCommand { get; set; }
        public DelegateCommand NavigateCommand { get; set; }
        public DelegateCommand SignInCommand { get; set; }
//=================================================================================================================
/// <summary>
/// Visibility status of all UI elements required to support a non-OAuth style sign-in attempt.
/// </summary>
//=================================================================================================================
        public bool SignInElementsVisible
        {
            get { return _signInElementsVisible; }
            set
            {
                SetProperty(ref _signInElementsVisible, value, "SignInElementsVisible");
            }
        }
//=================================================================================================================
/// <summary>
/// Visibility status of all UI elements required to support a OAuth style sign-in attempt.
/// </summary>
//=================================================================================================================
        public bool OAuthElementsVisible
        {
            get { return _oAuthElementsVisible; }
            set
            {
                this.SignInElementsVisible = value != true;
                SetProperty(ref _oAuthElementsVisible, value, "OAuthElementsVisible");
            }
        }
        #region Event Handlers
//=================================================================================================================
/// <summary>
/// All kinds of technology-mediated contact details for a person or organization, including telephone, email, etc.
/// </summary>
//=================================================================================================================
        private void OnPickOAuthProvider()
        {
            this.OAuthElementsVisible = !this.OAuthElementsVisible;
        }
//=================================================================================================================
/// <summary>
/// 
/// </summary>
//=================================================================================================================
        private CustomCredentials GetCredentials()
        {

            #if (RELEASE)
                return new DocFlockCredentials()
                {
                    Email = "admin@Cinder.com",
                    Password = "Password"
                    //Email =  this.Username,
                    //Password = this.Password
                };
            #endif
            #if (DEVELOPMENT)
                return new DocFlockCredentials()
                {
                    //Email =  this.Username,
                    //Password = this.Password
                    Email = "admin@Cinder.com",
                    Password = "Password"
                };
            #endif
            #if (DEBUG)
            return new CustomCredentials()
            {
                //Email =  this.Username,
                //Password = this.Password
                Email = "admin@Cinder.com",
                Password = "Password"
            };

            #else
                return new DocFlockCredentials
                    {
                        Email    = "admin@Cinder.com",
                        Password = "Password"
                    };
          #endif
        }
//=================================================================================================================
/// <summary>
/// 
/// </summary>
//=================================================================================================================
        private async Task<bool> SignIn()
        {
            if (this.Settings.WorkOffline) return true;

            var credentials         = this.GetCredentials();
            this.Client.Credentials = credentials;

            // Reregister our shared IRestClient Instance. Now with valid credentials..

            this.Container.RegisterInstance<IRestClient>(this.Client);
            var response = await this.AuthenticationProvider.AuthenticateAsync(credentials);

            // Successfull Authentication if non-null IPrincipal returned in Authentication Response.

            if (response.Principal != null && response.Principal.Identity.IsAuthenticated)
            {
                this.Client.Credentials = credentials;

                // Reregister our shared IRestClient Instance. Now with valid credentials..

                this.Container.RegisterInstance<IRestClient>(this.Client);
                return true;
            }
            else if (response.Principal != null && !response.Principal.Identity.IsAuthenticated)
            {
                this.AuthenticationFailedHandler?.Invoke();
                return false;
            }
            return false;
        }
//=================================================================================================================
/// <summary>
/// Occurs when the user chooses to sign in with a non-OAuth method..
/// </summary>
//=================================================================================================================
        public async void OnSignInClicked()
        {
            await OnSignInClickedAsync();
        }
//=================================================================================================================
/// <summary>
/// This syncchronous method is called by its nonAsync counter part above to prevent deadlocks. See this article 
/// for more details ==> https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
/// </summary>
//=================================================================================================================
        public async Task OnSignInClickedAsync()
        {
            try
            {
                var result = await this.SignIn();
                if (result)
                {
                    this.NavigationService.Navigate("DocumentSearch", null);
                }

            }
            catch (Exception ex)
            {
                int g = 7;
            }
        }
//=================================================================================================================
/// <summary>
/// Event sync for back navigation
/// </summary>
//=================================================================================================================
        void GoBack()
        {
            this.NavigationService.GoBack();
        }
        #endregion

        public void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
           // throw new NotImplementedException();
        }

        public void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
           // throw new NotImplementedException();
        }
    }
}
