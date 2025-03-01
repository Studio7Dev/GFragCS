using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace GFrag
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            // <Image Source="ubg.gif" Aspect="AspectFill" IsAnimationPlaying="True"></ Image >
            InitializeComponent();

            //BackgroundManager.WebViewRegistry.Add(backgroundWebView);
        }

        private void Login_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#4286F5");
        }

        private void Login_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#5A9AF5");
        }

        private void Register_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#FABC05");
        }

        private void Register_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#E6A300");
        }

        private void ResetHwid_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#C81C2D");
        }

        private void ResetHwid_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#F02236");
        }
        public class HwidRstMessage
        {
            public string message { get; set; }
        }


        private async void AnimateFrameBorderColor()
        {
            try
            {
                var colors2 = new Color[]
                {
                // Deep Blue
                Color.FromHex("#03055B"),
    
                // Blues (gradating towards Purple)
                Color.FromHex("#1A45A4"),
                Color.FromHex("#2E6BC5"),
                Color.FromHex("#4379C4"),
                Color.FromHex("#5287D3"),
    
                // Blue-Purple Transition
                Color.FromHex("#6C5CE7"),
                Color.FromHex("#7A49E8"),
                Color.FromHex("#8456E9"),
    
                // Purples
                Color.FromHex("#8F0CEA"),
                Color.FromHex("#9C3DEA"),
                Color.FromHex("#A93BEF"),
    
                // Light to Deep Rich Purples
                Color.FromHex("#B44AEF"),
                Color.FromHex("#C25AEF"),
                Color.FromHex("#D02EEF"),
    
                // Deep, Rich Purples
                Color.FromHex("#E011F5"),
                Color.FromHex("#E512E4"),
                Color.FromHex("#EA13E3")
                };
                // Ensure AFrame is not null
                //if (AFrame == null)
                //{
                //    Debug.WriteLine("AFrame is null");
                //    return;
                //}

                var A0_animation = new Animation(v =>
                {
                    var index = (int)(v * (colors2.Length - 1));
                    var color = colors2[index];
                    A0Frame.BorderColor = color;
                    //Debug.WriteLine($"A0Frame BorderColor set to {color}");
                }, 0, 1, easing: Easing.Linear);

                A0_animation.Commit(this, "A0FrameAnimation", length: 3500, repeat: () => true);

                //var A_animation = new Animation(v =>
                //{
                //    var index = (int)(v * (colors2.Length - 1));
                //    var color = colors2[index];
                //    AFrame.BorderColor = color;
                //    //Debug.WriteLine($"AFrame BorderColor set to {color}");
                //}, 0, 1, easing: Easing.Linear);

                //A_animation.Commit(this, "AFrameAnimation", length: 3500, repeat: () => true);

            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }
        }
        //private async Task LoginLogic()
        //{
        //    var displayAlertHandler = new AlertService(Application.Current.MainPage);
        //    LoaderSpin.IsVisible = true;
        //    var username = usernameEntry.Text;
        //    var password = passwordEntry.Text;
        //    Preferences.Set("UserName", username);
        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        LoaderSpin.IsVisible = false;
        //        await DisplayAlert("Auth Error", "Failed to login, Empty Fields!", "OK");
        //        return;
        //    }

        //    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));

        //    await Task.Run(async () =>
        //    {
        //        var hwid = auth.GetHwid();
        //        var token = await auth.LoginAsync(username, password, hwid);

        //        if (!string.IsNullOrEmpty(token))
        //        {
        //            Preferences.Set("Token", token);
        //            Debug.WriteLine("Session Token >>>>>>> " + token);

        //            // Verify the token and get expiration info
        //            var (isValid, expirationInfo) = await auth.VerifyAndGetTokenInfoAsync(token);

        //            if (!isValid)
        //            {
        //                await Device.InvokeOnMainThreadAsync(async () =>
        //                {
        //                    LoaderSpin.IsVisible = false;
        //                    await DisplayAlert("Auth Error", "Failed to verify session token!", "OK");
        //                });
        //            }
        //            else
        //            {
        //                await Device.InvokeOnMainThreadAsync(async () =>
        //                {
        //                    LoaderSpin.IsVisible = false;

        //                    // Log expiration info if available
        //                    if (!string.IsNullOrEmpty(expirationInfo))
        //                    {
        //                        Debug.WriteLine($"Token is valid. Expiration info: {expirationInfo}");
        //                    }
        //                    else
        //                    {
        //                        Debug.WriteLine("Token is valid, but expiration info could not be retrieved.");
        //                    }
        //                    var _expirationInfo = ParseExpirationInfo(expirationInfo);
        //                    await displayAlertHandler.DisplayAlertAsync("Auth Success", "Successfully Authenticated, Session Expiration: " + _expirationInfo.Expiration, "OK");
        //                    // Update ViewModel and navigate to the next page
        //                    var viewModel = (AppShellViewModel)App.Current.MainPage.BindingContext;
        //                    viewModel.IsLoggedIn = true;
        //                    await Navigation.PushAsync(new UploadPage());
        //                });
        //            }
        //        }
        //        else
        //        {
        //            await Device.InvokeOnMainThreadAsync(async () =>
        //            {
        //                LoaderSpin.IsVisible = false;
        //                await DisplayAlert("Auth Error", "Failed to login, Invalid Credentials.", "OK");
        //            });
        //        }
        //    });
        //}
        public class ExpirationInfo
        {
            [JsonPropertyName("expiration")]
            public DateTime Expiration { get; set; }
        }
        public static ExpirationInfo ParseExpirationInfo(string json)
        {
            try
            {
                // Deserialize the JSON string into the ExpirationInfo object
                var expirationInfo = JsonSerializer.Deserialize<ExpirationInfo>(json);
                return expirationInfo;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }
        //private async void LoginButton_Clicked(object sender, EventArgs e)
        //{
        //    loginButton.IsEnabled = false;
        //    await LoginLogic();
        //    loginButton.IsEnabled = true;
        //}

        //private async void OnEntryCompleted(object sender, EventArgs e)
        //{
        //    loginButton.IsEnabled = false;
        //    await LoginLogic();
        //    loginButton.IsEnabled = true;
        //    // Do something with the text
        //}

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            var viewModel = (AppShellViewModel)App.Current.MainPage.BindingContext;
            viewModel.IsLoggedIn = false;
        }
        //private async void RegisterButton_Clicked(object sender, EventArgs e)
        //{
        //    LoaderSpin.IsVisible = true;
        //    var username = usernameEntry.Text;
        //    var password = passwordEntry.Text;
        //    if (username == null || password == null)
        //    {
        //        LoaderSpin.IsVisible = false;
        //        await DisplayAlert("Auth Error", "Failed to Register, Empty Fields!", "OK");
        //        return;
        //    }
        //    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://"+ Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
        //    await Task.Run(async () => {
        //        var hwid = auth.GetHwid();
        //        bool registration = await auth.RegisterAsync(username, password, hwid);
        //        if (!registration)
        //        {
        //            await Device.InvokeOnMainThreadAsync(async () =>
        //            {
        //                LoaderSpin.IsVisible = false;
        //                await DisplayAlert("Auth Error", "Failed to Register, Contact Admin!", "OK");
        //            });
        //        }
        //        else
        //        {
        //            var token = await auth.LoginAsync(username, password, auth.GetHwid());
        //            if (!string.IsNullOrEmpty(token))
        //            {
        //                Preferences.Set("Token", token);
        //                Debug.WriteLine("Session Token >>>>>>> " + token);
        //                var isValid = await auth.VerifyTokenAsync(token);
        //                if (!isValid)
        //                {
        //                    await Device.InvokeOnMainThreadAsync(async () =>
        //                    {
        //                        LoaderSpin.IsVisible = false;
        //                        await DisplayAlert("Auth Error", "Failed to verify session token!", "OK"); ;
        //                    });
        //                }
        //                else
        //                {
        //                    await Device.InvokeOnMainThreadAsync(async () =>
        //                    {
        //                        LoaderSpin.IsVisible = false;
        //                        await Navigation.PushAsync(new UploadPage());
        //                    });
        //                }
        //                // Login successful, use the token
        //            }
        //            else
        //            {
        //                await Device.InvokeOnMainThreadAsync(async () =>
        //                {
        //                LoaderSpin.IsVisible = false;
        //                await DisplayAlert("Auth Error", "Failed to login, Invalid Creds.", "OK");
        //                });
        //                // Login failed
        //            }

        //        }
        //        // Handle the registration here
        //    });

            
        //}

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            AnimateFrameBorderColor();
        }

        private void passwordEntry_Completed(object sender, EventArgs e)
        {

        }
        private void backgroundWebView_HandlerChanged(object sender, EventArgs e)
        {
#if WINDOWS
#if DEBUG
            Debug.WriteLine("backgroundWebView_HandlerChanged");
#else
            if (sender is not WebView webView)
            {
                return;
            }

            if (webView.Handler?.PlatformView is not WebView2 webView2)
            {
                return;
            }

            webView2.CoreWebView2Initialized +=
                (sender, e) =>
                {
                    CoreWebView2Settings settings = webView2.CoreWebView2.Settings;

                    settings.AreDevToolsEnabled = false;
                    settings.IsGeneralAutofillEnabled = false;
                    settings.IsPinchZoomEnabled = false;
                    settings.AreDefaultContextMenusEnabled = false;
                    
                };
#endif
#endif
        }

        //private async void ResetHwidButton_Clicked(object sender, EventArgs e)
        //{
        //    var displayAlertHandler = new AlertService(Application.Current.MainPage);
        //    LoaderSpin.IsVisible = true;
        //    var username = usernameEntry.Text;
        //    var password = passwordEntry.Text;
        //    Preferences.Set("UserName", username);
        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        LoaderSpin.IsVisible = false;
        //        await DisplayAlert("Auth Error", "Failed to login, Empty Fields!", "OK");
        //        return;
        //    }
        //    var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));

        //    await Task.Run(async () => {

        //        var hwid = auth.GetHwid();
        //        string token = await auth.LoginAsync(username, password, hwid);
        //        if (token == "Invalid hardware ID")
        //        {
        //            await Device.InvokeOnMainThreadAsync(async () =>
        //                {
        //                    LoaderSpin.IsVisible = false;
        //                    var message = await PrivateClass.ResetHwid(username);
        //                    if (message.message == "HWID reset successfully")
        //                    {
        //                        await DisplayAlert("Hardware ID Reset", message.message, "OK");
        //                    }
        //                    else
        //                    {
        //                        await DisplayAlert("Hardware ID Reset", message.message, "OK");
        //                    }

        //                });
        //            return;
        //        }
        //        else
        //        {
        //            await Device.InvokeOnMainThreadAsync(async () =>
        //            {
        //                LoaderSpin.IsVisible = false;
        //                await DisplayAlert("Auth Error", "Failed to login, Invalid Credentials.", "OK");
        //            });
        //        }
        //    });

        //}
    }
}