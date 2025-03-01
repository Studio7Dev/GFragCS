
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Google.Apis.Drive.v3;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace GFrag
{
    public partial class UploadPage : ContentPage
    {
        private static string _selectedFilePath;
        private Gutils _gutils;
        private UwpNotifs _uwpnotifs;

        public UploadPage()
        {
            InitializeComponent();
            BackgroundManager.WebViewRegistry.Add(backgroundWebView);

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

           
                var redGradient = new Color[]
                {
                // Deep, Cool Reds
                Color.FromHex("#660000"), // Deep Maroon
                Color.FromHex("#6B0A0A"), // Rich Burgundy
                Color.FromHex("#750101"), // Warm Garnet
                Color.FromHex("#7F1313"), // Deep Crimson
                Color.FromHex("#8B0A1A"), // Cool Red

                // Transition to Warm Reds
                Color.FromHex("#9B1D1D"), // Warm Deep Red
                Color.FromHex("#A63333"), // Rich Warm Red
                Color.FromHex("#B34A4A"), // Earthy Red
                Color.FromHex("#C06363"), // Soft Warm Red
                Color.FromHex("#CB7272"), // Muted Fire Red

                // Vibrant Warm Reds
                Color.FromHex("#E01100"), // Vibrant Warm Red
                Color.FromHex("#E56B6B"), // Bright Fire Engine
                Color.FromHex("#EA8A8A"), // Lively Red
                Color.FromHex("#F2C2C2"), // Pastel Warm Red
                Color.FromHex("#F5A9A9"), // Soft Vibrant Red

                // Transition to Reddish-Pinks
                Color.FromHex("#FA8288"), // Warm Reddish Pink
                Color.FromHex("#FF99A9"), // Bright Reddish Pink
                Color.FromHex("#FFB3C5"), // Pastel Reddish Pink
                Color.FromHex("#FFC5CB"), // Soft Pinkish Red
                Color.FromHex("#FFD7E4"), // Light Reddish Pink
                Color.FromHex("#FF375F")  // Final Vibrant Reddish-Pink
                };
                // Ensure Frames is not null
                if (AFrame == null)
                {
                    Debug.WriteLine("AFrame is null");
                    return;
                }
                if (A0Frame == null)
                {
                    Debug.WriteLine("A0Frame is null");
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
                    var index = (int)(v * (redGradient.Length - 1));
                    var color = redGradient[index];
                    AFrame.BorderColor = color;
                    //Debug.WriteLine($"AFrame BorderColor set to {color}");
                }, 0, 1, easing: Easing.Linear);

                A_animation.Commit(this, "AFrameAnimation", length: 3500, repeat: () => true);

     


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void SelectFileButton_Clicked(object sender, EventArgs e)
        {

            var fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                _selectedFilePath = fileResult.FullPath;
                
                SelectedFileLabel.Text = Path.GetFileName(_selectedFilePath);
                
                UploadFileButton.IsEnabled = true;
            }
        }
        private async void OnDrop(object sender, DropEventArgs e)
        {
            Debug.WriteLine("Droped >>>>>");
        }


        private async void UploadFileButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                UploadFileButton.IsEnabled = false;
                UploadProgressBar.IsVisible = true;
                UploadProgressBar.Progress = 0;
                UploadStatusLabel.Text = "Uploading... 0.00%";

                await Task.Run(async () =>
                {
                    string filedir = FileHelper.CreateTempFolder();
                    string OriginalfileName = FileHelper.GetFileName(_selectedFilePath);
                    await _uwpnotifs.SendNotification("Fragmenting File: " + SelectedFileLabel.Text, "Preparing for upload, fragmenting...");
                    FileSplitter.FragmentFile(_selectedFilePath, filedir);
                    string[] files = Directory.GetFiles(filedir);
                    await _uwpnotifs.SendNotification("Uploading File: " + SelectedFileLabel.Text, "Fragmented, " + string.Join(",", files));
                    Gutils.FileMap fileMap = new Gutils.FileMap();
                    int filecount = 0;
                    object lockObject = new object();

                    Task[] tasks = files.Select(async (file) =>
                    {
                        long filesize = FileHelper.GetFileSize(file);
                        string[] Account = _gutils.PickRandomServiceAccounts(filesize.ToString());
                        bool res = await _gutils.InitializeDriveService(Account[0]);
                        if (!res)
                        {
                            Debug.WriteLine("3 Failed to initialize drive service for part " + Account[0]);
                            await ShowAlert("Suspended", "Issues with google services, Check you fucking email you idiot, stop abusing this shit!", "OK");
                            return;
                        }
                        else
                        {
                            string fileHash = FileHelper.CalculateSha256Hash(file);
                            string account = Account[0];
                            string fileId = await _gutils.UploadFile(file);
                            string fileName = FileHelper.GetFileName(file);
                            Gutils.PartMetadata metadata = new Gutils.PartMetadata
                            {
                                FileID = fileId,
                                FileName = fileName,
                                ServiceAccount = account,
                                Hash = fileHash
                            };

                            lock (lockObject)
                            {
                                string pKey = "p" + (filecount + 1).ToString();
                                if (!fileMap.Keys.Contains(pKey))
                                {
                                    fileMap.Add(pKey, metadata);
                                    Debug.Write(pKey + " added! <<<<<<<<<<<<<");
                                }
                                else
                                {
                                    Debug.Write(pKey + " already added! <<<<<<<<<<<<<");
                                }
                            }

                            int localFileCount = Interlocked.Increment(ref filecount);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                UploadProgressBar.Progress = (double)localFileCount / files.Length;
                                UploadStatusLabel.Text = $"Uploading... {(double)localFileCount / files.Length * 100:F2}%";
                            });

                            Debug.WriteLine("Uploaded >>>>>>>>>>>>>>> P" + (localFileCount).ToString());
                        }


                    }).ToArray();

                    await Task.WhenAll(tasks);
                    try
                    {
                        //DirectoryHelper.DeleteDirectory(filedir);
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
                    await _gutils.StoreFragmentedFileInDB(fragmentfFile);
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                    UploadFileButton.IsEnabled = true;
                    UploadProgressBar.IsVisible = false;
                    UploadStatusLabel.Text = "File uploaded successfully!";
                });

                await _uwpnotifs.SendNotification("File: " + SelectedFileLabel.Text, "Successfully Uploaded!");
                await ShowAlert("File Uploaded", "File uploaded successfully", "OK");
            }
            catch (Exception ex)
            {
                await ShowAlert(Title, "Failed to upload file. " + ex.ToString(), "OK");
                Device.BeginInvokeOnMainThread(() =>
                {
                    UploadProgressBar.IsVisible = false;
                    UploadFileButton.IsEnabled = true;
                });
            }
        }

        private async Task ShowAlert(string title, string message, string buttonText)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(title, message, buttonText);
            });
        }

        //private async Task Test_Clicked(object sender, EventArgs e)
        //{
        //    DatabaseManager databaseManager = new DatabaseManager();
        //    List<FileRowData> filesData = await databaseManager.GetDBFiles();

        //    foreach (var fileData in filesData)
        //    {
        //        Debug.WriteLine($"File Name: {fileData.FileName}, UFID: {fileData.UFID}");
        //    }
        //}

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            //var token = Preferences.Get("Token", string.Empty);
            //if (token == null)
            //{
            //    var pageHandler = new PageHandler(this as ContentPage);
            //    pageHandler.ToggleVisibility();
            //}

        }

        public static bool checked_ = false;
        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            AnimateFrameBorderColor();
            if (!checked_)
            {
                try
                {
                    var Auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
                    await Auth.Verify(this as ContentPage);
                    //await CheckVersion();
                    checked_ = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception has been thrown: " + ex.Message);
                }
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

        private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
        {

        }

        private void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
        {

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