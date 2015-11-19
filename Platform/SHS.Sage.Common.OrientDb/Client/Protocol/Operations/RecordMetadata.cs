﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orient.Client.Protocol.Operations
{
    internal class RecordMetadata : IOperation
    {
        public ORID _orid { get; set; }

        public RecordMetadata(ORID rid)
        {
            _orid = rid;
        }
        public Request Request(int sessionID)
        {
            Request request = new Request();

            request.AddDataItem((byte)OperationType.RECORD_METADATA);
            request.AddDataItem(sessionID);

            request.AddDataItem((short)_orid.ClusterId);
            request.AddDataItem((long)_orid.ClusterPosition);

            return request;
        }

        public ODocument Response(Response response)
        {
            ODocument document = new ODocument();

            if (response == null)
            {
                return document;
            }

            var reader = response.Reader;
            document.ORID = ReadORID(reader);
            document.OVersion = reader.ReadInt32EndianAware();

            return document;
        }

        private ORID ReadORID(BinaryReader reader)
        {
            ORID result = new ORID();
            result.ClusterId = reader.ReadInt16EndianAware();
            result.ClusterPosition = reader.ReadInt64EndianAware();
            return result;
        }
    }
}
