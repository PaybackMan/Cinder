using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public class ServiceMessage
    {
        public ServiceMessage() { }

        public ServiceMessage(string message, ServiceMessageType type)
        {
            this.Message = message;
            this.Type = type;
            this.ReferenceNumber = Guid.NewGuid();
        }

        public ServiceMessage(int id, string message, ServiceMessageType type, bool isSystem)
        {
            this.Id = id;
            this.Message = message;
            this.Type = type;
            this.ReferenceNumber = Guid.NewGuid();
            this.IsSystem = isSystem;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        public Guid? ReferenceNumber { get; set; }
        public ServiceMessageType Type { get; set; }
        public bool IsSystem { get; set; }

        public override bool Equals(object obj)
        {
            var msg = obj as ServiceMessage;

            if (msg != null)
                return msg.Id == this.Id;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Id:{0}, Message:{1}, ReferenceNumber:{2}, Type:{3}, IsSystem:{4}", this.Id, this.Message, this.ReferenceNumber, this.Type, this.IsSystem);
        }

    }
}
