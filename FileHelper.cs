using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System.Net.Http.Json;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using System.Text.Json;
using Windows.System;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Xml.Linq;
using static GFrag.Gutils;
using Svg.Skia.TypefaceProviders;

namespace GFrag
{

    public class FileData
    {
        public string UFID { get; set; }
        public string FileName { get; set; }
    }

    public class FileRowData
    {
        public string UFID { get; set; }
        public string FileName { get; set; }
        public bool IsSelected { get; set; }
        public ICommand DownloadBtn { get; set; }
        public ICommand DeleteBtn { get; set; }
        public ICommand SwitchSelectHandler { get; set; }

        public ICommand ShareBtn { get; set; }
        public ICommand UnShareBtn { get; set; }

        public ICommand CheckFileBtn { get; set; }
    }
    public class FileRowDataResponse
    {
        public string UFID { get; set; }
        public string FileName { get; set; }
        public string FileMap { get; set; }

        public string Owner { get; set; }
    }

    public class DirectoryHelper
    {
        [ArmDot.Client.VirtualizeCode]
        public static void DeleteDirectory(string folderPath, int maxAttempts = 3)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            int attempts = 0;
            bool success = false;

            while (attempts < maxAttempts && !success)
            {
                try
                {
                    // Attempt to delete the directory and its contents
                    Directory.Delete(folderPath, true);
                    success = true; // If deletion is successful, set success to true
                }
                catch (IOException ex)
                {
                    // Handle specific IO exceptions
                    Console.WriteLine($"IO Exception: {ex.Message}. Retrying...");
                    attempts++;
                    Thread.Sleep(1000); // Wait for a second before retrying
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Handle unauthorized access exceptions
                    Console.WriteLine($"Unauthorized Access: {ex.Message}. Cannot delete folder.");
                    throw; // Rethrow the exception as it cannot be handled here
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions
                    Console.WriteLine($"An error occurred: {ex.Message}. Retrying...");
                    attempts++;
                    Thread.Sleep(1000); // Wait for a second before retrying
                }
            }

