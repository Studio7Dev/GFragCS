using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
namespace GFrag;

public partial class FilesPage : ContentPage
{
    private UwpNotifs _uwpnotifs;
    
    public FilesPage()
    {
        InitializeComponent();
        BackgroundManager.WebViewRegistry.Add(backgroundWebView);
        _uwpnotifs = new UwpNotifs();
        LoadFiles();
    }


    private void DeleteUnHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#FF0000");
    }

    private void DeleteHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#b30000");
    }

    private void DownloadUnHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#51B4FF");
    }

    private void DownloadHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#4590C7");
    }
    private void ButtonHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#151515");
    }

    private void ButtonUnHover(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#0D0D0D");
    }

    private async void AnimateFrameBorderColor()
    {
        try
        {
            var colors = new Color[]
            {
            Color.FromHex("#800080"),
            Color.FromHex("#8A158A"),
            Color.FromHex("#942A94"),
            Color.FromHex("#9E3F9E"),
            Color.FromHex("#A854A8"),
            Color.FromHex("#B269B2"),
            Color.FromHex("#BC7EBB"),
            Color.FromHex("#C693C6"),
            Color.FromHex("#D0A8D0"),
            Color.FromHex("#DABDDA"),
            Color.FromHex("#E4D2E4"),
            Color.FromHex("#EDE7ED"),
            Color.FromHex("#F6F2F6"),
            Color.FromHex("#FFEEFF"),
            Color.FromHex("#FFD3D3"),
            Color.FromHex("#FFB8B8"),
            Color.FromHex("#FF9D9D"),
            Color.FromHex("#FF8282"),
            Color.FromHex("#FF6767"),
            Color.FromHex("#FF0000")
            };

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
            var redGradient = new Color[]
            {
                    Color.FromHex("#4B0082"), // Indigo
                    Color.FromHex("#6A0DAD"), // Violet
                    Color.FromHex("#8A2BE2"), // Blue Violet
                    Color.FromHex("#9370DB"), // Medium Purple
                    Color.FromHex("#A020F0"), // Violet
                    Color.FromHex("#BA55D3"), // Medium Orchid
                    Color.FromHex("#DA70D6"), // Orchid
                    Color.FromHex("#EE82EE"), // Violet
                    Color.FromHex("#DDA0DD"), // Plum
                    Color.FromHex("#E6E6FA"), // Lavender
                    Color.FromHex("#F0E68C"), // Khaki (optional for a slight contrast)
                    Color.FromHex("#800080"), // Purple
                    Color.FromHex("#9932CC"), // Dark Orchid
                    Color.FromHex("#8B008B"), // Dark Magenta
                    Color.FromHex("#4B0082"), // Indigo (repeated for depth)
            };
            // Ensure Frames is not null
            if (AFrame == null)
            {
                Debug.WriteLine("AFrame is null");
                return;
            }
            if (ProgressFrame == null)
            {
                Debug.WriteLine("ProgressFrame is null");
                return;
            }
            //if (A0Frame == null)
            //{
            //    Debug.WriteLine("A0Frame is null");
            //    return;
            //}
            if (CFrame == null)
            {
                Debug.WriteLine("CFrame is null");
                return;
            }


            //var A0_animation = new Animation(v =>
            //{
            //    var index = (int)(v * (colors.Length - 1));
            //    var color = colors[index];
            //    A0Frame.BorderColor = color;
            //    //Debug.WriteLine($"A0Frame BorderColor set to {color}");
            //}, 0, 1, easing: Easing.Linear);

            //A0_animation.Commit(this, "A0FrameAnimation", length: 3500, repeat: () => true);

            var A_animation = new Animation(v =>
            {
                var index = (int)(v * (redGradient.Length - 1));
                var color = redGradient[index];
                AFrame.BorderColor = color;
                //Debug.WriteLine($"AFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            A_animation.Commit(this, "AFrameAnimation", length: 3500, repeat: () => true);

            var B_animation = new Animation(v =>
            {
                var index = (int)(v * (redGradient.Length - 1));
                var color = redGradient[index];
                ProgressFrame.BorderColor = color;
                //Debug.WriteLine($"BFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            B_animation.Commit(this, "BFrameAnimation", length: 3500, repeat: () => true);

            var C_animation = new Animation(v =>
            {
                var index = (int)(v * (redGradient.Length - 1));
                var color = redGradient[index];
                CFrame.BorderColor = color;
                //Debug.WriteLine($"BFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            C_animation.Commit(this, "CFrameAnimation", length: 3500, repeat: () => true);


        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private async Task LoadFilesContinuously()
    {
        List<FileData> previousFilesData = null; // To store the previous files data
        string PreData = "";
        while (true) // Infinite loop to run continuously
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                Debug.WriteLine("Checking for new files data...");
                try
                {
                    // Fetch the current files data
                    List<FileData> filesData = await GetFileDataAsync();
                    string FilesDataString = string.Join(", ", filesData.Select(f => f.FileName));
                    if (FilesDataString != PreData)
                    {
                        PreData = FilesDataString;
                        previousFilesData = filesData;
                        // Debug output for previous and current file data
                        Debug.WriteLine("Previous Files Data: " + (previousFilesData != null ? string.Join(", ", previousFilesData.Select(f => f.FileName)) : "None"));
                        Debug.WriteLine("Current Files Data: " + string.Join(", ", filesData.Select(f => f.FileName)));
                        await LoadFiles();
                    }


                    //// Compare the new filesData with the previousFilesData
                    //if (!AreFileListsEqual(previousFilesData, filesData))
                    //{
                    //    await LoadFiles();
                    //    previousFilesData = filesData; // Update the previousFilesData
                    //    Debug.WriteLine("Refreshed Success.......");
                    //}
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Failed to retrieve files from server")
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                }
            });

            await Task.Delay(2000); // Wait for 2 seconds before the next iteration
        }
    }

    private static async Task<List<FileData>> GetFileDataAsync()
    {
        HttpClient httpClient = new HttpClient();
        string url = Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/get_shared_files_by_user?username=" + Preferences.Get("UserName", "");
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var fileRowData = JsonSerializer.Deserialize<List<FileData>>(jsonResponse);

        return fileRowData;
    }

    //// Helper method to compare two lists of FileRowData
    //private bool AreFileListsEqual(List<FileData> list1, List<FileRowData> list2)
    //{
    //    if (list1 == null || list2 == null)
    //        return false;

    //    if (list1.Count != list2.Count)
    //        return false;

    //    return !list1.Except(list2).Any() && !list2.Except(list1).Any();
    //}
    private async Task LoadFiles()
    {
        //DownloadProgress.IsVisible = true; // Show the progress bar
        //_cancellationTokenSource = new CancellationTokenSource();

        //// Start the infinite progress simulation
        //await SimulateInfiniteProgress(_cancellationTokenSource.Token);
        await Device.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                Debug.WriteLine("Loading files...");
                DatabaseManager databaseManager = new DatabaseManager();
                List<FileRowData> filesData = await databaseManager.GetDBFiles(DownloadProgress);
                Debug.WriteLine("List of Files C>>>>>>>>>>>>>>>>>" +filesData.Count);
                filesCollectionView.ItemsSource = filesData;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        });
        //_cancellationTokenSource?.Cancel(); // Stop the infinite progress
        //DownloadProgress.IsVisible = false; // Hide the progress bar

    }
    bool Loaded = false;
    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //}

    private async void RefreshButton_Clicked(object sender, EventArgs e)
    {
        await Task.Run(async () =>
        {
            await LoadFiles();
        });

    }

    private async void RefreshStorage_Clicked(object sender, EventArgs e)
    {
        await _uwpnotifs.SendNotification("Refreshing Caches Phase 1","The application will request all account data from gcs...");
        StorageUsageLabel.Text = "Storage Usage: " + "Currently Refreshing...";
        RefreshStorage.IsEnabled = false;

        //var progressBar = new ProgressBar { IsVisible = true, Progress = 0 };
        var progressLabel = StorageProgressPercentage;
        //StorageProgressPercentage.IsVisible = true;
        StorageProgressPercentage.Text = "Completed: 0%";
        var progressBar = StorageProgressBar;
        //progressBar.IsVisible = true;
        progressBar.Progress = 0;
        ProgressFrame.IsVisible = true;


        try
        {
            var _gutils = new Gutils();
            await Task.Run(async () =>
            {
                var progress = new Progress<int>(percent =>
                {
                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        double Prog = (double)percent / 100;
                        progressBar.Progress = Prog;
                        StorageProgressPercentage.Text = "Completed: "+ string.Format("{0:F2}", (Prog * 100)) + "%";
                    });
                });
                await _gutils.LoadAccounts(progress, AccountsLoadNo, AccountsNo);
                string usageInfo = _gutils.CalculateUsage();
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    //progressLabel.IsVisible = false;
                    //progressBar.IsVisible = false;
                    ProgressFrame.IsVisible = false;
                    RefreshStorage.IsEnabled = true;
                    StorageUsageLabel.Text = "Storage Usage: " + usageInfo.Split(":")[1];
                    _uwpnotifs.SendNotification("Refreshing Caches Complete!", usageInfo);
                    DisplayAlert("Storage Refreshed", usageInfo, "OK");
                });
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            RefreshStorage.IsEnabled = true;
        }
    }

    private async void DeleteSelected_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("File Deletion", "Please wait whilst files are being deleted...", "OK");
        var FileHelper = new DatabaseManager();
        await FileHelper.DeleteSelectedFiles();
        await LoadFiles();
        await DisplayAlert("File Deletion", "Selected Files have been deleted.", "OK");
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        
        await Task.Run(async () =>
        {
            if (!Loaded)
            {
                await LoadFilesContinuously();
                Loaded = true;
            }

        });
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

    public static bool checked_ = false;
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        AnimateFrameBorderColor();
        if (!checked_)
        {
            try
            {
                var Auth = new Auth(Preferences.Get("ServerProtocol", "") + "://"+ Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                await Auth.Verify(this as ContentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception has been thrown: " + ex.Message);
            }

            checked_ = true;
        }

    }
}