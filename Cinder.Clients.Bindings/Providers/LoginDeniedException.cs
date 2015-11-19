using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Clients.Bindings.Providers
{

    internal class LoginDeniedException : Exception
    {
        public LoginDeniedException()
        {
        }

        public LoginDeniedException(string message) : base(message)
        {
        }

        public LoginDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
