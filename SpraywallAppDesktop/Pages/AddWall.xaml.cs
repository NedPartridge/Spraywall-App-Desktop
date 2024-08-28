using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SpraywallAppDesktop.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SpraywallAppDesktop.Pages
{
    public partial class AddWall : ContentPage
    {
        private FileResult _selectedImage;

        public AddWall()
        {
            InitializeComponent();
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            // Prevent inputs
            Overlay.IsVisible = true;


            // Open file picker and select an image
            _selectedImage = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an Image"
            });

            // Optionally, show the selected image or file name
            if (_selectedImage == null)
            {
                await DisplayAlert("You can't choose null", "How would one pick holds on a null image?", "ok");
                Overlay.IsVisible = false;
                return;
            }

            // Only .webp images are displayed on the desktop app (android is fine lol)
            // Rather than convert on the server side, make it the user's problem
            if(!_selectedImage.FileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Invalid file type: .webp only", "Yes, really. Blame MAUI", "ok");
                Overlay.IsVisible = false;
                _selectedImage = null;
                return;
            }

            // Succesful selection: set the background

            // Load the image as a Stream, create an ImageSource from the stream, and set the
            // button's background to the image
            var stream = await _selectedImage.OpenReadAsync();
            var imageSource = ImageSource.FromStream(() => stream);
            SelectImageBackground.Source = imageSource;
            SelectImageBackground.Opacity = 1;


            Overlay.IsVisible = false;
        }

        // Validate, and submit the wall creation request.
        // 
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Prevent inputs while waiting for server
            Overlay.IsVisible = true;

            // Image must be entered (cannot be null)
            if (string.IsNullOrEmpty(WallName.Text) || _selectedImage == null)
            {
                await DisplayAlert("Error", "Please enter a name and select an image.", "OK");
                Overlay.IsVisible = false;
                return;
            }

            var wallName = WallName.Text;
            var imageFile = _selectedImage;

            // Prepare the form data
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(wallName), "Name");
            var imageContent = new StreamContent(await imageFile.OpenReadAsync());
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); // Adjust as needed
            formData.Add(imageContent, "Image", imageFile.FileName);

            // Send the request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",AppSettings.Token);
            var response = await client.PostAsync(AppSettings.absWallsAddress, formData);
            string message = await response.Content.ReadAsStringAsync();

            // Respond to all of server's possible responses
            if (response.IsSuccessStatusCode)
            {
                // Success :D
                await DisplayAlert("Success", "Wall added successfully!", "OK");
                Overlay.IsVisible = false;

                // Reset fields
                _selectedImage = null;
                WallName.Text = null;
                SelectImageBackground.Opacity = 0;
                
                // Nav to main
                await Shell.Current.GoToAsync("//" + nameof(Home));
            }
            else // Hanfle errors
            {
                if (message == "Wall name is in use")
                {
                    await DisplayAlert("Wall name is in use", "Try being more creative", "ok");
                    Overlay.IsVisible = false;
                    return;
                }
                else
                {
                    // Flat failure code: means either unauthentic, or just crap, request
                    // Ideally, this won't ever happen.
                    Overlay.IsVisible = false;
                    await DisplayAlert("Error", "Failed to add wall.", "OK");
                }
            }
        }
    }
}