            if (!success)
            {
                throw new Exception($"Failed to delete the directory '{folderPath}' after {maxAttempts} attempts.");
            }
        }
    }

    public class ShareFileRequest
    {
        public string UFID { get; set; }
        public string UserName { get; set; }
    }
    [ArmDot.Client.VirtualizeCode]
    public class ApiService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public class ErrorResponse
        {
            public string error { get; set; }
        }

        public class SuccessResponse
        {
            public string message { get; set; }
        }

        public class ShareFileData
        {
            public string UFID { get; set; }
            public string UserName { get; set; }
        }
        [ArmDot.Client.VirtualizeCode]
        public async Task<string> UnshareFileAsync(string ufid, string userName)
        {
            using (var httpClient = new HttpClient())
            {
                var requestPayload = new
                {
                    UFID = ufid,
                    UserName = userName
                };

                var jsonPayload = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync("https://gtaviso460.pythonanywhere.com/unshare_file", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var successResponse = JsonSerializer.Deserialize<SuccessResponse>(responseBody);
                        return "Success: " + successResponse.message;
                    }
                    else
                    {
                        var errorResponseBody = await response.Content.ReadAsStringAsync();
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorResponseBody);
                        return $"Error: {response.StatusCode} - {errorResponse.error}";
                    }
                }
                catch (HttpRequestException e)
                {
                    return "Request error: " + e.Message;
                }
                catch (Exception e)
                {
                    return "An unexpected error occurred: " + e.Message;
                }
            }
        }
        [ArmDot.Client.VirtualizeCode]
        public async Task<string> ShareFileAsync(string ufid, string userName)
        {
            var requestPayload = new ShareFileRequest
            {
                UFID = ufid,
                UserName = userName
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("https://gtaviso460.pythonanywhere.com/share_file", content);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the success response
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var successResponse = JsonSerializer.Deserialize<SuccessResponse>(responseBody);
                    return "Response: " + successResponse.message;
                }
                else
                {
                    // Deserialize the error response
                    var errorResponseBody = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorResponseBody);
                    return $"Error: {response.StatusCode} - {errorResponse?.error ?? "Unknown error"}";
                }
            }
            catch (HttpRequestException e)
            {
                return "Request error: " + e.Message;
            }
            catch (Exception e)
            {
                return "An unexpected error occurred: " + e.Message;
            }
        }
        [ArmDot.Client.VirtualizeCode]
        public async Task<List<string>> GetUsersAsync()
        {
            try
            {
                // Send the GET request
                var response = await httpClient.GetAsync("https://gtaviso460.pythonanywhere.com/get_users");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response to a List<string>
                var users = JsonSerializer.Deserialize<List<string>>(responseBody);

                return users;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Request error: " + e.Message);
                return new List<string>(); // Return an empty list in case of error
            }
            catch (JsonException e)
            {
                Console.WriteLine("JSON error: " + e.Message);
                return new List<string>(); // Return an empty list in case of JSON error
            }
        }
    }
    public class DatabaseManager
    {
        private CancellationTokenSource? _cancellationTokenSource;
        [ArmDot.Client.VirtualizeCode]
        private async Task SimulateInfiniteProgress(CancellationToken cancellationToken, ProgressBar DownloadProgress)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Increment progress from 0 to 1 repeatedly
                for (double progress = 0; progress <= 1; progress += 0.01)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    DownloadProgress.Progress = progress; // Update the progress bar
                    await Task.Delay(1); // Delay to control the speed of the animation
                }
                // Decrement progress from 1 to 0 repeatedly
                for (double progress = 1; progress >= 0; progress -= 0.01)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    DownloadProgress.Progress = progress; // Update the progress bar
                    await Task.Delay(1); // Delay to control the speed of the animation
                }
            }
        }

        private bool _canDownload = true;
        public static List<string> _selectedufidsfordel = new List<string>();
        [ArmDot.Client.VirtualizeCode]
        public async Task DeleteSelectedFiles()
        {
            var _gutils = new Gutils();
            await Parallel.ForEachAsync(_selectedufidsfordel, async (selectedfile, cancellationToken) =>
            {
                await _gutils.DeleteFragmentedFile(selectedfile);
                Debug.WriteLine("[ DELETE ] >>>>>> " + selectedfile);
            });
            _selectedufidsfordel = new List<string>();
        }

        private ShareModal _popupShareModal;
        [ArmDot.Client.VirtualizeCode]
        public async Task<List<FileRowData>> GetDBFiles(ProgressBar DownloadProgress)
        {
            var _gutils = new Gutils();

            var httpClient = new HttpClient();
            var response_main = await httpClient.GetAsync(Preferences.Get("ServerProtocol", "")+"://" + Preferences.Get("ServerHost", "") + ":"+ Preferences.Get("ServerPort", "") + "/get_all?username="+Preferences.Get("UserName",""));
            var response_shared = await httpClient.GetAsync(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/get_shared_files_by_user?username=" + Preferences.Get("UserName", ""));
            Debug.WriteLine("Username: " + Preferences.Get("UserName", ""));
            if (response_main.IsSuccessStatusCode && response_shared.IsSuccessStatusCode)
            {
                var main_files = await response_main.Content.ReadFromJsonAsync<List<FileRowDataResponse>>();
                var shared_files = await response_shared.Content.ReadFromJsonAsync<List<FileRowDataResponse>>();
                var filesData = main_files.Concat(shared_files).ToList();

                List<FileRowData> result = new List<FileRowData>();

                foreach (var file in filesData)
                {
                    var DownloadBtnCommand = new Command(async () =>
                    {
                        if (!_canDownload) {
                            await Device.InvokeOnMainThreadAsync(async () =>
                            {
                                var DisplayAHandler = new AlertService(Application.Current.MainPage);
                                await DisplayAHandler.DisplayAlertAsync("File Download in progress!", "Please wait until the current download is complete to start another!", "OK");
                            });
                            return;
                        };

                        _canDownload = false; // Disable the button

                        // Download logic here

                        //var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        //await DisplayAHandler.DisplayAlertAsync("Starting File Download", "File: " + file.FileName, "OK");
                        
                        Device.BeginInvokeOnMainThread( async () =>
                        {
                            DownloadProgress.IsVisible = true; // Show the progress bar
                            _cancellationTokenSource = new CancellationTokenSource();

                            // Start the infinite progress simulation
                            await SimulateInfiniteProgress(_cancellationTokenSource.Token, DownloadProgress);

                        });

                        await Task.Run(async () =>
                        {
                            Debug.WriteLine($">>>> Downloading " + file.UFID);
                            await _gutils.DownloadFragmentedFile(file.UFID);



                        });
                        Device.BeginInvokeOnMainThread( async () =>
                        {
                            _cancellationTokenSource?.Cancel(); // Stop the infinite progress
                            DownloadProgress.IsVisible = false; // Hide the progress bar
                            var DisplayAHandler = new AlertService(Application.Current.MainPage);
                            await DisplayAHandler.DisplayAlertAsync("File Downloaded", "Check Downloads Folder for File: " + file.FileName, "OK");
                        });
    
                        _canDownload = true; // Enable the button again
                    });

                    var DeleteBtnCommand = new Command(async () =>
                    {
                        var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        if (file.Owner != null)
                        {
                            await DisplayAHandler.DisplayAlertAsync("File Deletion", "File: " + file.FileName+", Failed, you are not the owner!", "OK");
                            return;
                        }

                        Debug.WriteLine(">>>> Deleting " + file.UFID);
                        await _gutils.DeleteFragmentedFile(file.UFID);
                        var UwpNotif = new UwpNotifs();

                        await UwpNotif.SendNotification("File: " + file.FileName, "File & its fragments have been successfully removed from all assigned accounts...");

                        await DisplayAHandler.DisplayAlertAsync("File Deletion", "File: " + file.FileName+", Success", "OK");
                        
                        


                    });
                    var ShareBtnCommand = new Command(async () =>
                    {
                        var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        var _actionSheetService = new ActionSheetService(Application.Current.MainPage);


                        //// Check if the user is the owner of the file
                        if (file.Owner != null)
                        {
                            await DisplayAHandler.DisplayAlertAsync("File Sharing", $"File: {file.FileName}, Failed, you are not the owner!", "OK");
                            return;
                        }

                        // Create and show the ShareModal popup
                        var _popupShareModal = new ShareModal();
                        var apiService = new ApiService();
                        List<string> users = await apiService.GetUsersAsync();
                        users.Remove(Preferences.Get("UserName",""));
                        _popupShareModal.UserPicker.ItemsSource = users;
                        // Output the list of users
                        foreach (var user in users)
                        {
                            Debug.WriteLine(user);
                        }

                        // Show the popup
                        Application.Current.MainPage.ShowPopup(_popupShareModal); // Use ShowPopup method

                        // Handle the submission of the form
                        _popupShareModal.OnSubmit += async (sender, args) =>
                        {

                            // Close the popup
                            var selectedUser = _popupShareModal.UserPicker.SelectedItem as string;
                            if (!string.IsNullOrEmpty(selectedUser))
                            {
                                // Call ShareFileAsync and capture the result
                                string result = await apiService.ShareFileAsync(file.UFID, selectedUser);

                                // Display the result message
                                await DisplayAHandler.DisplayAlertAsync("Share", result, "OK");

                                // Close the popup if the sharing was successful
                                if (result.StartsWith("Response:"))
                                {
                                    _popupShareModal.Close();
                                }
                            }
                            else
                            {
                                await DisplayAHandler.DisplayAlertAsync("Error", "Please select a user to share with.", "OK");
                            }

                        };
                    });
                    var UnShareBtnCommand = new Command(async () =>
                    {
                        var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        var _actionSheetService = new ActionSheetService(Application.Current.MainPage);

                        //// Check if the user is the owner of the file
                        if (file.Owner != null)
                        {
                            await DisplayAHandler.DisplayAlertAsync("File UnSharing", $"File: {file.FileName}, Failed, you are not the owner!", "OK");
                            return;
                        }

                        // Create and show the UnShareModal popup
                        var _popupUnShareModal = new UnShareModal();
                        var apiService = new ApiService();
                        List<string> users = await apiService.GetUsersAsync();
                        users.Remove(Preferences.Get("UserName", ""));
                        _popupUnShareModal.UserPicker.ItemsSource = users;

                        // Output the list of users for debugging
                        foreach (var user in users)
                        {
                            Debug.WriteLine(user);
                        }

                        // Show the popup
                        Application.Current.MainPage.ShowPopup(_popupUnShareModal);

                        // Handle the submission of the form
                        _popupUnShareModal.OnSubmit += async (sender, args) =>
                        {
                            // Get the selected user
                            var selectedUser = _popupUnShareModal.UserPicker.SelectedItem as string;

                            if (!string.IsNullOrEmpty(selectedUser))
                            {
                                // Call UnshareFileAsync and capture the result
                                string result = await apiService.UnshareFileAsync(file.UFID, selectedUser);

                                // Display the result message
                                if (result.StartsWith("Success:"))
                                {
                                    await DisplayAHandler.DisplayAlertAsync("UnShare", result, "OK");

                                    // Close the popup if the unsharing was successful
                                    _popupUnShareModal.Close();
                                }
                                else
                                {
                                    await DisplayAHandler.DisplayAlertAsync("Error", result, "OK");
                                }
                            }
                            else
                            {
                                await DisplayAHandler.DisplayAlertAsync("Error", "Please select a user to unshare with.", "OK");
                            }
                        };
                    });

                    var ToggledSelectHandler = new Command(async () =>
                    {
                        var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        if (file.Owner != null)
                        {
                            await DisplayAHandler.DisplayAlertAsync("File Deletion", "File: " + file.FileName + ", Failed, you are not the owner!", "OK");
                            return;
                        }
                        else
                        {

                            if (_selectedufidsfordel.Contains(file.UFID))
                            {
                                Debug.WriteLine("[Dnyx-Remove] >>>>>>> " + file.FileName + " [Dnyx-Remove] >>>>> " + file.UFID);
                                _selectedufidsfordel.Remove(file.UFID);
                            }
                            else
                            {
                                Debug.WriteLine("[DyDx] >>>>>>> " + file.FileName + " [Ddyx] >>>>> " + file.UFID);
                                _selectedufidsfordel.Add(file.UFID); // Replace Append with Add
                                Debug.WriteLine("[DyDx] >>>>>>> " + string.Join(", ", _selectedufidsfordel)); // Use string.Join to concatenate the list items
                            }
                        }


                    });
                    var CheckFileBtnCommand = new Command(async () =>
                    {
                        var DisplayAHandler = new AlertService(Application.Current.MainPage);
                        await DisplayAHandler.DisplayAlertAsync("File Check", "File: " + file.FileName + ", UFID: " + file.UFID, "OK");
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            DownloadProgress.IsVisible = true; // Show the progress bar
                            _cancellationTokenSource = new CancellationTokenSource();

                            // Start the infinite progress simulation
                            await SimulateInfiniteProgress(_cancellationTokenSource.Token, DownloadProgress);

                        });
                        var fileMap = file.FileMap;


                        var (CheckedResult, MissingFiles) = await _gutils.CheckFileFragmentFromCloud(fileMap);

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            _cancellationTokenSource?.Cancel(); // Stop the infinite progress
                            DownloadProgress.IsVisible = false; // Hide the progress bar
                        });

                            if (CheckedResult == 0)
                        {
                            await DisplayAHandler.DisplayAlertAsync("File Check", "File: " + file.FileName + ", UFID: " + file.UFID + ", All Fragments are present!", "OK");
                        }
                        else
                        {
                            await DisplayAHandler.DisplayAlertAsync("File Check", "File: " + file.FileName + ", \r\nUFID: " + file.UFID + ",\r\n"+@"Message: Some Fragments are missing!"+"\r\n\r\nFiles:\r\n"+string.Join(", ", MissingFiles), "OK");
                        }


                    });
                    FileRowData fileData = new FileRowData
                    {
                        UFID = file.UFID,
                        FileName = file.FileName,
                        DownloadBtn = DownloadBtnCommand,
                        DeleteBtn = DeleteBtnCommand,
                        SwitchSelectHandler = ToggledSelectHandler,
                        ShareBtn = ShareBtnCommand,
                        UnShareBtn = UnShareBtnCommand,
                        CheckFileBtn = CheckFileBtnCommand
                    };

                    result.Add(fileData);
                }

                return result;
            }
            else
            {
                throw new Exception("Failed to retrieve files from server");
            }
        }

    }
    [ArmDot.Client.VirtualizeCode]
    internal class FileHelper
    {
        
        public static string CalculateSha256Hash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating hash: {ex.Message}");
                return string.Empty;
            }
        }
        public static string GetMimeType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            string extension = Path.GetExtension(filePath);
            return _mappings.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";
        }

        public static string GetFilenameFromPath(string filePath)
        {
            try
            {
                return Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filename: {ex.Message}");
                return string.Empty;
            }
        }
        public static long GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file size: {ex.Message}");
                return 0;
            }
        }
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
        public static string CreateTempFolder()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        private static IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {

        #region Big freaking list of mime types
        // combination of values from Windows 7 Registry and 
        // from C:\Windows\System32\inetsrv\config\applicationHost.config
        // some added, including .7z and .dat
        {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/octet-stream"},
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        #endregion
        
        };

    }
}