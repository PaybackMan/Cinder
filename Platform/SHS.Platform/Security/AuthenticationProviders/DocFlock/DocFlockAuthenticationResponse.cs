using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security.AuthenticationProviders.DocFlock
{
//=======================================================================================================
/// <summary>
///  Used for sending the Auth Request Info to DocFlock..
/// </summary>
//=======================================================================================================
    public class DocFlockAuthenticationResponse
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }

    }
}

