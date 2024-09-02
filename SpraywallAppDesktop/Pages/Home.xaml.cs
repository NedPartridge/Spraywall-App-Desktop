using SpraywallAppDesktop.Helpers;
using SpraywallAppDesktop.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpraywallAppDesktop.Pages;

// Code-behind file for Home.xaml - see there for page purpose.
public partial class Home : ContentPage
{
	// Client to manage web requests
	HttpClient client = new HttpClient();

	List<WallDTO> walls = new List<WallDTO>();

	Wall wall;

	// Initialise component
	public Home()
	{
		InitializeComponent();
	}

	// Retrieve the list of walls the user manages
	private async Task GetManagedWalls()
	{
		try
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
			HttpResponseMessage response = await client.GetAsync(AppSettings.absGetManagedWallsAddress);
			string jsonWalls = await response.Content.ReadAsStringAsync();

			// Deserialise the list of walls, for use in future requests
			walls = JsonSerializer.Deserialize<List<WallDTO>>(jsonWalls);
		}
		catch (Exception ex) { await DisplayAlert("Error", "Check connection", "ok"); }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

		await GetManagedWalls();

		// If user does not manage any walls, 'gently' direct them to the add screen
		if(walls.Count() == 0)
		{
            NoWallsContent.IsVisible = true;
            MainContent.IsVisible = false;
            ErrorContent.IsVisible = false;
		}
		// Otherwise, open the first wall in their list
		else
		{
			wall = await GetWall(walls[0].Id);
            if (wall == null) // Request failed. somehow
            {
                NoWallsContent.IsVisible = false;
                MainContent.IsVisible = false;
                ErrorContent.IsVisible = true;
            }
            else // Success! :D
            {
                NoWallsContent.IsVisible = false;
                MainContent.IsVisible = true;
                ErrorContent.IsVisible = false;
                SetBindings();
            }
		}
    }

    // Navigate to the wall editing screen for the currently selected wall
    // No existence check required on wall.id, because if the button is available, it's valid.
    private async void OnEditWallClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(EditWall) + $"?wallId={wall.Id}");
    }


    // Navigate to the climb editing screen for the currently selected wall
    // No existence check required on wall.id, because if the button is available, it's valid.
    private async void OnEditClimbClicked(object sender, EventArgs e)
    {
        StateHelper.wallId = wall.Id;
        await Shell.Current.GoToAsync("//" + nameof(ManageCLimbs));
    }

    private void OnOpenSwitchWallClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = true;
    }
        
    private void OnCloseOverlayClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = false;
    }

    // When one of the items in the switch wall dropdown is clicked, 
    // change the display to that wall.
    private async void OnSwitchWallClicked(object sender, ItemTappedEventArgs e)
    {
        if (e.Item == null) // Ensure it's a valid click: existence check.
            return;

        WallDTO tappedWall = e.Item as WallDTO;

        if (tappedWall == null)
            return;

        // Prevent clicks while loading
        Overlay.IsVisible = true;
        // Set the wall to that at the ID
        wall = await GetWall(tappedWall.Id);

        if (wall == null) // Request failed. somehow
        {
            NoWallsContent.IsVisible = false;
            MainContent.IsVisible = false;
            ErrorContent.IsVisible = true;
        }
        else // Success! :D
        {
            NoWallsContent.IsVisible = false;
            MainContent.IsVisible = true;
            ErrorContent.IsVisible = false;
            SetBindings();
        }

        Overlay.IsVisible = false;
        WallSelectOverlay.IsVisible = false;
        
        // Deselect the item
        ((ListView)sender).SelectedItem = null;
    }

    // Retrieve a wall from the webserver, and store it's data
	private async Task<Wall?> GetWall(int id)
	{
		HttpResponseMessage response = await client.GetAsync(AppSettings.absGetWallAddress + $"/{id}");


        // Check if the request was successful
        if (response.IsSuccessStatusCode)
        {
            // Read the response content as a stream
            var responseStream = await response.Content.ReadAsStreamAsync();

            // Deserialize the response into a Wall object
            // Save the wall and json data locally, rather than holding in memory.
            using (var document = JsonDocument.Parse(responseStream))
            {
                var root = document.RootElement;

                // Get wall ID and Name
                int wallIdFromResponse = root.GetProperty("id").GetInt32();
                string wallName = root.GetProperty("name").GetString();

                // Download the Image - stored at the one address, more than one wall image
                // is never needed, and hence, should never be stored.
                string imageBase64 = root.GetProperty("image").GetString();
                byte[] imageBytes = Convert.FromBase64String(imageBase64); string imagePath = Path.Combine(FileSystem.AppDataDirectory, "wall_image.webp");
                await File.WriteAllBytesAsync(imagePath, imageBytes);

                // Read and save the JSON file content
                string jsonFileContent = root.GetProperty("jsonFile").GetString();
                string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "holds.json");
                await File.WriteAllTextAsync(jsonFilePath, jsonFileContent);

                return new Wall()
                {
                    Id = wallIdFromResponse,
                    Name = wallName,
                    ImagePath = imagePath,
                    JsonPath = jsonFilePath
                };
            }
        }
        else // Manage failed requests
        {
            if (await response.Content.ReadAsStringAsync() == "Invalid credentials")
                await DisplayAlert("Invalid request", "Try logging out/in?", "ok");
            else
                await DisplayAlert("Something went wrong", "We don't know what, please try again later", "ok");
            return null;
        } 
    }

    // Set the binding on the main page to the data collected from the webserver
    // Assumes wall is not null, because is only called from places where the data has already been retrieved/validated.
    private async void SetBindings()
    {
        WallTitle.Text = "Wall: " + wall.Name;
        WallImage.Source = ImageSource.FromFile(wall.ImagePath);
        WallImage.Opacity = 1;

        // Bind the walls dropdown to the list of walls
        WallsListView.ItemsSource = walls;
    }
}