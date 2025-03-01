using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;

namespace GFrag
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BGSetterPicker.SelectedIndex = Preferences.Get("BGindex", 0);
            //BGSetterPicker.SelectedIndex = 0;
            BackgroundManager.WebViewRegistry.Add(backgroundWebView);
            LoadSettings();
        }

        private void Def_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#4286F5");
        }

        private void Def_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#5A9AF5");
        }


        private void Save_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#05FA50");
        }

        private void Save_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#8BFF8D");
        }


        private void Reset_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#F14B5C");
        }

        private void Reset_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#F57C8A");
        }


        private void Import_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#FABC05");
        }

        private void Import_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#E6A300");
        }
        private void Logout_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#F33E51");
        }
        private void Logout_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#F25767");
        }

        private void LoadSettings()
        {
            if (Preferences.ContainsKey("DatabasePath"))
            {
                DatabasePathEntry.Text = Preferences.Get("DatabasePath", "");
            }
            else
            {
                DatabasePathEntry.Text = "";
            }

            if (Preferences.ContainsKey("AccountsDirectory"))
            {
                AccountsDirectoryEntry.Text = Preferences.Get("AccountsDirectory", "");
            }
            else
            {
                AccountsDirectoryEntry.Text = "";
            }

            if (Preferences.ContainsKey("DownloadsFolder"))
            {
                DownloadsFolderEntry.Text = Preferences.Get("DownloadsFolder", "");
            }
            else
            {
                DownloadsFolderEntry.Text = "";
            }

            if (Preferences.ContainsKey("ServerHost"))
            {
                ServerHostEntry.Text = Preferences.Get("ServerHost", "");
            }
            else
            {
                ServerHostEntry.Text = "";
            }

            if (Preferences.ContainsKey("ServerPort"))
            {
                ServerPortEntry.Text = Preferences.Get("ServerPort", "");
            }
            else
            {
                ServerPortEntry.Text = "";
            }

            if (Preferences.ContainsKey("ServerProtocol"))
            {
                if (Preferences.Get("ServerProtocol", "") == "https")
                {
                    HttpsSwitch.IsToggled = true;
                }
                else
                {
                    HttpsSwitch.IsToggled = false;
                }
            }
            else
            {
                HttpsSwitch.IsToggled = true;
            }
        }

        private async Task SaveSettingsFunc()
        {
            Preferences.Set("DatabasePath", DatabasePathEntry.Text);
            Preferences.Set("AccountsDirectory", AccountsDirectoryEntry.Text);
            Preferences.Set("DownloadsFolder", DownloadsFolderEntry.Text);
            Preferences.Set("ServerHost", ServerHostEntry.Text);
            Preferences.Set("ServerPort", ServerPortEntry.Text);

            if (HttpsSwitch.IsToggled)
            {
                Preferences.Set("ServerProtocol", "https");
            }
            else
            {
                Preferences.Set("ServerProtocol", "http");
            }

            await DisplayAlert("Settings Saved", "Your settings have been saved.", "OK");
        }
        private async void SaveSettingsButton_Clicked(object sender, EventArgs e)
        {
            await SaveSettingsFunc();
        }

        private async void ResetSettingsButton_Clicked(object sender, EventArgs e)
        {
            DatabasePathEntry.Text = "accounts.db";
            AccountsDirectoryEntry.Text = "";
            DownloadsFolderEntry.Text = "";
            ServerHostEntry.Text = "";
            ServerPortEntry.Text = "";
            HttpsSwitch.IsToggled = true;
            Preferences.Remove("DatabasePath");
            Preferences.Remove("AccountsDirectory");
            Preferences.Remove("DownloadsFolder");
            Preferences.Remove("ServerHost");
            Preferences.Remove("ServerPort");
            Preferences.Remove("ServerProtocol");
            await DisplayAlert("Settings Reset", "Your settings have been reset.", "OK");
        }

        private async void BrowseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var folderPath = await FolderPicker.Default.PickAsync();
                if (folderPath != null)
                {
                    AccountsDirectoryEntry.Text = folderPath.Folder.Path;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void BrowseDatabaseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var filePath = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { ".db" } },
                        { DevicePlatform.Android, new[] { ".db" } },
                        { DevicePlatform.WinUI, new[] { ".db" } },
                        { DevicePlatform.MacCatalyst, new[] { ".db" } },
                        { DevicePlatform.Tizen, new[] { ".db" } },
                    }),
                    PickerTitle = "Select Database File"
                });

                if (filePath != null)
                {
                    DatabasePathEntry.Text = filePath.FullPath;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        private async void ImportSettingsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { ".json" } },
                { DevicePlatform.Android, new[] { ".json" } },
                { DevicePlatform.WinUI, new[] { ".json" } },
                { DevicePlatform.MacCatalyst, new[] { ".json" } },
                { DevicePlatform.Tizen, new[] { ".json" } },
            }),
                    PickerTitle = "Select Settings File"
                });

                if (file != null)
                {
                    var json = await File.ReadAllTextAsync(file.FullPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    DatabasePathEntry.Text = settings["DatabasePath"];
                    AccountsDirectoryEntry.Text = settings["AccountsDirectory"];
                    DownloadsFolderEntry.Text = settings["DownloadsFolder"];
                    ServerHostEntry.Text = settings["ServerHost"];
                    ServerPortEntry.Text = settings["ServerPort"];
                    HttpsSwitch.IsToggled = settings["ServerProtocol"] == "https";
                    await SaveSettingsFunc();
                    await DisplayAlert("Settings Imported", "Your settings have been imported from the JSON file.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to import settings.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }
        private async void ExportSettingsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var settings = new Dictionary<string, string>
        {
            { "DatabasePath", DatabasePathEntry.Text },
            { "AccountsDirectory", AccountsDirectoryEntry.Text },
            { "DownloadsFolder", DownloadsFolderEntry.Text },
            { "ServerHost", ServerHostEntry.Text },
            { "ServerPort", ServerPortEntry.Text },
            { "ServerProtocol", HttpsSwitch.IsToggled? "https" : "http" }
        };

                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                var stream = new MemoryStream(bytes);

                var file = await FileSaver.Default.SaveAsync("settings.json", stream);

                if (file != null)
                {
                    await DisplayAlert("Settings Exported", "Your settings have been exported as a JSON file.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to export settings.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private async void BrowseDownloadsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var folderPath = await FolderPicker.Default.PickAsync();
                if (folderPath != null)
                {
                    DownloadsFolderEntry.Text = folderPath.Folder.Path;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void HttpsSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                Preferences.Set("ServerProtocol", "https");
            }
            else
            {
                Preferences.Set("ServerProtocol", "http");
            }
        }

        private async void ResetHwidButton_Clicked(object sender, EventArgs e)
        {
            bool confirmExit = await Application.Current.MainPage.DisplayAlert("Confirm Exit", "Do you really want to reset hwid and exit?", "Yes", "No");
            if (confirmExit)
            {
                var displayAlertHandler = new AlertService(Application.Current.MainPage);
                var message = await PrivateClass.ResetHwid(Preferences.Get("UserName", ""));
                if (message.message == "HWID reset successfully")
                {
                    await DisplayAlert("Hardware ID Reset", message.message, "OK");
                }
                else
                {
                    await DisplayAlert("Hardware ID Reset", message.message, "OK");
                }
            }
            //LoaderSpin.IsVisible = true;
            

        }
        private async void ExitAndLogoutButton_Clicked(object sender, EventArgs e)
        {
            bool confirmExit = await Application.Current.MainPage.DisplayAlert("Confirm Exit", "Do you really want to logout and exit?", "Yes", "No");
            if (confirmExit)
            {
                var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                var token = Preferences.Get("Token", string.Empty);
                bool Expired = await auth.ExpireToken(token);
                if (Expired)
                {
                    Preferences.Set("UserName", "Guest");
                    var viewModel = (AppShellViewModel)App.Current.MainPage.BindingContext;
                    viewModel.IsLoggedIn = false;
                    Application.Current.Quit();
                    //await Navigation.PushAsync(new LoginPage());
                }

            }


        }
        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    BackgroundManager.WebViewRegistry.Add(backgroundWebView);
        //}

        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    BackgroundManager.WebViewRegistry.Remove(backgroundWebView);
        //}
        private void BGSetterPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;

            // Check if the picker is not null and has a selected index
            if (picker != null && picker.SelectedIndex != -1)
            {
                // Get the selected item
                var selectedItem = picker.SelectedItem.ToString();

                // Optionally, you can also get the selected index
                var selectedIndex = picker.SelectedIndex;
                Preferences.Set("BGindex", selectedIndex);

                // Set preferences based on the selected item
                if (selectedItem == "Stars, Interactive")
                {
                    Preferences.Set("BGName", "background.html");
                }
                else if (selectedItem == "Neon Pink, Interactive, Dark")
                {
                    Preferences.Set("BGName", "bg2.html");
                }
                else if (selectedItem == "Interactive Particles")
                {
                    Preferences.Set("BGName", "bg.html");
                }
                else if (selectedItem == "80s Retro City")
                {
                    Preferences.Set("BGName", "80sbg.html");
                }
                else if (selectedItem == "VHS Retro Blue")
                {
                    Preferences.Set("BGName", "vhsbg.html");
                }

                // Reload the background or perform any other action
                BackgroundManager.UpdateAllWebViews();

                // Display the selected item
                DisplayAlert("Selected Item", $"You selected: {selectedItem} (Index: {selectedIndex})", "OK");
            }
        }

        private void backgroundWebView_HandlerChanged(object sender, EventArgs e)
        {
#if WINDOWS
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
        }
    }
}