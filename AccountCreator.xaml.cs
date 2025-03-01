using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GFrag
{
    public partial class AccountCreator : ContentPage
    {
        private string _fullText = ""; // Holds the full text for scrolling
        private int _totalAccounts = 0;
        private int _accountsCreated = 0;

        public AccountCreator()
        {
            InitializeComponent();
            BackgroundManager.WebViewRegistry.Add(backgroundWebView);

        }

        private void Run_PointerExited(object sender, PointerEventArgs e)
        {
            (sender as Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#4286F5");
        }

        private void Run_PointerEntered(object sender, PointerEventArgs e)
        {
            (sender as Button).BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#5A9AF5");
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

                var button_gradient = new Color[]
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

                // Ensure AFrame is not null
                if (AFrame == null)
                {
                    Debug.WriteLine("AFrame is null");
                    return;
                }

                // Ensure BFrame is not null
                if (BFrame == null)
                {
                    Debug.WriteLine("BFrame is null");
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

                var C_animation = new Animation(v => {

                    var index = (int)(v * (button_gradient.Length - 1));
                    var color = button_gradient[index];
                    RunScriptBtn.BorderColor = color;


                }, 0, 1, easing: Easing.Linear);
                C_animation.Commit(this, "CButtonAnimation", length:2500, repeat: () => true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private async void OnRunScriptButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Get user inputs from the UI
                string projectIdPrefix = ProjectIdPrefixEntry.Text;
                int projectStartNumber = int.Parse(ProjectStartNumberEntry.Text);
                int projectEndNumber = int.Parse(ProjectEndNumberEntry.Text);
                int serviceAccountStartNumber = int.Parse(ServiceAccountStartNumberEntry.Text);
                int serviceAccountEndNumber = int.Parse(ServiceAccountEndNumberEntry.Text);
                string workingDirectory = Preferences.Get("AccountsDirectory", "");

                // Validate inputs
                if (string.IsNullOrEmpty(projectIdPrefix))
                {
                    AppendToOutput("Project ID prefix cannot be empty.\n");
                    return;
                }

                if (projectStartNumber > projectEndNumber)
                {
                    AppendToOutput("Project start number cannot be greater than project end number.\n");
                    return;
                }

                if (serviceAccountStartNumber > serviceAccountEndNumber)
                {
                    AppendToOutput("Service account start number cannot be greater than service account end number.\n");
                    return;
                }

                // Calculate the total number of accounts
                _totalAccounts = (projectEndNumber - projectStartNumber + 1) * (serviceAccountEndNumber - serviceAccountStartNumber + 1);

                // Reset the progress bar and label
                ProgressBar.Progress = 0;
                ProgressLabel.Text = "Progress: 0%";

                // Change the working directory
                if (!string.IsNullOrEmpty(workingDirectory) && Directory.Exists(workingDirectory))
                {
                    Environment.CurrentDirectory = workingDirectory;
                }
                else
                {
                    AppendToOutput("Invalid working directory. Using default.\n");
                }

                // Run the logic
                await CreateProjectsAndServiceAccounts(
                    projectIdPrefix,
                    projectStartNumber,
                    projectEndNumber,
                    serviceAccountStartNumber,
                    serviceAccountEndNumber
                );
            }
            catch (FormatException ex)
            {
                AppendToOutput("Invalid input format. Please check your inputs.\n");
                Debug.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                AppendToOutput("An error occurred. Please try again later.\n");
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task CreateProjectsAndServiceAccounts(
            string projectIdPrefix,
            int projectStartNumber,
            int projectEndNumber,
            int serviceAccountStartNumber,
            int serviceAccountEndNumber)
        {
            try
            {
                for (int projectNumber = projectStartNumber; projectNumber <= projectEndNumber; projectNumber++)
                {
                    string projectId = $"{projectIdPrefix}{projectNumber}";
                    AppendToOutput($"Creating Project: {projectId}\n");

                    // Create the project
                    await RunCommandAsync($"gcloud projects create {projectId} --set-as-default");

                    // Enable Google Drive API
                    await RunCommandAsync($"gcloud services enable drive.googleapis.com --project={projectId}");

                    // Set the project ID
                    await RunCommandAsync($"gcloud config set project {projectId}");

                    // Create service accounts
                    for (int accountNumber = serviceAccountStartNumber; accountNumber <= serviceAccountEndNumber; accountNumber++)
                    {
                        string serviceAccountId = $"data-{GenerateRandomHash(5)}-{accountNumber}";
                        string keyFileName = $"{serviceAccountId}.json";

                        AppendToOutput($"Creating Service Account: {serviceAccountId}\n");

                        // Create service account
                        await RunCommandAsync($"gcloud iam service-accounts create {serviceAccountId} --description='nb' --display-name='{serviceAccountId}'");

                        // Add IAM policy binding
                        await RunCommandAsync($"gcloud projects add-iam-policy-binding {projectId} --member=\"serviceAccount:{serviceAccountId}@{projectId}.iam.gserviceaccount.com\" --role=\"roles/owner\"");

                        // Create service account key
                        await RunCommandAsync($"gcloud iam service-accounts keys create {keyFileName} --iam-account={serviceAccountId}@{projectId}.iam.gserviceaccount.com");

                        // Update the progress bar and label
                        _accountsCreated++;
                        double progress = (double)_accountsCreated / _totalAccounts;
                        ProgressBar.Progress = progress;
                        ProgressLabel.Text = $"Progress: {(int)(progress * 100)}%";
                    }
                }
                _fullText = "";
                ProgressBar.Progress = 0;
                ProgressLabel.Text = $"Progress: 0%";
                AppendToOutput("All projects and service accounts created successfully!\n");
            }
            catch (Exception ex)
            {
                AppendToOutput("An error occurred while creating projects and service accounts. Please try again later.\n");
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task RunCommandAsync(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    AppendToOutput($"Output: {output}\n");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    AppendToOutput($">: {error}\n");
                }
            }
            catch (Exception ex)
            {
                AppendToOutput("An error occurred while running the command. Please try again later.\n");
                Debug.WriteLine(ex.ToString());
            }
        }

        private string GenerateRandomHash(int length)
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random();
                var hash = new StringBuilder();

                for (int i = 0; i < length; i++)
                {
                    hash.Append(chars[random.Next(chars.Length)]);
                }

                return hash.ToString();
            }
            catch (Exception ex)
            {
                AppendToOutput("An error occurred while generating the random hash. Please try again later.\n");
                Debug.WriteLine(ex.ToString());
                return string.Empty;
            }
        }

        private void AppendToOutput(string text)
        {
            try
            {
                _fullText += text; // Append new text to the full text
                UpdateOutputLabel(); // Update the output label
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void UpdateOutputLabel()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        OutputLabel.Text = _fullText;
                        OutputScrollView.ScrollToAsync(OutputLabel, ScrollToPosition.End, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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
    }
}