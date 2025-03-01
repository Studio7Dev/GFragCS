using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Windows.Storage;
using static GFrag.Gutils;
namespace GFrag;

public partial class MultiUpload : ContentPage
{
    private List<string> _selectedFiles;
    private List<string> _selectViewFileNames;
    private List<FragmentedFile> _pendingDbCalls;
    private Gutils _gutils;
    private UwpNotifs _uwpnotifs;

    public MultiUpload()
    {
        InitializeComponent();
        BackgroundManager.WebViewRegistry.Add(backgroundWebView);
        _selectedFiles = new List<string>();
        _pendingDbCalls = new List<FragmentedFile>();
        _gutils = new Gutils(); // Create an instance of Gutils
        _uwpnotifs = new UwpNotifs();
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
            // Ensure Frames is not null
            if (AFrame == null)
            {
                Debug.WriteLine("AFrame is null");
                return;
            }
            if (BFrame == null)
            {
                Debug.WriteLine("BFrame is null");
                return;
            }
            if(A0Frame == null)
            {
                Debug.WriteLine("A0Frame is null");
                return;
            }
            if (CFrame == null)
            {
                Debug.WriteLine("CFrame is null");
                return;
            }
            var A0_animation = new Animation(v =>
            {
                var index = (int)(v * (colors.Length - 1));
                var color = colors[index];
                A0Frame.BorderColor = color;
                //Debug.WriteLine($"A0Frame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            A0_animation.Commit(this, "A0FrameAnimation", length: 3500, repeat: () => true);

            var A_animation = new Animation(v =>
            {
                var index = (int)(v * (colors.Length - 1));
                var color = colors[index];
                AFrame.BorderColor = color;
                //Debug.WriteLine($"AFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            A_animation.Commit(this, "AFrameAnimation", length: 3500, repeat: () => true);

            var B_animation = new Animation(v =>
            {
                var index = (int)(v * (colors.Length - 1));
                var color = colors[index];
                BFrame.BorderColor = color;
                //Debug.WriteLine($"BFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            B_animation.Commit(this, "BFrameAnimation", length: 3500, repeat: () => true);


            var C_animation = new Animation(v =>
            {
                var index = (int)(v * (colors2.Length - 1));
                var color = colors2[index];
                CFrame.BorderColor = color;
                //Debug.WriteLine($"BFrame BorderColor set to {color}");
            }, 0, 1, easing: Easing.Linear);

            C_animation.Commit(this, "CFrameAnimation", length: 3200, repeat: () => true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private async void SelectFilesButton_Clicked(object sender, EventArgs e)
    {
        _selectedFiles = new List<string>();
        _selectViewFileNames = new List<string>();
        var fileResults = await FilePicker.Default.PickMultipleAsync();
        if (fileResults != null)
        {
            if (fileResults.Count() > 0)
            {
                foreach (var fileResult in fileResults)
                {
                    _selectedFiles.Add(fileResult.FullPath);
                    _selectViewFileNames.Add(fileResult.FileName);
                }

                SelectedFilesListView.ItemsSource = _selectViewFileNames;
                UploadFilesButton.IsEnabled = true;
            }

        }
    }

    private async void UploadFilesButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            UploadFilesButton.IsEnabled = false;
            UploadProgressBar.IsVisible = true;
            UploadProgressBar.Progress = 0;
            UploadStatusLabel.Text = "Uploading... 0.00%";

            int fileCount = 0;
            int totalFiles = _selectedFiles.Count;

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
            var pendingDbCalls = new ConcurrentBag<Gutils.FragmentedFile>();

            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(_selectedFiles, parallelOptions, async (file, cancellationToken) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await _uwpnotifs.SendNotification("Fragmenting File: " + Path.GetFileName(file), "Preparing for upload, fragmenting...");
                    string filedir = FileHelper.CreateTempFolder();
                    string OriginalfileName = FileHelper.GetFileName(file);
                    FileSplitter.FragmentFile(file, filedir);
                    string[] files = Directory.GetFiles(filedir);
                    await _uwpnotifs.SendNotification("Uploading File: " + Path.GetFileName(file), "Fragmented, " + string.Join(",", files));
                    Gutils.FileMap fileMap = new Gutils.FileMap();
                    int fragmentCount = 0;

                    foreach (var fragment in files)
                    {
                        long fragmentSize = FileHelper.GetFileSize(fragment);
                        string[] Account = _gutils.PickRandomServiceAccounts(fragmentSize.ToString());
                        bool res = await _gutils.InitializeDriveService(Account[0]);
                        if (!res)
                        {
                            Debug.WriteLine("4 Failed to initialize drive service for part " + Account[0]);
                            await DisplayAlert("Suspended", "Issues with google services, Check you fucking email you idiot, stop abusing this shit!", "OK");
                            return;
                        }
                        else
                        {
                            string fragmentHash = FileHelper.CalculateSha256Hash(fragment);
                            string account = Account[0];
                            string fileId = await _gutils.UploadFile(fragment);
                            string fileName = FileHelper.GetFileName(fragment);
                            Gutils.PartMetadata metadata = new Gutils.PartMetadata
                            {
                                FileID = fileId,
                                FileName = fileName,
                                ServiceAccount = account,
                                Hash = fragmentHash
                            };

                            string pKey = "p" + (fragmentCount + 1).ToString();
                            if (!fileMap.Keys.Contains(pKey))
                            {
                                fileMap.Add(pKey, metadata);
                                Debug.Write(pKey + " added! <<<<<<<<<<<<<");
                            }
                            else
                            {
                                Debug.Write(pKey + " already added! <<<<<<<<<<<<<");
                            }

                            fragmentCount++;

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                UploadProgressBar.Progress = ((double)fileCount / totalFiles) + ((double)fragmentCount / files.Length / totalFiles);
                                UploadStatusLabel.Text = $"Uploading... {((double)fileCount / totalFiles) * 100 + ((double)fragmentCount / files.Length / totalFiles) * 100:F2}%";
                            });


                        }


                    }
                    try
                    {
                        DirectoryHelper.DeleteDirectory(filedir);
                        Debug.WriteLine($"Successfully deleted the directory: {filedir}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                    }

                    string json = JsonConvert.SerializeObject(fileMap);
                    Debug.WriteLine(json);
                    string uuid = Guid.NewGuid().ToString("D");
                    Gutils.FragmentedFile fragmentfFile = new Gutils.FragmentedFile
                    {
                        FileMap = fileMap,
                        UFID = uuid,
                        FileName = OriginalfileName,
                        UserName = Preferences.Get("UserName", "")
                    };
                    pendingDbCalls.Add(fragmentfFile);

                    Interlocked.Increment(ref fileCount);
                }
                finally
                {

                    semaphore.Release();
                }

            });

            foreach (var DbCall in pendingDbCalls)
            {
                await _gutils.StoreFragmentedFileInDB(DbCall);
                await Task.Delay(150);
            }
            
            _pendingDbCalls = new List<FragmentedFile>();
            UploadFilesButton.IsEnabled = true;
            UploadProgressBar.IsVisible = false;
            UploadStatusLabel.Text = "Files uploaded successfully!";
            await _uwpnotifs.SendNotification("Files", "Successfully Uploaded!");
            await DisplayAlert("Files Uploaded", "Files uploaded successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(Title, "Failed to upload files. " + ex.ToString(), "OK");
            UploadProgressBar.IsVisible = false;
            UploadFilesButton.IsEnabled = true;
        }
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

    private void Select_PointerExited(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#4286F5");
    }

    private void Select_PointerEntered(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#5A9AF5");
    }

    private void Upload_PointerExited(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#FABC05");
    }

    private void Upload_PointerEntered(object sender, PointerEventArgs e)
    {
        (sender as Microsoft.Maui.Controls.Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#E6A300");
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
