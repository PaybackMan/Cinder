using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Cinder.Platform.Security.AuthProviders;
using System.Collections.Generic;
using System.Linq;
using ModernHttpClient;

namespace Cinder.Platform.RestClients
{
    public class HttpClientProvider : IRestClient
    {
        private HttpClient Client { get; set; }
        public HttpContext Context { get; set; }
        private CancellationToken CancellationToken { get; set; }
        public BaseCredentials Credentials { get; set; }
        public List<HttpContext> Contexts { get; set; }

//==================================================================================================================
/// <summary>
/// Required ctor accepting a context which should be used to stream in any auth tokens into the request..
/// </summary>
/// <param name="contexts"></param>
/// <param name="cancellationToken"></param>
/// <param name="credentials"></param>
//==================================================================================================================
        public HttpClientProvider(List<HttpContext> contexts, CancellationToken cancellationToken, BaseCredentials credentials)
        {
            this.CancellationToken = cancellationToken;
            this.Credentials       = credentials;

            if (this.ValidateHttpContextList(contexts)) {
                this.Contexts = contexts;
            }
            else {
                throw new InvalidOperationException("To register a list of HttpContext instances make sure each one has a unique name assigned to it.");
            }
        }
//==================================================================================================================
/// <summary>
/// Required ctor accepting a context which should be used to stream in any auth tokens into the request..
/// </summary>
/// <param name="context"></param>
/// <param name="cancellationToken"></param>
/// <param name="credentials"></param>
//==================================================================================================================
        public HttpClientProvider(HttpContext context, CancellationToken cancellationToken, BaseCredentials credentials)
        {
            this.CancellationToken = cancellationToken;
            this.Context           = context;
            this.Credentials       = credentials;
        }
//==================================================================================================================
/// <summary>
/// Required ctor accepting a context which should be used to stream in any auth tokens into the request..
/// </summary>
/// <param name="context"></param>
/// <param name="cancellationToken"></param>
//==================================================================================================================
        public HttpClientProvider(HttpContext context, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            this.Context           = context;
        }
//==================================================================================================================
/// <summary>
/// Required ctor accepting a context which should be used to stream in any auth tokens into the request..
/// </summary>
/// <param name="contexts"></param>
/// <param name="cancellationToken"></param>
//==================================================================================================================
        public HttpClientProvider(List<HttpContext> contexts, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;

            if (this.ValidateHttpContextList(contexts)) {
                this.Contexts = contexts;
            }
            else {
                throw new InvalidOperationException("To register a list of HttpContext instances make sure each one has a unique name assigned to it.");
            }
        }
//==================================================================================================================
/// <summary>
/// Ensures that each Registered HttpContext has a unique name..
/// </summary>
/// <param name="contexts"></param>
/// <returns></returns>
//==================================================================================================================
        private bool ValidateHttpContextList(IEnumerable<HttpContext> contexts)
        {
            return contexts.All(c => c.Name != null);
        }
//==================================================================================================================
/// <summary>
///  Sets the current HttpContext based on a supplied literal string.
/// </summary>
/// <param name="name"></param>
//==================================================================================================================
        public void SetHttpContext(string name)
        {
            if (this.Contexts.Any(c => c.Name == name))
            {
                this.Client = null;
                this.Context = this.Contexts.Single(c => c.Name == name);
            }
            else {
                throw new InvalidOperationException("HttpContext not found.");
            }
        }
//==================================================================================================================
/// <summary>
///  Sets the current HttpContext based on a supplied TargetServer enum value..
/// </summary>
/// <param name="targetServer"></param>
//==================================================================================================================
        public void SetContext(TargetServer targetServer)
        {
            if (targetServer == TargetServer.Web)
            {
                if (this.Contexts.Any(c => c.Name.ToUpper() == "WEB")) {
                    this.Client = null;
                    this.Context = this.Contexts.Single(c => c.Name.ToUpper() == "WEB");
                }
                else {
                    throw new InvalidOperationException("Web Server HttpContext not registered.");
                }
            }
            else
            {
                if (this.Contexts.Any(c => c.Name.ToUpper() == "API")) {
                    this.Client = null;
                    this.Context = this.Contexts.Single(c => c.Name.ToUpper() == "API");
                }
                else {
                    throw new InvalidOperationException("API Server HttpContext not registered.");
                }
            }
        }
//==================================================================================================================
/// <summary>
///   Set required security headers based on the credentials passed in. 
/// </summary>
//==================================================================================================================
        private void SetSecurityHeaders()
        {
            var securityHeaders = this.Credentials.GetAuthorizationHeaders();

            if (securityHeaders != null)
            {
                foreach (var kvp in securityHeaders)
                {
                    this.Client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
            }
        }
//==================================================================================================================
/// <summary>
///   Invoke an HTTP GET Request against the supplied URI.
/// </summary>
/// <typeparam name="TResponse">Expected Type of Return value of call</typeparam>
/// <param name="request">Additional options and routing information for the request</param>
/// <returns></returns>
//==================================================================================================================
        public async Task<TResponse> SendGetRequestAsync<TResponse>(HttpRequest request) where TResponse : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Setup the client and Payload..

            var jsonEntity              = JsonConvert.SerializeObject(request);
            var content                 = new StringContent(jsonEntity);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(this.GetContentType(request));
           
            var client = this.CreateHttpClient(request);
            if (client == null) throw new ArgumentNullException(nameof(client));

            var response = await client.GetAsync(this.ResolveRoute(request), this.CancellationToken);
            await response.Content.LoadIntoBufferAsync();
            
            // Post has failed for some reason so return null; 

            if (!this.IsStatusNormal(response.StatusCode)) {
                return null;

            }
            else
            {
                // POST was successfull. Deserialize the DTO and send the response back to the caller..

                return await this.PrepareResponse<TResponse>(response);
            }          
        }
//==================================================================================================================
/// <summary>
/// Invoke an HTTP PUT Request against the supplied URI.
/// </summary>
/// <typeparam name="TResponse">Expected Type of Return value of call</typeparam>
/// <typeparam name="TRequest">Type of input value to be sent to the server</typeparam>
/// <param name="entity">Instance of the POCO to be PUT</param>
/// <param name="request">Additional options and routing information for the request</param>
/// <returns></returns>
//==================================================================================================================
        public async Task<TResponse> SendPutRequestAsync<TRequest, TResponse>(TRequest entity, HttpRequest request) where TRequest : class
        {
            throw new NotImplementedException();
        }
//==================================================================================================================
/// <summary>
///   Invoke an HTTP DELETE Request against the supplied URI.
/// </summary>
/// <param name="request">Additional options and routing information for the request</param>
/// <returns></returns>
//==================================================================================================================
        public async Task<bool> SendDeleteRequestAsync<T>(HttpRequest request) where T : class
        {
            throw new NotImplementedException();
        }
//==================================================================================================================
/// <summary>
///  Convert a Stream to a byte array..
/// </summary>
/// <param name="stream"></param>
/// <returns></returns>
//==================================================================================================================
        private byte[] ConvertStream(Stream stream)
        {
            using (var memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
//==================================================================================================================
/// <summary>
/// 
/// </summary>
/// <param name="uri"></param>
/// <param name="method"></param>
/// <returns></returns>
//==================================================================================================================
        private HttpWebRequest CreateRequest(string uri, string method)
        {
            var request                         = (HttpWebRequest)WebRequest.Create(uri);
            request.Method                      = method;
            request.Accept                      = "application/json";
            request.Headers["Accept-Encoding"]  = "gzip, deflate";
            request.Headers["Accept-Language"]  = "en-US,en;q=0.8";
            request.Headers["Cache-Control"]    = "no-cache";
            request.Headers["X-Requested-With"] = "ScrubHubForiOS";
 
            return request;
        }
//==================================================================================================================
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="data"></param>
/// <returns></returns>
//==================================================================================================================
        public async Task SendMultiPartStreamAsync(MultiPartData data, HttpRequest request)
        {
            var requestContent = new MultipartFormDataContent();
            //requestContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            // Move through each byte array submitted and turn that into Content for the request..

            foreach (var item in data.Items) {                

                var content                 = new ByteArrayContent(item.Data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(item.ContentType);
                if (item.Comments != null)
                   requestContent.Add(content, "files", System.Net.WebUtility.UrlEncode(item.Comments) );
                //requestContent.Add(content, "files", item.Name);


                //  this MultipartFormDataContent form, HttpContent content, IDictionary<string, object> formValues, string name = null, string fileName = null)

                // var formValues = new Dictionary<string, object> { { "Comment", item.Comments } };


                // requestContent.Add(content, "files", item.Name);

                //var fComments = System.Net.WebUtility.UrlEncode(item.Comments);
                // requestContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("Comment", fComments));



                // requestContent.Headers.Add("Comment" + item.Name, item.Comments);
               

            }

            // Setup the client and Post the Content..

            var client = this.CreateHttpClient(request);
            var uri    = this.ResolveRoute(request);

            try
            {
                var response = await client.PostAsync(uri, requestContent);
                await response.Content.LoadIntoBufferAsync();

                if (!this.IsStatusNormal(response.StatusCode)) {
                    return;
                }                
            }
            catch (Exception ex)
            {
                int h = 8;
                // Need to log this..
            }
        }
//==================================================================================================================
/// <summary>
/// Returns the Required Content-Type for the Request or the Json Default..
/// </summary>
/// <param name="request"></param>
/// <returns></returns>
//==================================================================================================================
        private string GetContentType(HttpRequest request)
        {
            if (request.ContentType != null) {
                return request.ContentType;
            }
            else {
                return "application/json";
            }
        }
//==================================================================================================================
/// <summary>
/// Converts the HttpResponseMessage into either a requested POCO type or a 
/// more generic response via ApiResponse instance.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="response"></param>
/// <returns></returns>
//==================================================================================================================
        private async Task<TResponse> PrepareResponse<TResponse>(HttpResponseMessage response)
        {
            // return either some model class or that, plus more detailed info on the call. 

            var rawResponse = await response.Content.ReadAsStringAsync();
            if (typeof(TResponse).Name == "ApiResponse")
            {
                return JsonConvert.DeserializeObject<TResponse>(rawResponse);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<TResponse>>(rawResponse).Result;
                    //return JsonConvert.DeserializeObject<TResponse>(apiResponse.Result.ToString());
                }
                return default(TResponse);
            }
        }
//==================================================================================================================
/// <summary>
/// Invoke an HTTP POST Request against the supplied URI.
/// </summary>
/// <typeparam name="TResponse">Expected Type of Return value of call</typeparam>
/// <typeparam name="TRequest">Type of input value to be sent to the server</typeparam>
/// <param name="entity">Instance of the POCO to be Posted</param>
/// <param name="request">Additional options and routing information for the request</param>
/// <returns></returns>
//==================================================================================================================
        public async Task<TResponse> SendPostRequestAsync<TRequest, TResponse>(TRequest entity, HttpRequest request) where TRequest : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                // Setup the client and Payload..

                var jsonEntity              = JsonConvert.SerializeObject(entity);
                var content                 = new StringContent(jsonEntity);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(this.GetContentType(request));

                var client = this.CreateHttpClient(request);
                if (client == null) throw new ArgumentNullException(nameof(client));

                var route    = this.ResolveRoute(request);
                var response = await client.PostAsync(route, content, this.CancellationToken);

                await response.Content.LoadIntoBufferAsync();
                return await this.PrepareResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                int t = 9; // Need to log this..
            }
            return default(TResponse);
        }
//=================================================================================================================
/// <summary>
/// Combine Host Address with requested route to single Uri or override base host with fully qualified url 
/// property.
/// </summary>
/// <param name="request"></param>
/// <returns></returns>
//=================================================================================================================
        private Uri ResolveRoute(HttpRequest request)
        {
            return !string.IsNullOrEmpty(request.Route) ? new Uri(this.Context.HostAddress, request.Route) : request.Url;
        }
//==================================================================================================================
/// <summary>
/// Indicates if the status code supplied is considered abnormal or not..
/// </summary>
/// <param name="status"></param>
/// <returns></returns>
//==================================================================================================================
        protected bool IsStatusNormal(HttpStatusCode status)
        {
            return status == HttpStatusCode.OK || 
                   status == HttpStatusCode.Created || 
                   status == HttpStatusCode.NoContent || 
                   status == HttpStatusCode.Accepted;
        }
//==================================================================================================================
/// <summary>
///  Setup a new client, we'll work with that..
/// </summary>
/// <returns></returns>
//==================================================================================================================
        private HttpClient CreateHttpClient(HttpRequest request)
        {
            var handler = new NativeMessageHandler();

            if (this.Client == null && this.Context != null)
            {
                // Create the HttpClient with required default values..

                this.Client = new HttpClient(handler)
                {
                    MaxResponseContentBufferSize = 1053387136,
                    Timeout                      = new TimeSpan(0, 0, 20, 1, 1),
                    BaseAddress                  = this.Context.HostAddress
                };
                
                this.SetHeaders(request);
                this.SetSecurityHeaders();
                this.Client.BaseAddress = this.Context.HostAddress;
            }
            else
            {
                this.Client.BaseAddress = this.Context.HostAddress;
                this.SetHeaders(request);
                this.SetSecurityHeaders();
                return this.Client;
            }
            return this.Client;
        }
//==================================================================================================================
/// <summary>
///   Append any supplied values from the caller into the header collection..
/// </summary>
/// <returns></returns>
//==================================================================================================================
        private void SetHeaders(HttpRequest request)
        {
            // Add in any additional headers from the caller..
            
            if (request.ContentHeaders != null)
            {
                foreach (var kvp in request.ContentHeaders)
                {
                    this.Client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
