using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui.Controls;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace GFrag
{
    public class VersionChecker
    {
        private readonly string _baseUrl;
        private readonly string _appVersion;

        public VersionChecker(string baseUrl, string appVersion)
        {
            _baseUrl = baseUrl;
            _appVersion = appVersion;
        }

        public async Task<VersionComparisonResult> CompareVersionsAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{_baseUrl}version");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var versionResponse = JsonSerializer.Deserialize<VersionResponse>(responseBody);

                    if (versionResponse != null && !string.IsNullOrEmpty(versionResponse.Version))
                    {
                        var apiVersion = new Version(versionResponse.Version);
                        var appVersion = new Version(_appVersion);

                        Debug.WriteLine($"[0x2] >>>>>>>> WEB: {apiVersion} APP: {appVersion}");
                        if (apiVersion > appVersion)
                        {
                            return VersionComparisonResult.OutOfDate;
                        }
                        else if (apiVersion < appVersion)
                        {
                            return VersionComparisonResult.NewerThanApi;
                        }
                        else
                        {
                            return VersionComparisonResult.UpToDate;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("API version is null or empty");
                        return VersionComparisonResult.Unknown;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to get API version");
                    return VersionComparisonResult.Unknown;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                System.Diagnostics.Debug.WriteLine($"Error comparing versions: {ex.Message}");
                return VersionComparisonResult.Unknown;
            }
        }

        private class VersionResponse
        {
            [JsonPropertyName("version")]
            public string Version { get; set; }
        }

        public enum VersionComparisonResult
        {
            UpToDate,
            OutOfDate,
            NewerThanApi,
            Unknown
        }
    }
}