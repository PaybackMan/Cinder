﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.Protocol
{
    internal class Connection : IDisposable
    {
        private TcpClient _socket;
        //private BufferedStream _networkStream;
        private NetworkStream _networkStream;
        private byte[] _readBuffer;

        internal string Hostname { get; set; }
        internal int Port { get; set; }
        internal ConnectionType Type { get; private set; }
        internal ODatabase Database { get; set; }

        internal string DatabaseName { get; private set; }
        internal ODatabaseType DatabaseType { get; private set; }
        internal string UserName { get; private set; }
        internal string UserPassword { get; private set; }

        internal string Alias { get; set; }
        internal bool IsReusable { get; set; }
        internal short ProtocolVersion { get; set; }
        internal int SessionId { get; private set; }
        internal bool IsActive
        {
            get
            {
                // If the socket has been closed by your own actions (disposing the socket, 
                // calling methods to disconnect), Socket.Connected will return false. If 
                // the socket has been disconnected by other means, the property will return 
                // true until the next attempt to send or receive information.
                // more info: http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
                // why not to use socket.Poll solution: it fails when the socket is being initialized
                // and introduces additional delay for connection check
                if ((_socket != null) && _socket.Connected)
                {
                    return true;
                }

                return false;
            }
        }
        internal ODocument Document { get; set; }

        internal Connection(string hostname, int port, string databaseName, ODatabaseType databaseType, string userName, string userPassword, string alias, bool isReusable)
        {
            Hostname = hostname;
            Port = port;
            Type = ConnectionType.Database;
            Alias = alias;
            IsReusable = isReusable;
            ProtocolVersion = 0;
            SessionId = -1;

            DatabaseName = databaseName;
            DatabaseType = databaseType;
            UserName = userName;
            UserPassword = userPassword;

            InitializeDatabaseConnection(databaseName, databaseType, userName, userPassword);
        }

        internal Connection(string hostname, int port, string userName, string userPassword)
        {
            Hostname = hostname;
            Port = port;
            Type = ConnectionType.Server;
            IsReusable = false;
            ProtocolVersion = 0;
            SessionId = -1;

            InitializeServerConnection(userName, userPassword);
        }

        internal ODocument ExecuteOperation(IOperation operation)
        {
            return ExecuteOperation<ODocument>(operation, (response) => ((IOperation)operation).Response(response));
        }

        internal void ExecuteOperation(IOperation operation, Action<Response> typeBuilder)
        {
            ExecuteOperation<object>(operation, (r) =>
                {
                    typeBuilder(r);
                    return null;
                });
        }

        internal T ExecuteOperation<T>(IOperation operation, Func<Response, T> typeBuilder)
        {
            if (_networkStream.DataAvailable) // need to empty the read buffer before running another command on this connection
            {
                var bytes = new byte[1024];
                var read = 0;
                do
                {
                    read = _networkStream.Read(bytes, 0, bytes.Length);
                } while (_networkStream.DataAvailable);
            }

            Request request = operation.Request(SessionId);
            byte[] buffer;

            foreach (RequestDataItem item in request.DataItems)
            {
                switch (item.Type)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "int":
                    case "long":
                        Send(item.Data);
                        break;
                    case "record":
                        buffer = new byte[2 + item.Data.Length];
                        Buffer.BlockCopy(BinarySerializer.ToArray(item.Data.Length), 0, buffer, 0, 2);
                        Buffer.BlockCopy(item.Data, 0, buffer, 2, item.Data.Length);
                        Send(buffer);
                        break;
                    case "bytes":
                    case "string":
                    case "strings":
                        //buffer = new byte[4 + item.Data.Length];
                        //Buffer.BlockCopy(BinarySerializer.ToArray(item.Data.Length), 0, buffer, 0, 4);
                        //Buffer.BlockCopy(item.Data, 0, buffer, 4, item.Data.Length);
                        //Send(buffer);

                        Send(BinarySerializer.ToArray(item.Data.Length));
                        Send(item.Data);
                        break;
                    default:
                        break;
                }
                _networkStream.Flush();
            }


            if (request.OperationMode == OperationMode.Synchronous)
            {
                try
                {
                    Response response = new Response(this);

                    response.Receive();

                    return typeBuilder(response);
                }
                catch (Exception exception)
                {
                    //reset connection as the socket may contains unread data and is considered unstable
                    Reconnect();
                    throw;
                }
            }
            else
            {
                return default(T);
            }
        }

        private void Reconnect()
        {
            Close();
            if (Type == ConnectionType.Database)
            {
                InitializeDatabaseConnection(DatabaseName, DatabaseType, UserName, UserPassword);
            }
            else
            {
                InitializeServerConnection(UserName, UserPassword);
            }
        }

        internal Stream GetNetworkStream()
        {
            return _networkStream;
        }

        internal void Close()
        {
            SessionId = -1;

            DbClose operation = new DbClose();
            ExecuteOperation(operation);

            if ((_networkStream != null) && (_socket != null))
            {
                _networkStream.Close();
                _socket.Close();
            }

            _networkStream = null;
            _socket = null;
        }

        public void Dispose()
        {
            Close();
        }

        public void Reload()
        {
            DbReload operation = new DbReload();
            var document = ExecuteOperation(operation);
            Document.SetField("Clusters", document.GetField<List<OCluster>>("Clusters"));
            Document.SetField("ClusterCount", document.GetField<short>("ClusterCount"));
        }

        #region Private methods

        private void InitializeDatabaseConnection(string databaseName, ODatabaseType databaseType, string userName, string userPassword)
        {
            _readBuffer = new byte[OClient.BufferLength];

            // initiate socket connection
            try
            {
                _socket = new TcpClient(Hostname, Port);
            }
            catch (SocketException ex)
            {
                throw new OException(OExceptionType.Connection, ex.Message, ex.InnerException);
            }

            //_networkStream = new BufferedStream(_socket.GetStream());
            _networkStream = _socket.GetStream();
            _networkStream.Read(_readBuffer, 0, 2);

            OClient.ProtocolVersion = ProtocolVersion = BinarySerializer.ToShort(_readBuffer.Take(2).ToArray());

            // execute db_open operation
            DbOpen operation = new DbOpen();
            operation.DatabaseName = databaseName;
            operation.DatabaseType = databaseType;
            operation.UserName = userName;
            operation.UserPassword = userPassword;

            Document = ExecuteOperation(operation);
            SessionId = Document.GetField<int>("SessionId");
        }

        private void InitializeServerConnection(string userName, string userPassword)
        {
            _readBuffer = new byte[OClient.BufferLength];

            // initiate socket connection
            try
            {
                _socket = new TcpClient(Hostname, Port);
            }
            catch (SocketException ex)
            {
                throw new OException(OExceptionType.Connection, ex.Message, ex.InnerException);
            }

            //_networkStream = new BufferedStream(_socket.GetStream());
            _networkStream = _socket.GetStream();
            _networkStream.Read(_readBuffer, 0, 2);

            OClient.ProtocolVersion = ProtocolVersion = BinarySerializer.ToShort(_readBuffer.Take(2).ToArray());

            // execute connect operation
            Connect operation = new Connect();
            operation.UserName = userName;
            operation.UserPassword = userPassword;

            Document = ExecuteOperation(operation);
            SessionId = Document.GetField<int>("SessionId");
        }

        private void Send(byte[] rawData)
        {
            //            Console.WriteLine(string.Join(", ", rawData.Select(x => x.ToString("X2"))));

            if ((_networkStream != null) && _networkStream.CanWrite)
            {
                try
                {
                    _networkStream.Flush(); // clear buffers before reading/writing the network stream, or this will throw an exception
                    _networkStream.Write(rawData, 0, rawData.Length);
                }
                catch (Exception ex)
                {

                    throw new OException(OExceptionType.Connection, ex.Message, ex.InnerException);
                }
            }
        }

        #endregion
    }
}
