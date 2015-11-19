using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Platform.Security.AuthProviders;

namespace Cinder.Platform.RestClients
{
    public enum TargetServer
    {
        Web,
        Api
    }

    public interface IRestClient
    {
        void SetContext(TargetServer targetServer);
        void SetHttpContext(string name);
        HttpContext Context { get; set; }
        Task SendMultiPartStreamAsync(MultiPartData data, HttpRequest request);
        BaseCredentials Credentials { get; set; } 
        Task<TResponse> SendGetRequestAsync<TResponse>(HttpRequest request) where TResponse : class;
        Task<TResponse> SendPutRequestAsync<TRequest, TResponse>(TRequest entity, HttpRequest request) where TRequest : class;
        Task<bool> SendDeleteRequestAsync<TRequest>(HttpRequest request) where TRequest : class;
        Task<TResponse> SendPostRequestAsync<TRequest, TResponse>(TRequest entity, HttpRequest request) where TRequest : class;
    }
}
