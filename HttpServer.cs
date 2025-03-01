using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LightHTTP;
using LightHTTP.Handling;
using LightHTTP.Handling.Handlers;

namespace GFrag
{
    public class HttpServer
    {
        Dictionary<string, string> StaticRoutes = new Dictionary<string, string>
        {
            {"/particles.min.js", "particles.min.js"},
            {"/style.css", "style.css"},
            {"/style2.css", "style2.css"},
            {"/login.css", "login.css" }

        };


        Dictionary<string, Func<HttpListenerContext, CancellationToken, Task>> RouteWithFuncs = new Dictionary<string, Func<HttpListenerContext, CancellationToken, Task>>
        {
            { "/background", 
                async (context, cancellationToken) => {
                    await ServeFile(Preferences.Get("BGName", "background.html"), context);
                }
            },
            { "/login", 
                async (context, cancellationToken) => {
                    var displayAlertHandler = new AlertService(Application.Current.MainPage);
                    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                    bool HasBody = context.Request.HasEntityBody;
                    if (HasBody)
                    {
                        var Body = context.Request.InputStream;
                        Debug.WriteLine(Body);
                        var Encoding = context.Request.ContentEncoding;
                        var Reader = new System.IO.StreamReader(Body, Encoding);
                        var BodyString = await Reader.ReadToEndAsync();
                        var BodyDict = BodyString.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                        var Username = BodyDict["username"];
                        var Password = BodyDict["password"];
                        var hwid = auth.GetHwid();
                        var Token = await auth.LoginAsync(Username, Password, hwid);
                        if(Token != null)
                        {
                            if (!Token.Contains("Invalid"))
                            {

                                
                                // Update ViewModel and navigate to the next page

                                context.Response.ContentEncoding = Encoding.UTF8;
                                context.Response.ContentType = "text/plain";
                                var bytes = Encoding.UTF8.GetBytes("Success Login");
                                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                Preferences.Set("Token", Token.Trim());
                                Preferences.Set("UserName", Username);
                                await Task.Run( async () =>
                                {
                                    
                                    await Task.Delay(1200);
                                    var viewModel = (AppShellViewModel)App.Current.MainPage.BindingContext;
                                    viewModel.IsLoggedIn = true;
                                    await Application.Current.MainPage.Navigation.PushAsync(new UploadPage());
                                    await displayAlertHandler.DisplayAlertAsync("Auth Success", "Successfully Authenticated", "OK");
                                });
                            }
                            else
                            {
                                context.Response.ContentEncoding = Encoding.UTF8;
                                context.Response.ContentType = "text/plain";
                                var bytes = Encoding.UTF8.GetBytes("Failed Login");
                                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                            }
                        }
                        else
                        {
                            context.Response.ContentEncoding = Encoding.UTF8;
                            context.Response.ContentType = "text/plain";
                            var bytes = Encoding.UTF8.GetBytes("Failed Login");
                            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        }
                        Debug.WriteLine("Web Login Success: " + Token);

                    }
                    else
                    {
                        await ServeFile("login.html", context); 
                    }
                } 
            },
            { "/register", 
                async (context, cancellationToken) =>{
                    var displayAlertHandler = new AlertService(Application.Current.MainPage);
                    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                    bool HasBody = context.Request.HasEntityBody;
                    if (HasBody)
                    {
                        var Body = context.Request.InputStream;
                        Debug.WriteLine(Body);
                        var Encoding = context.Request.ContentEncoding;
                        var Reader = new System.IO.StreamReader(Body, Encoding);
                        var BodyString = await Reader.ReadToEndAsync();
                        var BodyDict = BodyString.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                        var Username = BodyDict["username"];
                        var Password = BodyDict["password"];
                        var hwid = auth.GetHwid();
                        bool registration = await auth.RegisterAsync(Username, Password, hwid);
                        if (!registration)
                        {
                                    context.Response.ContentEncoding = Encoding.UTF8;
                                    context.Response.ContentType = "text/plain";
                                    var bytes = Encoding.UTF8.GetBytes("Failed Register");
                                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            var token = await auth.LoginAsync(Username, Password, auth.GetHwid());
                            if (!string.IsNullOrEmpty(token))
                            {
                                Preferences.Set("Token", token);
                                Debug.WriteLine("> Web Session Token >>>>>>> " + token);
                                var isValid = await auth.VerifyTokenAsync(token);
                                if (!isValid)
                                {
                                    context.Response.ContentEncoding = Encoding.UTF8;
                                    context.Response.ContentType = "text/plain";
                                    var bytes = Encoding.UTF8.GetBytes("Failed Register");
                                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                    await Device.InvokeOnMainThreadAsync(async () =>
                                    {
                                        context.Response.ContentEncoding = Encoding.UTF8;
                                        context.Response.ContentType = "text/plain";
                                        var bytes = Encoding.UTF8.GetBytes("Failed Register");
                                        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                    });
                                }
                                else
                                {
                                    context.Response.ContentEncoding = Encoding.UTF8;
                                    context.Response.ContentType = "text/plain";
                                    var bytes = Encoding.UTF8.GetBytes("Success Register");
                                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                    await Task.Run(async () =>
                                    {
                                        await Task.Delay(1200);
                                        await Application.Current.MainPage.Navigation.PushAsync(new UploadPage());
                                    });
                                }
                                // Login successful, use the token
                            }
                            else
                            {
                                    context.Response.ContentEncoding = Encoding.UTF8;
                                    context.Response.ContentType = "text/plain";
                                    var bytes = Encoding.UTF8.GetBytes("Failed Register");
                                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                // Login failed
                            }

                        }
                    }
                    else
                    {
                        context.Response.ContentEncoding = Encoding.UTF8;
                        context.Response.ContentType = "text/plain";
                        context.Response.StatusCode = 405;
                        var bytes = Encoding.UTF8.GetBytes("Invalid Request Method, Not Allowed");
                        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    }

                }
            },
            { "/resethwid",
                async (context, cancellationToken) =>{
                    var displayAlertHandler = new AlertService(Application.Current.MainPage);
                    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                    bool HasBody = context.Request.HasEntityBody;
                    if (HasBody)
                    {
                        var Body = context.Request.InputStream;
                        Debug.WriteLine(Body);
                        var Encoding = context.Request.ContentEncoding;
                        var Reader = new System.IO.StreamReader(Body, Encoding);
                        var BodyString = await Reader.ReadToEndAsync();
                        var BodyDict = BodyString.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                        var Username = BodyDict["username"];
                        var Password = BodyDict["password"];
                        var hwid = auth.GetHwid();
                        await Task.Run(async () => {

                            var hwid = auth.GetHwid();
                            string token = await auth.LoginAsync(Username, Password, hwid);
                            if (token == "Invalid hardware ID")
                            {
                                
                                await Device.InvokeOnMainThreadAsync(async () =>
                                    {

                                        var message = await PrivateClass.ResetHwid(Username);
                                        if (message.message == "HWID reset successfully")
                                        {
                                            context.Response.ContentEncoding = Encoding.UTF8;
                                            context.Response.ContentType = "text/plain";
                                            var bytes = Encoding.UTF8.GetBytes("Success Reset HWID");
                                            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                        }
                                        else
                                        {
                                            context.Response.ContentEncoding = Encoding.UTF8;
                                            context.Response.ContentType = "text/plain";
                                            var bytes = Encoding.UTF8.GetBytes(message.message);
                                            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                        }

                                    });
                                return;
                            }
                            else
                            {
                                        var message = await PrivateClass.ResetHwid(Username);
                                        if (message.message == "HWID reset successfully")
                                        {
                                            context.Response.ContentEncoding = Encoding.UTF8;
                                            context.Response.ContentType = "text/plain";
                                            var bytes = Encoding.UTF8.GetBytes("Success Reset HWID");
                                            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                        }
                                        else
                                        {
                                            context.Response.ContentEncoding = Encoding.UTF8;
                                            context.Response.ContentType = "text/plain";
                                            var bytes = Encoding.UTF8.GetBytes(message.message);
                                            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                                        }
                            }
                        });
                    }
                    else
                    {
                        context.Response.ContentEncoding = Encoding.UTF8;
                        context.Response.ContentType = "text/plain";
                        context.Response.StatusCode = 405;
                        var bytes = Encoding.UTF8.GetBytes("Invalid Request Method, Not Allowed");
                        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    }
               
                }
            }
        };
        public static async Task ServeFile(string FileName, HttpListenerContext context)
        {
            try
            {
                var AbsPath = "";
                if (FileName.EndsWith(".html"))
                {
                    AbsPath = "Html/" + FileName;
                }
                else if (FileName.EndsWith(".css"))
                {
                    AbsPath = "Css/" + FileName;
                }
                else if (FileName.EndsWith(".js"))
                {
                    AbsPath = "Js/" + FileName;
                }
                else
                {
                    AbsPath = "Html/500.html";
                }

                string FileMimeType = MimeTypeHelper.GetMimeType(AbsPath);
                string FileContent = await GetFileContentAsync(AbsPath);
                var FileContentBytes = Encoding.UTF8.GetBytes(FileContent);

                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentType = FileMimeType;
                await context.Response.OutputStream.WriteAsync(FileContentBytes, 0, FileContentBytes.Length);
            } catch (Exception e)
            {
                var AbsPath = "Html/500.html";
                string FileMimeType = MimeTypeHelper.GetMimeType(AbsPath);
                string FileContent = await GetFileContentAsync(AbsPath);
                var FileContentBytes = Encoding.UTF8.GetBytes(FileContent);

                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentType = FileMimeType;
                await context.Response.OutputStream.WriteAsync(FileContentBytes, 0, FileContentBytes.Length);
            }
            //return (FileContentBytes, FileMimeType);
        }

        private static async Task<string> GetFileContentAsync(string FileName)
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
        public void NewHttpServer(int port)
        {
            var server = new LightHttpServer();


            server.Listener.Prefixes.Add("http://localhost:" + port.ToString()+"/");
            server.Listener.Prefixes.Add("http://127.0.0.1:" + port.ToString() + "/");

            foreach (KeyValuePair<string, string> route in StaticRoutes)
            {
                string Path = route.Key;
                string File = route.Value;

                server.HandlesPath(Path, async (context, cancellationToken) =>
                {
                    await ServeFile(File, context);

                });
                Debug.WriteLine($"Route: {route.Key}, File: {route.Value}");
            }

            foreach (var kvp in RouteWithFuncs)
            {
                server.HandlesPath(kvp.Key, async (context, cancellationToken) =>
                {
                    await kvp.Value(context, cancellationToken);

                });

            }

            server.RequestAccepted += async (e) =>
            {
                Debug.WriteLine($"Request accepted: {e.Request.Url} Code: {e.Response.StatusCode} Desc: {e.Response.StatusDescription}");
            };

            // finally start serving
            Task.Run(() => server.Start());
        }
    }
}
