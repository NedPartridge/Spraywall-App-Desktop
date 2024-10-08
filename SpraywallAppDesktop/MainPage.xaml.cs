﻿using SpraywallAppDesktop.Pages;

namespace SpraywallAppDesktop
{
    public partial class MainPage : ContentPage
    {
        // Initialise the component - only applicable to certain deployment platforms.
        public MainPage()
        {
            InitializeComponent();
        }

        // Send user to account log in page
        private async void LogInButtonClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//" + nameof(LogIn));
        }

        // Send user to new account creation page
        private async void SignUpButtonClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//" + nameof(SignUp));
        }
    }

}
