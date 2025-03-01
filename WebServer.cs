using System;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.WindowsAppSDK.Runtime;

namespace GFrag
{
    internal class WebServer
    {
        private readonly HttpListener _listener;
        private readonly int _port;
        private readonly Route[] _routes;

        public WebServer(int port)
        {
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
            _routes = DiscoverRoutes();
        }

        public void Run()
        {
            try
            {
                _listener.Start();
                Console.WriteLine($"Server running on port {_port}...");
                Listen();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
            }
        }
        public void Stop()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
                Console.WriteLine("Server stopped.");
            }
            else
            {
                Console.WriteLine("Server is not running.");
            }
        }
        private Route[] DiscoverRoutes()
        {

            var routes = new Route[0];
            foreach (var method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var attr = (RouteAttribute)Attribute.GetCustomAttribute(method, typeof(RouteAttribute));
                if (attr != null)
                {
                    Array.Resize(ref routes, routes.Length + 1);
                    routes[routes.Length - 1] = new Route(attr.Path, attr.Method, method);
                }
            }
            return routes;
        }

        private void Listen()
        {
            _listener.BeginGetContext(ListenerCallback, _listener);
        }

        public Dictionary<string, string> defaultHeaders = new Dictionary<string, string>
{
    { "Access-Control-Allow-Origin", "*" },
    { "Access-Control-Allow-Methods", "GET, POST, OPTIONS" },
    { "Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept" },
    { "Access-Control-Max-Age", "3600" }
};

        public static Dictionary<string, Type> types = new Dictionary<string, Type>
    {
        { "info", typeof(InfoResponse) },
    };

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                var context = _listener.EndGetContext(result);
                var request = context.Request;
                var response = context.Response;

                Console.WriteLine($"Request: {request.HttpMethod} {request.Url}");

                foreach (var header in defaultHeaders)
                {
                    response.Headers[header.Key] = header.Value;
                }

                var route = _routes.FirstOrDefault(r => r.Method == request.HttpMethod && r.Path == request.Url.AbsolutePath);
                if (route != null)
                {

                    if (request.HttpMethod == "POST")
                    {

                        try
                        {
                            var requestBody = ReadRequestBody(request);
                            var responseBody = (string)route.MethodInfo.Invoke(this, new object[] { request, requestBody });
                            var buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
                            response.ContentLength64 = buffer.Length;
                            var output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        catch (Exception ex)
                        {
                            var requestBody = ReadRequestBody(request);
                            var responseBody = (Response)route.MethodInfo.Invoke(this, new object[] { request, requestBody });

                            string jsonResponse = JsonConvert.SerializeObject(responseBody);

                            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                            response.ContentLength64 = buffer.Length;

                            var output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                    }
                    else
                    {
                        if (route.MethodInfo.GetParameters().Length == 1)
                        {
                            var responseBody = (string)route.MethodInfo.Invoke(this, new object[] { request });
                            var buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
                            response.ContentLength64 = buffer.Length;
                            var output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        else if (route.MethodInfo.GetParameters().Length == 2)
                        {
                            var responseBodyTask = (Task<string>)route.MethodInfo.Invoke(this, new object[] { request, response });
                            var responseBody = responseBodyTask.Result;
                            var buffer = System.Text.Encoding.UTF8.GetBytes(responseBody);
                            response.ContentLength64 = buffer.Length;
                            var output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }

                    }
                }
                else
                {
                    response.StatusCode = 404;
                    response.Close();
                }

                Listen();
            }
        }

        private string ReadRequestBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        protected class RouteAttribute : Attribute
        {
            public string Path { get; set; }
            public string Method { get; set; }

            public RouteAttribute(string path, string method = "GET")
            {
                Path = path;
                Method = method;
            }
        }

        protected class Route
        {
            public string Path { get; set; }
            public string Method { get; set; }
            public MethodInfo MethodInfo { get; set; }

            public Route(string path, string method, MethodInfo methodInfo)
            {
                Path = path;
                Method = method;
                MethodInfo = methodInfo;
            }
        }

        public async Task<string> ServeFile(string FileName, HttpListenerRequest request, HttpListenerResponse response)
        {
            string FileMimeType = MimeTypeHelper.GetMimeType(FileName);
            response.ContentType = FileMimeType;

            string FileContent = await GetFileContentAsync(FileName);
            return FileContent;
        }

        private async Task<string> GetFileContentAsync(string FileName)
        {
            try
            {
                using Stream stream = await FileSystem.Current.OpenAppPackageFileAsync(FileName);
                using StreamReader reader = new StreamReader(stream, encoding: System.Text.Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"File {FileName} not found in the app package.");
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Failed to read {FileName}.", ex);
            }
        }

        [RouteAttribute("/background", "GET")]
        protected async Task<string> Hello(HttpListenerRequest request, HttpListenerResponse response)
        {
            return await ServeFile("Html/"+Preferences.Get("BGName", "background.html"), request, response);
        }
        [Route("/goodbye", "GET")]
        protected string Goodbye(HttpListenerRequest request)
        {
            return "Goodbye, World!";
        }

        [Route("/getinfo", "POST")]
        protected string GetInfoHandler(HttpListenerRequest rq, string requestBody)
        {
            var jsonHandler = new JsonHandler();
            Request request = jsonHandler.HandlePost<Request>(rq, requestBody);

            if (request == null)
            {
                Debug.WriteLine("Request object is null. Returning error response.");
                var AjsonResponseData = new Response()
                {
                    Data = "Invalid request. Could not deserialize JSON."
                };
                string AjsonResponse_ = JsonConvert.SerializeObject(AjsonResponseData);
                return AjsonResponse_;
            }

            if (types.ContainsKey(request.Type))
            {
                var responseType = types[request.Type];

                var responseInstance = Activator.CreateInstance(responseType);

                var properties = responseType.GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name == "HostName" && property.PropertyType == typeof(string))
                    {
                        property.SetValue(responseInstance, Environment.MachineName);
                    }
                    else if (property.Name == "Hwid" && property.PropertyType == typeof(string))
                    {
                        property.SetValue(responseInstance, PrivateClass.InfoGetHwid());
                    }
                    else if (property.Name == "Version" && property.PropertyType == typeof(string))
                    {
                        string version = PrivateClass.InfoGetVersion();
                        property.SetValue(responseInstance, version);
                    }
                }

                var RQ = new Response()
                {
                    Data = responseInstance
                };

                string Res = JsonConvert.SerializeObject(RQ);
                return Res;
            }
            var jsonResponseData = new Response()
            {
                Data = "Error Processing Request!"
            };
            string jsonResponse_ = JsonConvert.SerializeObject(jsonResponseData);
            return jsonResponse_;
        }
    }

    public class JsonHandler
    {
        public T HandlePost<T>(HttpListenerRequest request, string requestBody) where T : class
        {
            Debug.WriteLine($"Received POST request with body: {requestBody}");

            try
            {

                T response = JsonConvert.DeserializeObject<T>(requestBody);

                if (response == null)
                {
                    Debug.WriteLine("Deserialization returned null. Check the JSON structure.");
                }

                return response;
            }
            catch (JsonSerializationException jse)
            {
                Debug.WriteLine($"JSON Serialization Error: {jse.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        private void ProcessResponse(object response, Type responseType)
        {

        }
    }

    public class Request
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

    }
    public class Response
    {
        [JsonProperty("data")]
        public object Data { get; set; }
    }
    public class InfoResponse
    {
        [JsonProperty("hostname")]
        public string HostName { get; set; }
        [JsonProperty("hwid")]
        public string Hwid { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

}