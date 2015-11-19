using System;

namespace Cinder.Platform.Configuration
{
    public class ConfigurationSettings
    {
        public ConfigurationSettings()
        {}

        public string LocalCache { get; set; }
        public Uri PaasUri { get; set; }
        public Uri WebServerAddress { get; set; }
        public Uri ApiServerAddress { get; set; }
        public bool WorkOffline { get; set; }
    }
}
