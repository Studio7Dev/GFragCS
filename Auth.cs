using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Management;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Maui.Storage;
using Windows.Media.Protection.PlayReady;

namespace GFrag
{
    [ArmDot.Client.VirtualizeCode]
    internal class Auth
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public class VerifyRequest
        {
            public string Token { get; set; }
        }
        public class TokenVerifyResponse
        {
            public string Message { get; set; }
            public string Username { get; set; }
        }

        public class TokenVerifyError
        {
            public string Error { get; set; }
        }
        public class RegisterResponse
        {
            public string Message { get; set; }
        }

        public class RegisterError
        {
            public string Error { get; set; }
        }


        public class LoginResponse
        {
            public string Token { get; set; }
        }

        public class LoginError
        {
            public string Error { get; set; }
        }

        public class TokenExpireRequest
        {
            public string Token { get; set; }
        }

        public class TokenExpireResponse
        {
            public string Message { get; set; }
        }

        public class TokenExpireError
        {
            public string Error { get; set; }
        }


        public Auth(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        public async Task<bool> ExpireToken(string token)
        {
            // Define the endpoint URL
            string url = $"{_baseUrl}/token_expire";

            // Create the JSON payload
            var payload = new
            {
                Token = token
            };

            // Serialize the payload to JSON
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Set up the request content
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Read the response content
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Handle the response based on the status code
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the JSON response
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        Console.WriteLine($"Success: {responseObject.message}");
                        return true; // Indicate success
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Parse the JSON response
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        Console.WriteLine($"Error: {responseObject.error}");
                        return false; // Indicate failure due to unauthorized
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected Error: {responseContent}");
                        return false; // Indicate failure due to unexpected error
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    return false; // Indicate failure due to exception
                }
            }
        }
        
        public async Task<(bool IsValid, string ExpirationInfo)> VerifyAndGetTokenInfoAsync(string token)
        {
            try
            {
                // Step 1: Verify the token
                var verifyRequest = new { Token = token };
                var verifyJson = JsonConvert.SerializeObject(verifyRequest);
                var verifyContent = new StringContent(verifyJson, Encoding.UTF8, "application/json");

                var verifyResponse = await _httpClient.PostAsync($"{_baseUrl}/token_verify", verifyContent);

                if (!verifyResponse.IsSuccessStatusCode)
                {
                    if (verifyResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var errorResponse = await verifyResponse.Content.ReadAsStringAsync();
                        try
                        {
                            var error = JsonConvert.DeserializeObject<TokenVerifyError>(errorResponse);
                            Debug.WriteLine($" >>>>> Token Verify Failed: {error.Error}");
                        }
                        catch (JsonException ex)
                        {
                            Debug.WriteLine($"Error deserializing error response: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($" >>>>> Token Verify Failed with status code {verifyResponse.StatusCode}");
                    }
                    return (false, null); // Token is invalid
                }

                // Step 2: Get token expiration info
                var expirationRequest = new { Token = token };
                var expirationJson = JsonConvert.SerializeObject(expirationRequest);
                var expirationContent = new StringContent(expirationJson, Encoding.UTF8, "application/json");

                var expirationResponse = await _httpClient.PostAsync($"{_baseUrl}/token_info", expirationContent);

                if (expirationResponse.IsSuccessStatusCode)
                {
                    var expirationResponseBody = await expirationResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($" >>>>> Token Expiration Info: {expirationResponseBody}");
                    return (true, expirationResponseBody); // Token is valid and expiration info is available
                }
                else
                {
                    Debug.WriteLine($" >>>>> Failed to retrieve token expiration info with status code {expirationResponse.StatusCode}");
                    return (true, null); // Token is valid but expiration info could not be retrieved
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP request error: {ex.Message}");
                return (false, null); // Token is invalid due to request failure
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return (false, null); // Token is invalid due to an unexpected error
            }
        }

        public async Task<string> GetTokenExpirationAsync(string token)
        {
            var requestUrl = $"{_baseUrl}/token_info"; // Replace with your actual endpoint URL
            var requestBody = new
            {
                Token = token
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody; // This will contain the expiration information
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                var verifyRequest = new VerifyRequest
                {
                    Token = token
                };

                var json = JsonConvert.SerializeObject(verifyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/token_verify", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        try
                        {
                            var result = JsonConvert.DeserializeObject<TokenVerifyResponse>(responseBody);
                            Debug.WriteLine($" >>>>> Token Verify Success! {result.Message}");
                            return true;
                        }
                        catch (JsonException ex)
                        {
                            Debug.WriteLine($"Error deserializing response: {ex.Message}");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.WriteLine(" >>>>> Empty response body");
                        return false;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        try
                        {
                            var error = JsonConvert.DeserializeObject<TokenVerifyError>(responseBody);
                            Debug.WriteLine($" >>>>> Token Verify Failed: {error.Error}");
                            return false;
                        }
                        catch (JsonException ex)
                        {
                            Debug.WriteLine($"Error deserializing response: {ex.Message}");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.WriteLine(" >>>>> Empty response body");
                        return false;
                    }
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($" >>>>> Token Verify Failed with status code {response.StatusCode}: {responseBody}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error verifying token: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public async Task Verify(ContentPage page)
        {
            var DisplayAHandler = new AlertService(Application.Current.MainPage);
            
            var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://"+ Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));
            var pageHandler = new PageHandler(page);
            var token = Preferences.Get("Token", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                pageHandler.ToggleVisibility();
            }
            else
            {
                try
                {
                    var isValid = await auth.VerifyTokenAsync(token);
                    if (!isValid)
                    {
                        pageHandler.ToggleVisibility();
                        //await DisplayAHandler.DisplayAlertAsync("Auth Error", "Failed to verify session token!", "OK");
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    pageHandler.ToggleVisibility();
                    await DisplayAHandler.DisplayAlertAsync("Auth Error", "Failed to verify session token!, Error: " + ex.Message, "OK");
                }
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string hwid)
        {
            try
            {
                var registerRequest = new RegisterRequest
                {
                    Username = username,
                    Password = password,
                    Hwid = hwid
                };

                var json = JsonConvert.SerializeObject(registerRequest);
                Debug.WriteLine(json);  // Add this line to print the JSON data
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<RegisterResponse>(responseBody);
                    Debug.WriteLine($" >>>>> Register Success! {result.Message}");
                    return true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<RegisterError>(responseBody);
                    Debug.WriteLine($" >>>>> Register Failed: {error.Error}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        public async Task<string> LoginAsync(string username, string password, string hwid)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = username,
                    Password = password,
                    Hwid = hwid
                };

                var json = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
                    Debug.WriteLine($" >>>>> Login Success! Token: {result.Token}");
                    return result.Token;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<LoginError>(responseBody);
                    Debug.WriteLine($" >>>>> Login Failed: {error.Error}");
                    return error.Error;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error logging in user: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task LogoutAsync(string token)
        {
            try
            {
                var expireRequest = new TokenExpireRequest
                {
                    Token = token
                };

                var json = JsonConvert.SerializeObject(expireRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/token_expire", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TokenExpireResponse>(responseBody);
                    Debug.WriteLine($" >>>>> Logout Success! {result.Message}");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<TokenExpireError>(responseBody);
                    Debug.WriteLine($" >>>>> Logout Failed: {error.Error}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error logging out: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }


        public string GetHwid()
        {
            try
            {
                var hwid = string.Empty;

                // Get the CPU ID
                var cpuId = GetCpuId();

                // Get the Motherboard ID
                var motherboardId = GetMotherboardId();

                // Get the Disk ID
                var diskId = GetDiskId();

                // Generate the HWID
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(cpuId + motherboardId + diskId);
                var hashBytes = sha256.ComputeHash(bytes);
                foreach (var b in hashBytes)
                {
                    hwid += b.ToString("x2");
                }

                return hwid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetCpuId()
        {
            try
            {
                var cpuId = string.Empty;
                var cpuInfo = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in cpuInfo.Get())
                {
                    cpuId = obj["ProcessorId"].ToString();
                    break;
                }
                return cpuId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetMotherboardId()
        {
            try
            {
                var motherboardId = string.Empty;
                var motherboardInfo = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject obj in motherboardInfo.Get())
                {
                    motherboardId = obj["SerialNumber"].ToString();
                    break;
                }
                return motherboardId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetDiskId()
        {
            try
            {
                var diskId = string.Empty;
                var diskInfo = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject obj in diskInfo.Get())
                {
                    diskId = obj["SerialNumber"].ToString();
                    break;
                }
                return diskId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hwid { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hwid { get; set; }
    }
}