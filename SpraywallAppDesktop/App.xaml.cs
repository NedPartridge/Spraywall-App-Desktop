﻿using SpraywallAppDesktop.Models;

namespace SpraywallAppDesktop
{
    public partial class App : Application
    {
        // HttpClients should be reused where possible - as such, this one will
        // be initialised in the App constructor, and serve for the full file.
        private readonly HttpClient _client;

        // Initialise the application
        public App()
        {
            _client = new HttpClient();

            // Open the UI
            InitializeComponent();
            MainPage = new AppShell();

            // Set appsettings variables
            InitialiseAppSettings();
        }

        // Appsettings requires an httpget to initialise the public key variable
        // This must be ran asynchronously, so it is separated from the main constructor
        private async void InitialiseAppSettings()
        {
            try
            {
                // Determine which platform's URL to use - required because android is quirky like that
                string baseUrl;
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    baseUrl = AppSettings.AndroidBaseUrl;
                else
                    baseUrl = AppSettings.DefaultBaseUrl;

                // Initialise variable appsettings URLs
                AppSettings.absRetrievePublicKeyAddress = new(baseUrl + AppSettings.RetrievePublicKeyAddress);
                AppSettings.absSignUpAddress = new(baseUrl + AppSettings.SignUpAddress);
                AppSettings.absLogInAddress = new(baseUrl + AppSettings.LogInAddress);
                AppSettings.absWallsAddress = new(baseUrl + AppSettings.WallsAddress);
                AppSettings.absGetManagedWallsAddress = new(baseUrl + AppSettings.GetManagedWallsAddress);
                AppSettings.absGetWallAddress = new(baseUrl + AppSettings.GetWallAddress);
                AppSettings.absUpdateWallAddress = new(baseUrl + AppSettings.UpdateWallAddress);
                AppSettings.absGetWallNameAddress = new(baseUrl + AppSettings.GetWallNameAddress);
                AppSettings.absGetUserAddress = new(baseUrl + AppSettings.GetUserAddress);
                AppSettings.absEditUserAddress = new(baseUrl + AppSettings.EditUserAddress);
                AppSettings.absGetClimbs = new(baseUrl + AppSettings.GetClimbs);
                AppSettings.absDeleteClimb = new(baseUrl + AppSettings.DeleteClimb);

                // Retrieve the public key
                HttpResponseMessage response = await _client.GetAsync(AppSettings.absRetrievePublicKeyAddress);

                // Update appsettings
                string publicKeyXML = await response.Content.ReadAsStringAsync();
                AppSettings.PublicKeyXML = publicKeyXML;
            }
            // Errors will likely only occur if no wifi, or if server is down.
            // If no wifi, system will alert user
            // Server is expected to have 115% uptime, so the second possibility is, in fact, impossible.
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}
