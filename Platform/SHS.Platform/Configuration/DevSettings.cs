using System;

namespace Cinder.Platform.Configuration
{
   public class DevSettings : ConfigurationSettings
    {
        public DevSettings()
        {
            this.WebServerAddress = new Uri("http://Cinder-dev.docflock.com/");
            this.ApiServerAddress = new Uri("http://api-Cinder-dev.docflock.com/");
        }
    }
}
 