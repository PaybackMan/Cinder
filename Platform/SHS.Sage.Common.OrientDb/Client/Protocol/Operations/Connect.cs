﻿using System.Linq;
using Orient.Client.Protocol.Serializers;
using System.Text;

namespace Orient.Client.Protocol.Operations
{
    internal class Connect : IOperation
    {
        internal string UserName { get; set; }
        internal string UserPassword { get; set; }

        public Request Request(int sessionID)
        {
            Request request = new Request();

            // standard request fields
            request.DataItems.Add(new RequestDataItem() { Type = "byte", Data = BinarySerializer.ToArray((byte)OperationType.CONNECT) });
            request.DataItems.Add(new RequestDataItem() { Type = "int", Data = BinarySerializer.ToArray(sessionID) });
            // operation specific fields
            if (OClient.ProtocolVersion > 7)
            {
                request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(OClient.DriverName) });
                request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(OClient.DriverVersion) });
                request.DataItems.Add(new RequestDataItem() { Type = "short", Data = BinarySerializer.ToArray(OClient.ProtocolVersion) });
                request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(OClient.ClientID) });
            }
            if (OClient.ProtocolVersion > 21)
            {
                request.DataItems.Add(new RequestDataItem { Type = "string", Data = BinarySerializer.ToArray(OClient.SerializationImpl) });
            }
            if (OClient.ProtocolVersion > 25)
            {
                request.DataItems.Add(new RequestDataItem { Type = "bool", Data = BinarySerializer.ToArray(OClient.TokenSession) });
            }
            request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(UserName) });
            request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(UserPassword) });

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

            // operation specific fields
            var sessionId = reader.ReadInt32EndianAware();
            document.SetField("SessionId", sessionId);
            if (OClient.ProtocolVersion > 25)
            {
                var len = reader.ReadInt32EndianAware();
                var tokenBytes = new byte[len];
                reader.Read(tokenBytes, 0, len);
                document.SetField("Token", ASCIIEncoding.ASCII.GetString(tokenBytes));
            }

            return document;
        }
    }
}
