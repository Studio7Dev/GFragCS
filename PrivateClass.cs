using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Reflection;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GFrag
{


    public class ProductKeyManager
    {
        private readonly byte[] _key;

        // Constructor that accepts a custom HMAC key or generates one if null
        public ProductKeyManager(byte[] key = null)
        {
            if (key == null || key.Length == 0)
            {
                // Generate a random key for HMAC if no key is provided
                using (var hmac = new HMACSHA256())
                {
                    _key = hmac.Key;
                }
            }
            else
            {
                _key = key;
            }
        }

        // Generate a product key
        public string GenerateProductKey(string productId, DateTime expiryDate)
        {
            // Combine product ID and expiry date
            string data = $"{productId}|{expiryDate:yyyy-MM-dd}";

            // Create HMAC signature
            using (var hmac = new HMACSHA256(_key))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = hmac.ComputeHash(dataBytes);

                // Combine the data and signature into a single string
                string signature = Convert.ToBase64String(signatureBytes);
                return $"{data}|{signature}";
            }
        }

        // Validate a product key
        public bool ValidateProductKey(string productKey, out string productId, out DateTime expiryDate)
        {
            productId = null;
            expiryDate = default;

            try
            {
                // Split the product key into data and signature
                string[] parts = productKey.Split('|');
                if (parts.Length < 3) return false;

                string data = $"{parts[0]}|{parts[1]}";
                string signature = parts[2];

                // Extract product ID and expiry date
                productId = parts[0];
                expiryDate = DateTime.Parse(parts[1]);

                // Verify the signature
                using (var hmac = new HMACSHA256(_key))
                {
                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signatureBytes = Convert.FromBase64String(signature);
                    byte[] computedSignature = hmac.ComputeHash(dataBytes);

                    // Compare the computed signature with the provided signature
                    return AreEqual(computedSignature, signatureBytes);
                }
            }
            catch
            {
                return false; // Validation failed
            }
        }

        // Compare two byte arrays for equality
        private bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
    internal class PrivateClass
    {
        public static bool checked_ = false;
        public class HwidRstMessage
        {
            public string message { get; set; }
        }

        public static async Task<HwidRstMessage> ResetHwid(string username)
        {
            using (var client = new HttpClient())
            {
                // Set the base address for the HTTP client
                client.BaseAddress = new Uri(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", "") + "/");

                // Serialize the username into JSON
                var jsonData = JsonSerializer.Serialize(new { username });
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                try
                {
                    // Send the POST request
                    var response = await client.PostAsync("reset_hwid", content);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Deserialize the response JSON into the HwidRstMessage object
                    var responseMessage = await response.Content.ReadFromJsonAsync<HwidRstMessage>();

                    // Return the message from the response
                    return responseMessage;
                }
                catch (Exception ex)
                {
                    // Log the exception and return an error message
                    Console.WriteLine($"Error: {ex.Message}");
                    return new HwidRstMessage { message = "" };
                }
            }
        }

        public static string InfoGetHwid()
        {
            var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));

            return auth.GetHwid();
        }

        public static string InfoGetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var version = AppInfo.Current.Version.ToString();
            return version;
        }
        private string _NetInfoString;
        public string NetInfoString
        {
            get { return _NetInfoString; }
            set
            {
                NetInfoString = value;
            }
        }
        public static async Task MeasureNetworkSpeedAsync()
        {
            // Get all network interfaces
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            // Select the default NIC (e.g., the first operational NIC)
            var nic = nics.FirstOrDefault(n =>
                n.OperationalStatus == OperationalStatus.Up && // NIC is active
                !n.Description.Contains("Virtual") &&
                !n.Description.Contains("Remote") &&
                !n.Description.Contains("NDIS") &&
                n.GetIPv4Statistics().BytesReceived > 0 &&
                n.GetIPv4Statistics().BytesSent > 0 &&
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback && // Exclude loopback
                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel); // Exclude tunnel adapters

            if (nic == null)
            {
                Debug.WriteLine("No active network interface found.");
                return;
            }

            Debug.WriteLine($"Using NIC: {nic.Name}, " + $"Description: {nic.Description}");

            var reads = Enumerable.Empty<double>();
            var sw = new Stopwatch();
            var lastBr = nic.GetIPv4Statistics().BytesReceived;

            // Initialize variables to store the last bytes sent and the readings for upload
            long lastBs = 0;
            var uploadReads = new List<double>();
            //var reads = new List<double>(); // Ensure you have this for download readings

            for (var i = 0; i < int.MaxValue; i++)
            {
                sw.Restart();
                await Task.Delay(100); // Asynchronous delay
                var elapsed = sw.Elapsed.TotalSeconds;

                // Get current bytes received and sent
                var br = nic.GetIPv4Statistics().BytesReceived;
                var bs = nic.GetIPv4Statistics().BytesSent;

                // Calculate download speed
                var downloadLocal = (br - lastBr) / elapsed;
                lastBr = br;

                // Calculate upload speed
                var uploadLocal = (bs - lastBs) / elapsed;
                lastBs = bs;

                // Keep last 20 readings (~2 seconds) for download
                reads = new[] { downloadLocal }.Concat(reads).Take(20).ToList();
                // Keep last 20 readings (~2 seconds) for upload
                uploadReads = new[] { uploadLocal }.Concat(uploadReads).Take(20).ToList();

                if (i % 10 == 0) // ~1 second
                {
                    // Calculate average download speed
                    var bSecDownload = reads.Sum() / reads.Count();
                    var kbsDownload = (bSecDownload * 8) / 1024; // Convert bytes/sec to kilobits/sec
                    var mbsDownload = kbsDownload / 1024 / 8; // Convert kilobits/sec to megabits/sec
                    mbsDownload = Math.Round(mbsDownload, 2); // Round to 2 decimal places

                    // Calculate average upload speed
                    var bSecUpload = uploadReads.Sum() / uploadReads.Count();
                    var kbsUpload = (bSecUpload * 8) / 1024; // Convert bytes/sec to kilobits/sec
                    var mbsUpload = kbsUpload / 1024 / 8; // Convert kilobits/sec to megabits/sec
                    mbsUpload = Math.Round(mbsUpload, 2); // Round to 2 decimal places

                    string UpDownInfoString = $"| Logged in as: {Preferences.Get("UserName","")} | Upload: {mbsUpload} MB/s, Download: {mbsDownload} MB/s";

                    // Output upload and download speeds on the same line
                    Debug.WriteLine(UpDownInfoString);

                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        Application.Current.MainPage.Title = "GFrag - GCS Fragmented File Store " + UpDownInfoString;
                    });
                    // If you want to move to the next line after this output, you can use:
                    //Debug.WriteLine(""); // This will move to the next line without any additional text
                }
            }
        }
        public async Task CheckVersion()
        {
            if (checked_)
            {
                return;
            }
            string version = PrivateClass.InfoGetVersion();

            var versionChecker = new GFrag.VersionChecker("https://gtaviso460.pythonanywhere.com/", version);
            var result = await versionChecker.CompareVersionsAsync();
            string VersionCheck = result.ToString();
            Debug.WriteLine("[0x0] >>>>>>>>>>>>>>>>>>>>> " + result.ToString());

            var DisplayAHandler = new AlertService(Application.Current.MainPage);
            switch (result)
            {
                case GFrag.VersionChecker.VersionComparisonResult.UpToDate:
                    //await DisplayAlert("Version Check", "Your app is up to date!", "OK");
                    Debug.WriteLine("[0x1] >>>>>>>>>>>>>>>>>>>>> Your app is up to date!");
                    //await DisplayAHandler.DisplayAlertAsync("Version Check", "Your app is up to date!", "OK");
                    break;
                case GFrag.VersionChecker.VersionComparisonResult.OutOfDate:
                    //await DisplayAlert("Version Check", "Your app is out of date, please update to the latest version!", "OK");

                    await DisplayAHandler.DisplayAlertAsync("Version Check", "Your app is out of date, please update to the latest version!", "OK");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/c start ms-appinstaller:?source=https://gtaviso460.pythonanywhere.com/Installer.msix",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    });
                    break;
                case GFrag.VersionChecker.VersionComparisonResult.NewerThanApi:
                    await DisplayAHandler.DisplayAlertAsync("Version Check", "Your app is newer than the API version!", "OK");

                    //await DisplayAlert("Version Check", "Your app is newer than the API version!", "OK");
                    break;
                default:
                    await DisplayAHandler.DisplayAlertAsync("Version Check", "Failed to compare versions!", "OK");
                    //await DisplayAlert("Version Check", "Failed to compare versions!", "OK");
                    break;
            }
        }
    }
}
