using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public abstract class ServiceResponseBase
    {
        protected ServiceResponseBase()
        {
            Messages = new ServiceMessageList();
        }

        public ServiceMessageList Messages { get; set; }

        public bool Success
        {
            get
            {
                return !Messages.ContainsErrors;
            }
        }
    }
}
