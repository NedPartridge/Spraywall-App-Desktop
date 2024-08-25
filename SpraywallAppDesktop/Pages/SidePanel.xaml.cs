namespace SpraywallAppDesktop.Pages
{
    public partial class SidePanel : ContentView
    {
        public static readonly BindableProperty ButtonParameterProperty =
            BindableProperty.Create(
                nameof(ButtonParameter),
                typeof(string),
                typeof(SidePanel),
                propertyChanged: OnButtonParameterChanged);

        public string ButtonParameter
        {
            get => (string)GetValue(ButtonParameterProperty);
            set => SetValue(ButtonParameterProperty, value);
        }

        public SidePanel()
        {
            InitializeComponent();
        }

        private static void OnButtonParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (SidePanel)bindable;
            control.UpdateSettingsButtonColor((string)newValue);
        }


        // Use colours to show the user which page they're on
        private void UpdateSettingsButtonColor(string parameter)
        {
            if (parameter == "settings")
            {
                SettingsButton.BackgroundColor = (Color)Application.Current.Resources["ForestGreen"];
                SpraywallAppButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];
                AddWallButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];

                SettingsButton.TextColor = Colors.White;
                SpraywallAppButton.TextColor = Colors.Black;
                AddWallButton.TextColor = Colors.Black;
            }
            else if (parameter == "home")
            {
                SettingsButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];
                SpraywallAppButton.BackgroundColor = (Color)Application.Current.Resources["ForestGreen"];
                AddWallButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];

                SettingsButton.TextColor = Colors.Black;
                SpraywallAppButton.TextColor = Colors.White;
                AddWallButton.TextColor = Colors.Black;
            }
            else if (parameter == "add wall")
            {
                SettingsButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];
                SpraywallAppButton.BackgroundColor = (Color)Application.Current.Resources["FernGreen"];
                AddWallButton.BackgroundColor = (Color)Application.Current.Resources["ForestGreen"];

                SettingsButton.TextColor = Colors.Black;
                SpraywallAppButton.TextColor = Colors.Black;
                AddWallButton.TextColor = Colors.White;
            }
        }

        // Navigate to home page
        private async void OnSpraywallAppClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }

        // Navigate to add wall page
        private async void OnAddWallClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//" + nameof(AddWall));
        }

        // Navigate to settings page
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//" + nameof(Settings));
        }

        // Open contact mailto
        private void OnContactUsClicked(object sender, EventArgs e)
        {
            
        }
    }
}
