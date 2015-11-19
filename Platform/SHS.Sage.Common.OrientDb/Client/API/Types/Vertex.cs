using Orient.Client.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orient.Client.API.Types
{
    public class Vertex<T> : IBaseRecord where T: class, new()
    {
        internal Vertex(ORID orid, short oClassId, string oClassName, int oVersion, PayloadStatus payloadStatus)
        {
            PayloadStatus = payloadStatus;
            ORID = orid;
            OClassId = oClassId;
            OClassName = oClassName;
            OVersion = oVersion;
        }

        public ORID ORID
        {
            get;
            set;
        }

        public int OVersion
        {
            get;
            set;
        }

        public short OClassId
        {
            get;
            set;
        }

        public string OClassName
        {
            get;
            set;
        }

        internal PayloadStatus PayloadStatus { get; private set; }
    }
}
