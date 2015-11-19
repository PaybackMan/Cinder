using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Cinder.Core.Interfaces;
using Cinder.Clients.Bindings.Organizations;
using Cinder.Clients.Bindings.Providers;
using Cinder.Platform.Configuration;
using Cinder.Core.Domain.Administrative;
using Cinder.Core.Domain.Common;
using System.Collections.ObjectModel;

namespace Cinder.Clients.Bindings.Test.Organizations
{
    /// <summary>
    /// Summary description for OrganizationTest
    /// </summary>
    [TestClass]
    public class OrganizationTest
    {
        private static UnityContainer Container { get; set; }
        private TestContext testContextInstance;

        public OrganizationTest()
        {}

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
  
         [ClassInitialize()]
        public static async void MyClassInitialize(TestContext testContext)
        {
            var settings = ConfigurationFactory.GetSettings(BuildConfiguration.Debug);
            Container    = new UnityContainer();
          
            Container.RegisterType<IOrganizationService, OrganizationMobileService>();
            Container.RegisterInstance<ConfigurationSettings>(settings);
            var dataStore = Container.Resolve<OfflineDataStore>();
            await dataStore.InitializeAsync();
            await dataStore.AuthenticateAsync();
            Container.RegisterInstance<OfflineDataStore>(dataStore);            
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SaveOrganizationOffline()
        {
            var service = Container.Resolve<IOrganizationService>();
            var result  = service.SaveOrganization(SeedTestData.CreateTestOrganization()).Result;

            string test = result.Result.Name;
            int g = 8;

        }
    }
}
