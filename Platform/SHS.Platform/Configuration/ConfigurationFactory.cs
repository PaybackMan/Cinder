namespace Cinder.Platform.Configuration
{
    public enum BuildConfiguration
    {
        Debug,
        Test,
        Release,
        Development
    }
    public static class ConfigurationFactory
    {
        public static ConfigurationSettings GetSettings(BuildConfiguration configuration)
        {
            //#if (DEBUG)
            //            return new LocalSettings();
            //#elif (TEST)
            //              return new TestSettings();
            //#elif (DEVELOPMENT)
            //              return new DevSettings();
            //#elif (RELEASE)
            //              return new TestSettings();
            //#endif


            switch (configuration)
            {
                case (BuildConfiguration.Debug):
                    return new LocalSettings();
                    break;

                case (BuildConfiguration.Development):
                    return new DevSettings();
                    break;

                case (BuildConfiguration.Release):
                    return new ReleaseSettings();
                    break;

                case (BuildConfiguration.Test):
                    return new TestSettings();
                    break;
            }
            return null;
        }
    }
}
