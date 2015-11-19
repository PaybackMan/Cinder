using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security
{
    public class CinderIdentity : IIdentity
    {
        public CinderIdentity(string name, string password, string customerId, bool isAuthenticated)
        {
            Name            = name;
            Password        = password;
            IsAuthenticated = isAuthenticated;
            CustomerId      = customerId;
        }

        public string AuthenticationType
        {
            get { return "CinderAuth"; }
        }

        public bool IsAuthenticated { get; private set; }

        public string Name { get; private set; }

        public string Password { get; private set; }

        public string CustomerId { get; private set; }
    }
}
