using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using Google;
using Newtonsoft.Json;
using System.Data.Entity.Core.Common;
using Google.Apis.Drive.v3.Data;
using static Google.Apis.Requests.BatchRequest;
using System.Globalization;
using System.Net.Http.Json;
using ExCSS;
using Windows.Storage;

namespace GFrag
{
    [ArmDot.Client.VirtualizeCode]
    internal class Gutils
    {

        private DriveService _driveService;


        public async Task<string> UploadFile(string path)
        {
            

            var fileMetadata = new Google.Apis.Drive.v3.Data.File();

            string uploadedFileId;
            // Create a new file on Google Drive
            await using (var fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Create a new file, with metadata and stream.

                var request = _driveService.Files.Create(fileMetadata, fsSource, "text/plain");
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);
                // share a file with a user


                if (results.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    //var permission = new Permission
                    //{
                    //    Type = "user",
                    //    Role = "reader",
                    //    EmailAddress = "gtavisfreebro@gmail.com"
                    //};
                    //var createPermissionRequest = _driveService.Permissions.Create(permission, request.ResponseBody?.Id);
                    //createPermissionRequest.Fields = "*";
                    //var permissionResults = await createPermissionRequest.ExecuteAsync();
                    Console.WriteLine("File uploaded successfully");
                }
                else if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                }
                //if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                //{
                //    Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                //}

