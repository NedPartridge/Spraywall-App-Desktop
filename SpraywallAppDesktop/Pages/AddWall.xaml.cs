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
            // Open file picker and select an image
            _selectedImage = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an Image",
                FileTypes = FilePickerFileType.Images
            });

            // Optionally, show the selected image or file name
            if (_selectedImage != null)
            {
                // Display the file name or do something with it
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(WallName.Text) || _selectedImage == null)
            {
                await DisplayAlert("Error", "Please enter a name and select an image.", "OK");
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

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Wall added successfully!", "OK");
                // Optionally, navigate away or reset the form
            }
            else
            {
                await DisplayAlert("Error", "Failed to add wall.", "OK");
                Debug.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
