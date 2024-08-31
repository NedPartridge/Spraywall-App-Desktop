using SpraywallAppDesktop.Models;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SpraywallAppDesktop.Pages;

// Code behind for the edit wall page - allows the user to update the name and image of a wall they manage.
// Query param gets the id of the wall to edit when navigated to
[QueryProperty(nameof(WallId), "wallId")]
public partial class EditWall : ContentPage
{
    public int WallId { get; set; }
	public EditWall()
	{
		InitializeComponent();
	}


    private FileResult _selectedImage;

    // A new image is being selected
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
        if (!_selectedImage.FileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
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

    // Validate, and submit the wall editing request.
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
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        formData.Add(imageContent, "Image", imageFile.FileName);

        // Send the request
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
        var response = await client.PostAsync(AppSettings.absUpdateWallAddress + $"/{WallId}", formData);
        string message = await response.Content.ReadAsStringAsync();

        // Respond to all of server's possible responses
        if (response.IsSuccessStatusCode)
        {
            // Success :D
            await DisplayAlert("Success", "Wall updated successfully!", "OK");
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reset fields
        _selectedImage = null;
        WallName.Text = null;
        SelectImageBackground.Opacity = 0;
    }
}