using System;

namespace Cinder.Platform.Configuration
{
    public class LocalSettings : ConfigurationSettings
    {
        public LocalSettings()
        {
            this.PaasUri = new Uri("https://cinderservices.azure-mobile.net/");
            this.LocalCache = "localstore.db";
            this.WebServerAddress = new Uri("http://localhost:30815/");
            this.ApiServerAddress = new Uri("http://localhost:1235/");
        }
    }
}
