using System;

namespace Cinder.Platform.Configuration
{
    public class ReleaseSettings : ConfigurationSettings
    {
        public ReleaseSettings()
        {
            this.WebServerAddress = new Uri("http://api-Cinder-dev.docflock.com/");
            this.ApiServerAddress = new Uri("http://Cinder-dev.docflock.com");

            //this.WebServerAddress = new Uri("http://192.168.1.101:8081/");
            //this.ApiServerAddress = new Uri("http://192.168.1.101:8080/");
        }
    }
}

 