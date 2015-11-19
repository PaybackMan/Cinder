using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Unity.Windows;
using Cinder.Platform.Configuration;
using Cinder.Platform.RestClients;
using Cinder.Platform.Security.AuthProviders;


namespace Cinder.Windows.AdminTool
{
//=======================================================================================================
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
//=======================================================================================================
    sealed partial class App : PrismUnityApplication
    {
        private const string ProductName = "DocFlockWindows";
        private readonly CancellationToken _cancellationToken = new CancellationToken(false);

//=======================================================================================================
/// <summary>
/// 
/// </summary>
//=======================================================================================================
        public App()
        {
            InitializeComponent();
        }        
//=======================================================================================================
/// <summary>
/// 
/// </summary>
/// <returns></returns>
//=======================================================================================================
        protected override IUnityContainer CreateContainer()
        {
            return  base.CreateContainer();
        }
//=======================================================================================================
/// <summary>
///  Create an empty security principal and identity.
/// </summary>
//=======================================================================================================
        private void RegisterTypes()
        {
            // Configure the applications endpoints (servers)

            var settings = ConfigurationFactory.GetSettings(BuildConfiguration.Debug);
            var contextList = new List<HttpContext>
            {
                new HttpContext("Web")
                {
                    HostAddress = settings.WebServerAddress,
                    ProductName = ProductName
                },
                new HttpContext("Api")
                {
                    HostAddress = settings.ApiServerAddress,
                    ProductName = ProductName
                }
            };

            Container.RegisterInstance<ConfigurationSettings>(settings);
            Container.RegisterInstance<IRestClient>(new HttpClientProvider(contextList, _cancellationToken));
            Container.RegisterType(typeof(IAuthenticationProvider), typeof(Cinder.Core.Domain.Security.DocFlock.DocFlockAuthProvider));
        }
////=======================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="args"></param>
/// <returns></returns>
////=======================================================================================================
        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //ViewModelLocationProvider.Register(typeof(MainPage).ToString(), () => new MainPageViewModel(NavigationService));
            //ViewModelLocationProvider.Register(typeof(UserInput
            this.RegisterTypes();
            return base.OnInitializeAsync(args);
        }
//=======================================================================================================
/// <summary>
/// 
/// </summary>
//=======================================================================================================
        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((type) => this.Container.Resolve(type));
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

                return Type.GetType(viewModelName);
            });

            base.ConfigureViewModelLocator();
        }
//=======================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="args"></param>
/// <returns></returns>
//=======================================================================================================
        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                // Here we would load the application's resources.
                await this.LoadAppResources();
            }

            this.NavigationService.Navigate("SignIn", null);
        }
//=======================================================================================================
/// <summary>
/// We use this method to simulate the loading of resources from different sources asynchronously.
/// </summary>
/// <returns></returns>
//=======================================================================================================
        private async Task LoadAppResources()
        {
            await Task.Delay(1000);
        }
    }
}