                // the file id of the new file we created
                uploadedFileId = request.ResponseBody?.Id;
            }
            return uploadedFileId;

        }

        public class AlertMessage
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string ButtonText { get; set; }
        }

        public class MyViewModel
        {
            public void DisplayAlert()
            {
                var alertMessage = new AlertMessage
                {
                    Title = "Info",
                    Message = "Download Completed!",
                    ButtonText = "OK"
                };

                MessagingCenter.Send(this, "DisplayAlert", alertMessage);
            }
        }
        public class FragmentedFile
        {
            public string UFID { get; set; }
            public string FileName { get; set; }
            public FileMap FileMap { get; set; }
            public string UserName { get; set; }
        }
        public class PartMetadata
        {
            [JsonProperty("file_id")]
            public string? FileID { get; set; }

            [JsonProperty("file_name")]
            public string? FileName { get; set; }

            [JsonProperty("service_account")]
            public string? ServiceAccount { get; set; }

            [JsonProperty("sha256")]
            public string? Hash { get; set; }
        }


        public class FileMap : Dictionary<string, PartMetadata> { }

        public static SemaphoreSlim StoreFragmentedFileInDBSemaphore = new SemaphoreSlim(1, 1);

        public async Task StoreFragmentedFileInDB(Gutils.FragmentedFile fragmentFile)
        {
            int retryCount = 0;
            const int maxRetries = 5;
            const int delay = 1000; // 1 second

            await StoreFragmentedFileInDBSemaphore.WaitAsync();

            try
            {
                while (retryCount < maxRetries)
                {
                    try
                    {
                        var httpClient = new HttpClient();
                        var response = await httpClient.PostAsync(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/add",
                            new StringContent(JsonConvert.SerializeObject(new
                            {
                                UFID = fragmentFile.UFID,
                                FileName = fragmentFile.FileName,
                                FileMap = fragmentFile.FileMap,
                                UserName = fragmentFile.UserName
                            }), Encoding.UTF8, "application/json"));

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                        else
                        {
                            //throw new HttpRequestException($"Failed to add file to database. Status code: {response.StatusCode}, Body: {response.Content}");
                            // Show the http reponse content
                            Debug.WriteLine($"Error storing fragmented file in database: {response.StatusCode}");
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message.Contains("database is locked") || ex.Message.Contains("Timeout"))
                        {
                            retryCount++;
                            await Task.Delay(delay);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (retryCount >= maxRetries)
                {
                    throw new Exception("Failed to store fragmented file in database after " + maxRetries + " retries");
                }
            }
            finally
            {
                StoreFragmentedFileInDBSemaphore.Release();
            }
        }


        //public void ExecuteSQLiteQuery(string query)
        //{
        //    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //    Debug.WriteLine("Application base directory: " + currentDirectory);

        //    string dbDir = Path.Combine(currentDirectory, FileDB);
        //    Debug.WriteLine("Looking for files.db in: " + dbDir);

        //    Debug.WriteLine("Current working directory: " + Directory.GetCurrentDirectory());

        //    string connectionString = "Data Source=" + dbDir + ";Version=3";

        //    try
        //    {
        //        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        //        {
        //            connection.Open();

        //            if (connection.State == ConnectionState.Open)
        //            {
        //                using (SQLiteCommand command = new SQLiteCommand(query, connection))
        //                {
        //                    try
        //                    {
        //                        command.ExecuteNonQuery();
        //                    }
        //                    catch (SQLiteException ex)
        //                    {
        //                        Debug.WriteLine("Error executing query: " + ex.Message);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Debug.WriteLine("Connection is not open");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error connecting to database: " + ex.Message);
        //    }
        //}
        
        public async Task DeleteFileFromDB(string ufid)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(Preferences.Get("ServerProtocol", "") + "://"+ Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/delete",
                    new StringContent(JsonConvert.SerializeObject(new { UFID = ufid , UserName = Preferences.Get("UserName", "")}), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(">>>>> File no longer in database! <<<<<");
                }
                else
                {
                    Debug.WriteLine($"Error deleting file from DB: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error deleting file from DB: {ex.Message}");
                throw;
            }
        }

        public List<Dictionary<string, object>> ExecuteSQLiteQuery(string query)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Debug.WriteLine("Application base directory: " + currentDirectory);
                string FileDB = Preferences.Get("DatabasePath", "");
                string dbDir = FileDB;
                Debug.WriteLine("Looking for files.db in: " + FileDB);

                Debug.WriteLine("Current working directory: " + Directory.GetCurrentDirectory());

                string connectionString = "Data Source=" + FileDB + ";Version=3";

                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetName(i), reader.GetValue(i));
                                }
                                results.Add(row);
                            }
                        }
                    }
                }
                return results;
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Error executing query: " + ex.Message);
                return new List<Dictionary<string, object>>();
            }
        }

        public List<Dictionary<string, object>> ExecuteSQLiteQueryWithParams(string query, Dictionary<string, string> parameters)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Debug.WriteLine("Application base directory: " + currentDirectory);
                string FileDB = Preferences.Get("DatabasePath", "");
                string dbDir = FileDB;
                Debug.WriteLine("Looking for files.db in: " + FileDB);

                Debug.WriteLine("Current working directory: " + Directory.GetCurrentDirectory());

                string connectionString = "Data Source=" + FileDB + ";Version=3";

                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetName(i), reader.GetValue(i));
                                }
                                results.Add(row);
                            }
                        }
                    }
                }
                return results;
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Error executing query: " + ex.Message);
                return new List<Dictionary<string, object>>();
            }
        }

        public async Task< (int, List<string>) >CheckFileFragmentFromCloud(string FileMap)
        {
            var fileMapDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PartMetadata>>(FileMap);
            int Hashs = fileMapDict.Count;
            List<string> MissingFiles = new List<string>();

            Debug.WriteLine("Hashs Left >>>>>>>>> " + Hashs.ToString());

            // Download file fragments in parallel
            await Parallel.ForEachAsync(fileMapDict, new ParallelOptions { MaxDegreeOfParallelism = 1 }, async (part, cancellationToken) =>
            {

                Debug.WriteLine($"Part {part.Key}:");
                Debug.WriteLine($"  File ID: {part.Value.FileID}");
                Debug.WriteLine($"  File Name: {part.Value.FileName}");
                Debug.WriteLine($"  Service Account: {part.Value.ServiceAccount}");
                Debug.WriteLine($"  Hash: {part.Value.Hash}");
                bool res = await InitializeDriveService(part.Value.ServiceAccount);
                if (res)
                {
                    var FileMetadataG = new Google.Apis.Drive.v3.Data.File()
                    {
                        Id = part.Value.FileID,
                        Name = part.Value.FileName
                    };
                    try
                    {
                        var request = _driveService.Files.Get(part.Value.FileID);
                        // check if file exists in cloud
                        request.Fields = "id, name, size, mimeType, parents, md5Checksum, trashed";
                        var fileMetadata = request.Execute();
                        //Debug.WriteLine("File ID: " + fileMetadata.Id);
                        //Debug.WriteLine("File Name: " + fileMetadata.Name);
                        Hashs = Hashs - 1;
                        Debug.WriteLine("Account Valid -----------------------> " + part.Value.ServiceAccount);
                        Debug.WriteLine("File Valid -----------------------> " + part.Value.FileName);
                    }
                    catch (Google.GoogleApiException ex)
                    {
                        Debug.WriteLine("Error Checking, Not Valid -----------------------> " + part.Value.FileName);
                        Debug.WriteLine("Error checking file fragment from cloud: " + ex.Message);
                        MissingFiles.Add(part.Value.FileName);
                    }
                }
                else
                {
                    Debug.WriteLine("Not Valid -----------------------> " + part.Value.ServiceAccount);
                }


                //Gutils.CheckFileFragmentFromCloud(se)


            });

            return (Hashs, MissingFiles);
        }

        private static void DownloadFile(Google.Apis.Drive.v3.DriveService service, Google.Apis.Drive.v3.Data.File file, string saveTo)
        {

            var request = service.Files.Get(file.Id);
            var stream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        {
                            Debug.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Completed:
                        {
                            Debug.WriteLine(">|>|>|>|>|>|>| Download complete.");
                            SaveStream(stream, saveTo);
                            DownloadCount = DownloadCount + 1;
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                            Debug.WriteLine(">|>|>|>|>|>|>| Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream);

        }

        private static void SaveStream(System.IO.MemoryStream stream, string saveTo)
        {
            using (System.IO.FileStream file = new System.IO.FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }

        public async Task DeleteFragmentedFile(string ufid)
        {
            var (fileName, fileMap) = await GetFileIDFromDB(ufid);
            Debug.WriteLine(">>>> FileName: " + fileName + " >>>> FileMAP: " + fileMap);
            var fileMapDict = JsonConvert.DeserializeObject<Dictionary<string, PartMetadata>>(fileMap);
            await Parallel.ForEachAsync(fileMapDict, new ParallelOptions { MaxDegreeOfParallelism = 20 }, async (part, cancellationToken) =>
            {
                Debug.WriteLine($"Part {part.Key}:");
                Debug.WriteLine($"  File ID: {part.Value.FileID}");
                Debug.WriteLine($"  File Name: {part.Value.FileName}");
                Debug.WriteLine($"  Service Account: {part.Value.ServiceAccount}");
                Debug.WriteLine($"  Hash: {part.Value.Hash}");
                bool res = await InitializeDriveService(part.Value.ServiceAccount);
                if (!res)
                {
                    await DeleteFileFromDB(ufid);
                    return;
                }
                var request = _driveService.Files.Delete(part.Value.FileID);
                try
                {
                    var response = await request.ExecuteAsync();
                    Debug.WriteLine(">>>>>>>>>DELETE>>>>>>>>>" + response);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(">>>>>>>>>DELETE>>>>>>>>>" + ex);
                }
            });
            await DeleteFileFromDB(ufid);


        }

        public static int DownloadCount = 0;

        public async Task DownloadFragmentedFile(string ufid)
        {
            string DownloadsFolder = Preferences.Get("DownloadsFolder", "");
            Debug.WriteLine(">>>>>>> ["+ufid+"] <<<<<<<");
            var _uwpnotifs = new UwpNotifs();
            
            string tmpfiledir = FileHelper.CreateTempFolder();
            var (fileName, fileMap) = await GetFileIDFromDB(ufid);
            Debug.WriteLine(">>>> FileName: " + fileName + " >>>> FileMAP: " + fileMap);
            await _uwpnotifs.SendNotification("Downloading File: " +fileName, "Begining to fetch all fragments...");
            var fileMapDict = JsonConvert.DeserializeObject<Dictionary<string, PartMetadata>>(fileMap);
            int Hashs = fileMapDict.Count;
            Debug.WriteLine("Hashs Left >>>>>>>>> " + Hashs.ToString());

            // Download file fragments in parallel
            await Parallel.ForEachAsync(fileMapDict, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (part, cancellationToken) =>
            {
                Debug.WriteLine($"Part {part.Key}:");
                Debug.WriteLine($"  File ID: {part.Value.FileID}");
                Debug.WriteLine($"  File Name: {part.Value.FileName}");
                Debug.WriteLine($"  Service Account: {part.Value.ServiceAccount}");
                Debug.WriteLine($"  Hash: {part.Value.Hash}");
                var FileMetadataG = new Google.Apis.Drive.v3.Data.File()
                {
                    Id = part.Value.FileID,
                    Name = part.Value.FileName
                };
                string fileFragmentPath = Path.Combine(tmpfiledir, part.Value.FileName);
                bool res = await InitializeDriveService(part.Value.ServiceAccount);
                if (res)
                {
                    DownloadFile(_driveService, FileMetadataG, fileFragmentPath);
                    Interlocked.Increment(ref DownloadCount);
                }
                else
                {
                    Debug.WriteLine("2 Failed to initialize drive service for part " + part.Key);
                }
            });
            await _uwpnotifs.SendNotification("Checking File: " + fileName, "Comparing Hashes to make sure there is no corruption...");
            Debug.WriteLine("[ 0x00x0 ] Hashs Left >>>>>>>>> " + Hashs.ToString());
            Debug.WriteLine("[ 0x00x0 ] DLC  >>>>>>>>> " + (DownloadCount / 2).ToString());
            if (Hashs == DownloadCount/2)
            {
                string tempFolderDL = tmpfiledir;
                string[] files = Directory.GetFiles(tempFolderDL);
                int checkedfileindex = -1;
                foreach (var file in files)
                {
                    string sha256 = FileHelper.CalculateSha256Hash(file);
                    var mdata = fileMapDict.Values.ToList();
                    bool found = false;
                    foreach (var part in mdata)
                    {
                        Debug.WriteLine("Hash Calc: " + sha256 + " StoredHash: " + part.Hash);
                        if (part.Hash == sha256)
                        {
                            checkedfileindex = checkedfileindex + 1;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Debug.WriteLine("Mismatch Hash");
                    }
                }
                int finalcheckedfileindex = (checkedfileindex + 1);

                Debug.WriteLine("[ Final ] Hashs Left >>>>>>>>> " + Hashs.ToString() + "Hashs Checked >>>>>> " + finalcheckedfileindex.ToString());
                if (Hashs == finalcheckedfileindex)
                {
                    await _uwpnotifs.SendNotification("Assembling File: " + fileName, "Begining to piece fragments together!");
                    FileSplitter.ReassembleFile(Path.Combine(DownloadsFolder, fileName), tempFolderDL, fileName);
                    Debug.WriteLine("Assembled File....");
                    await _uwpnotifs.SendNotification("Assembled File: " + fileName, "File has been assembled! Check Downloads...");

                }
                else
                {
                    Debug.WriteLine("Error While Assembling File...");
                }
            }
            try
            {
                DirectoryHelper.DeleteDirectory(tmpfiledir);
                Debug.WriteLine($"Successfully deleted the directory: {tmpfiledir}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
            DownloadCount = 0;

        }

        public async Task<(string, string)> GetSharedFileByUfid(string ufid)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/get_shared_file_by_ufid",
                    new StringContent(JsonConvert.SerializeObject(new { UFID = ufid }), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<FileRowDataResponse>();
                    Debug.WriteLine(">>>> Response: " + result);
                    return (result.FileName, result.FileMap);
                }
                else
                {
                    Debug.WriteLine($"Error getting shared file by UFID: {response.StatusCode}");
                    return (null, null);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting shared file by UFID: {ex.Message}");
                throw;
            }
        }

        public async Task<(string, string)> GetFileIDFromDB(string uuid)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(Preferences.Get("ServerProtocol", "") + "://"+ Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/get_file_by_ufid",
                    new StringContent(JsonConvert.SerializeObject(new { UFID = uuid, UserName = Preferences.Get("UserName", "") }), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<FileRowDataResponse>();
                    Debug.WriteLine(">>>> FileName: " + result.FileName + " >>>> FileMAP: " + result.FileMap);
                    return (result.FileName, result.FileMap);
                }
                else
                {
                    if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var (FileName, FileMap) = await GetSharedFileByUfid(uuid);
                        if (FileMap != null)
                        {
                            return (FileName, FileMap);
                        }
                        else
                        {
                            Debug.WriteLine("Failed to retrieve shared file.");
                            return (null, null);
                        }
                        return (null, null);
                    }
                    Debug.WriteLine($"Error getting file ID from DB x: {response.StatusCode}");
                    return (null, null);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting file ID from DB c: {ex.Message}");
                throw;
            }
        }

        public class FileRowDataResponse
        {
            public string UFID { get; set; }
            public string FileName { get; set; }
            public string FileMap { get; set; }
        }

        public async Task LoadAccounts(IProgress<int> progress, Microsoft.Maui.Controls.Label AccProg, Microsoft.Maui.Controls.Label AccTotal)
        {
            var UwpNotif = new UwpNotifs();

            this.ExecuteSQLiteQuery("DELETE FROM Accounts;");
            string[] serviceAccounts = Directory.GetFiles(Preferences.Get("AccountsDirectory", ""));
            List<string> AccountsFromDirQueries = new List<string>();

            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // Use the number of processor cores
            };

            int fileCount = 0;
            Parallel.ForEach(serviceAccounts, parallelOptions, async (account) =>
            {
                try
                {
                    ProcessAccount(account, AccountsFromDirQueries);
                    
                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        AccProg.Text = "Accounts: " + fileCount.ToString() + " / "+serviceAccounts.Length.ToString();
                    });
                }
                catch (GoogleApiException ex)
                {
                    Console.WriteLine($"Google API Exception: {ex.Message}");
                    Console.WriteLine($"Error code: {ex.HttpStatusCode}");
                    Console.WriteLine($"Error message: {ex.Message}");
                    var DisplayAHandler = new AlertService(Application.Current.MainPage);
                    await DisplayAHandler.DisplayAlertAsync("Account Checker", "ErrorCode: "+ex.HttpStatusCode.ToString()+" Message: "+ex.Message.ToString(), "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    Interlocked.Increment(ref fileCount);
                    progress.Report((int)((double)fileCount / serviceAccounts.Length * 100));
                }

            });
            Device.InvokeOnMainThreadAsync(() =>
            {
                AccTotal.Text = "Accounts: " + fileCount.ToString()+" / "+serviceAccounts.Length.ToString();
            });
            // Reset the progress bar
            progress.Report(0);
            await UwpNotif.SendNotification("Refreshing Caches Phase 2", "The application is now importing data into our local database..");
            // Execute the queries and report the progress
            int queryCount = 0;
            int queryTotal = AccountsFromDirQueries.Count;
            foreach (string query in AccountsFromDirQueries)
            {
                this.ExecuteSQLiteQuery(query);
                Debug.WriteLine("> " + query);
                Interlocked.Increment(ref queryCount);
                progress.Report((int)((double)queryCount / queryTotal * 100));
            }

        }

        public string CalculateUsage()
        {
            // Get the total number of accounts
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string FileDB = Preferences.Get("DatabasePath", "");
            string dbDir = FileDB;
            Debug.WriteLine(">>>>>>>>>>>>>>>>>>> " + FileDB + " <<<<<<<<<<<<<<<<<<<<");
            string connectionString = "Data Source=" + FileDB + ";Version=3";

            long totalUsage = 0;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = $@"SELECT ""Usage"", ""Limit"", ""name"" FROM Accounts ORDER BY RANDOM()";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string usage = reader["Usage"].ToString();
                            int conversion = int.Parse(usage);
                            totalUsage += conversion;
                        }
                    }
                }
            }

            // Get the total number of accounts
            int accounts = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Accounts";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    accounts = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            int countAccounts = accounts - 1;
            int totalStorage = countAccounts * 15;

            // Convert the total usage to GB
            double usageInGB = (double)totalUsage / (1024 * 1024 * 1024);

            // Create the info string
            string infoString = string.Format("Total Usage: [ {0:F2} ] / [ {1} ] GB", usageInGB, totalStorage);

            Debug.WriteLine(infoString);
            Debug.WriteLine("");

            return infoString;
        }
        private async void ProcessAccount(string account, List<string> AccountsFromDirQueries)
        {
            try
            {
                Debug.WriteLine(">>>>>>>>>>>> " + account);
                bool res = await InitializeDriveService(FileHelper.GetFilenameFromPath(account)); // Ensure this method authenticates the service
                if (res)
                {
                    string name = Path.GetFileName(account).Split(".")[0] + ".json";

                    // Retrieve the storage quota
                    var aboutRequest = _driveService.About.Get();
                    aboutRequest.Fields = "storageQuota(limit,usageInDrive)";
                    var aboutResponse = aboutRequest.Execute();

                    if (aboutResponse == null)
                    {
                        Debug.WriteLine("Failed to retrieve storage quota for account: " + account);
                        return;
                    }

                    long? limit = aboutResponse.StorageQuota.Limit;
                    long? usage = aboutResponse.StorageQuota.UsageInDrive;

                    if (!limit.HasValue || !usage.HasValue)
                    {
                        Debug.WriteLine("Invalid storage quota values for account: " + account);
                        return;
                    }

                    double? bytes = limit - usage;
                    double? totalSpaceLeft = bytes / Math.Pow(1024, 3);

                    Debug.WriteLine($"Total Space Left: {totalSpaceLeft:F4} GB");

                    string query = $@"
        INSERT INTO Accounts (""name"", ""Usage"", ""Limit"")
        SELECT '{name}', '{usage}', '{limit}'
        WHERE NOT EXISTS (
            SELECT 1
            FROM Accounts
            WHERE ""name"" = '{name}'
        );";

                    lock (AccountsFromDirQueries)
                    {
                        AccountsFromDirQueries.Add(query);
                    }
                }
                else
                {
                    Debug.WriteLine("1 Failed to initialize drive service for part " + account);
                }
                
            }
            catch (Google.GoogleApiException ex)
            {
                Debug.WriteLine($"Google API exception for account: {account}. Error: {ex.Message}");
                if (ex.Message.Contains("Forbidden"))
                {
                    System.IO.File.Delete(account);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unknown exception for account: {account}. Error: {ex.Message}");
            }
        }

        public string[] PickRandomServiceAccounts(string fileSize)
        {
            var random = new Random();
            string FileDB = Preferences.Get("DatabasePath", "");
            Debug.WriteLine(">>>>>>>>>>>> " + FileDB + " <<<<<<<<<<<<<<");
            using (var connection = new SQLiteConnection("Data Source=" + FileDB + ";Version=3;Journal Mode=Off;"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @$"SELECT ""Usage"", ""Limit"", ""name"" FROM Accounts ORDER BY RANDOM()";

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var usage = reader["Usage"].ToString();
                            var limit = reader["Limit"].ToString();
                            var name = reader["name"].ToString();

                            Console.WriteLine($"Usage={usage}, Limit={limit}, Name={name}");

                            var (allowed, serviceName) = CheckIfCanStore(limit, usage, fileSize, name);
                            if (allowed)
                            {
                                return new string[] { serviceName };
                            }
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return new string[0];
        }

        public (bool, string) CheckIfCanStore(string limit, string usage, string fileSize, string name)
        {
            // Convert string to long
            if (!long.TryParse(limit, out long limitLong) ||
              !long.TryParse(usage, out long usageLong) ||
              !long.TryParse(fileSize, out long fileSizeLong))
            {
                Console.WriteLine("Error converting one of the values to long.");
                return (false, string.Empty);
            }

            // Now you can safely perform your comparison
            if (limitLong - usageLong >= fileSizeLong)
            {
                Console.WriteLine("Enough space available to store the file.");
                return (true, name);
            }
            else
            {
                Console.WriteLine($"Insufficient space. Required: {fileSizeLong}, Available: {limitLong - usageLong}");
            }
            return (false, string.Empty);
        }
        
        public async Task<bool> InitializeDriveService(string credentialFilePath)
        {

            string fullpath = Path.Combine(Preferences.Get("AccountsDirectory", ""), credentialFilePath);
            var scopes = new[]
            {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveAppdata,
                DriveService.Scope.DriveScripts,
            };

            try
            {
                var credential = GoogleCredential.FromFile(fullpath).CreateScoped(scopes);
                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential
                });
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                Debug.WriteLine($"Google API exception for account: {fullpath}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unknown exception for account: {fullpath}. Error: {ex.Message}");
                return false;
            }


        }
    }
}
