using System;

namespace Cinder.Platform.Configuration
{
    public class TestSettings : ConfigurationSettings
    {
        public TestSettings()
        {
            this.WebServerAddress = new Uri("http://Cinder-TEST:1235");
            this.ApiServerAddress = new Uri("http://Cinder-TEST:1235");
        }
    }
}
