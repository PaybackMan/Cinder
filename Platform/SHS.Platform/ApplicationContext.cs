using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform
{
    public enum ApplicationSize
    {
        Small,
        Medium,
        Large
    }

    public enum ThemeType
    {
        Light,
        Dark
    }

    public enum Orientation
    {
        Landscape,
        Portrait
    }

    public static class ApplicationContext
    {
        public static Orientation Orientation { get; set; }
        public static ThemeType Theme { get; set; }
        public static ApplicationSize ApplicationSize { get; set; }
        public static IPrincipal Principal { get; set; }
        public static string ApplicationName { get; set; }
        public static bool IsLoggedIn { get; set; }

    }
}
