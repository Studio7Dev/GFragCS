using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;




namespace GFrag
{
    public partial class AppShell : Shell
    {
        public bool IsLoggedIn { get; set; }
        public AppShell()
        {
            InitializeComponent();

            var HttpServer = new HttpServer();
            HttpServer.NewHttpServer(52347);
            //var webServer = new GFrag.WebServer(52347);
            //webServer.Run();

            //Console.WriteLine("Press 'q' to quit.");
            //while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
            //webServer.Stop();
        
            BindingContext = new AppShellViewModel();
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(currentDirectory);
            Environment.CurrentDirectory = currentDirectory;

        }


        //protected override bool OnBackButtonPressed()
        //{
        //    var result = DisplayAlert("Confirm", "Are you sure you want to exit?", "Yes", "No");
        //    if (result.Result)
        //    {
        //        Environment.Exit(0);
        //    }
        //    return true;
        //}
        protected override bool OnBackButtonPressed()
        {
            // Handle back button press manually
            return true;
        }
        protected override async void OnAppearing()
        {
            var displayAlertHandler = new AlertService(Application.Current.MainPage);
            base.OnAppearing();

            // Initialize the Auth class with server details
            var auth = new Auth(Preferences.Get("ServerProtocol", "") + "://" + Preferences.Get("ServerHost", "") + ":" + Preferences.Get("ServerPort", ""));

            // Retrieve the token from preferences
            var token = Preferences.Get("Token", string.Empty);
            Debug.WriteLine(" ------------ Session Token Provided " + token);

            if (string.IsNullOrEmpty(token))
            {
                // Navigate to the login page if no token is found
                await Navigation.PushAsync(new LoginPage());
            }
            else
            {
                // Verify the token and get expiration info
                var (isValid, expirationInfo) = await auth.VerifyAndGetTokenInfoAsync(token);

                if (!isValid)
                {
                    // Navigate to the login page if the token is invalid
                    await Navigation.PushAsync(new LoginPage());

                    // Display an alert to the user
                   
                    //await displayAlertHandler.DisplayAlertAsync("Auth Error", "Failed to verify session token!", "OK");
                }
                else
                {
                    // Log the expiration info if available
                    if (!string.IsNullOrEmpty(expirationInfo))
                    {
                        Debug.WriteLine($"Token is valid. Expiration info: {expirationInfo}");
                    }
                    else
                    {
                        Debug.WriteLine("Token is valid, but expiration info could not be retrieved.");
                    }

                    // Update the ViewModel to indicate the user is logged in
                    var viewModel = (AppShellViewModel)App.Current.MainPage.BindingContext;
                    viewModel.IsLoggedIn = true;

                    // Optional: Add a delay for smoother navigation
                    await Task.Delay(200);
                    var _expirationInfo = ParseExpirationInfo(expirationInfo);
                    await displayAlertHandler.DisplayAlertAsync("Auth Success", "Successfully Authenticated, Session Expiration: " + _expirationInfo.Expiration, "OK");
                    // Navigate to the next page if needed
                    // await Navigation.PushAsync(new MultiUpload());
                }
            }
        }
        public class ExpirationInfo
        {
            [JsonPropertyName("expiration")]
            public DateTime Expiration { get; set; }
        }
        public static ExpirationInfo ParseExpirationInfo(string json)
        {
            try
            {
                // Deserialize the JSON string into the ExpirationInfo object
                var expirationInfo = JsonSerializer.Deserialize<ExpirationInfo>(json);
                return expirationInfo;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }
        private async void Shell_Loaded(object sender, EventArgs e)
        {

            await Task.Run(async () => {
                Device.BeginInvokeOnMainThread( async () =>
                {
                    var pc = new PrivateClass();
                    await pc.CheckVersion();
                });
                await PrivateClass.MeasureNetworkSpeedAsync();

            });
            
        }

        

    }

    public class AppShellViewModel : INotifyPropertyChanged
    {
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsLoginVisible));
                OnPropertyChanged(nameof(IsOtherPagesVisible));
            }
        }

        public bool IsLoginVisible => !IsLoggedIn;

        public bool IsOtherPagesVisible => IsLoggedIn;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
