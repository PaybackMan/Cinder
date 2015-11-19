using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public class  ApiResponse <TResult>
    {
        public ApiResponse()
        {
            Messages = new ServiceMessageList();
        }

        public ApiResponse(ServiceResponseBase serviceResponse)
        {
            Messages = serviceResponse.Messages;
        }

        public TResult Result { get; set; }
        public ServiceMessageList Messages { get; set; }
        public bool Success => !this.Messages.ContainsErrors;
    }
}
