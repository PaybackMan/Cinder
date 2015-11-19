using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public class ServiceMessageList : List<ServiceMessage>
    {
        public void AddInformationMessage(int id, string message, bool isSystem)
        {
            this.Add(new ServiceMessage(id, message, ServiceMessageType.INFORMATION, isSystem));
        }

        public void AddInformationMessage(string message)
        {
            this.Add(new ServiceMessage(message, ServiceMessageType.INFORMATION));
        }

        public void AddErrorMessage(int id, string message, bool isSystem)
        {
            this.Add(new ServiceMessage(id, message, ServiceMessageType.ERROR, isSystem));
        }

        public void AddErrorMessage(string message)
        {
            this.Add(new ServiceMessage(message, ServiceMessageType.ERROR));
        }

        public void AddExceptionMessage(Exception ex)
        {
            ServiceMessage m = new ServiceMessage();

            m.Id = ex.HResult;
            m.Type = ServiceMessageType.ERROR;
            m.Message = ServiceMessageStrings.Unhandled;
            m.IsSystem = false;
            m.ReferenceNumber = Guid.NewGuid();

            //if (Configuration.Errors.ReturnDetailedExceptionMessages)
            //{
            //    m.Message = ex.ToDetailString();
            //}

            this.Add(m);
        }

        public void AddValidationMessage(string message)
        {
            this.Add(new ServiceMessage(-1, message, ServiceMessageType.VALIDATION, false));
        }

        public bool ContainsErrors
        {
            get
            { return this.Any(message => message.Type != ServiceMessageType.INFORMATION); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (ServiceMessage m in this)
            {
                sb.AppendLine(m.ToString());
            }

            return sb.ToString();
        }
    }
}
