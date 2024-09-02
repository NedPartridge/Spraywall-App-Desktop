using SpraywallAppDesktop.Helpers;
using SpraywallAppDesktop.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpraywallAppDesktop.Pages;

// Code behind for the page which manages the climbs on a given wall
public partial class ManageCLimbs : ContentPage
{
    // CLimbs on the wall
    List<ClimbDto> climbs = new();
    HttpClient client = new HttpClient();

    public ManageCLimbs()
	{
		InitializeComponent();
	}

    // Bind controls
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Get the wclimbs on the wall
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
        var response = await client.GetAsync(AppSettings.absGetClimbs + $"/{StateHelper.wallId}");
        if(!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Something's gone wrong", "Go home", "ok");
            await Shell.Current.GoToAsync("//" + nameof(Home));
            return;
        }

        // Bind 'all climbs' to all of the climbs
        climbs = JsonSerializer.Deserialize<List<ClimbDto>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        ClimbsListView.ItemsSource = climbs;

        // Get all flagged climbs, bind to other control
        var flaggedClimbs = new List<ClimbDto>();
        foreach(var c in climbs)
            if(c.Flagged)
                flaggedClimbs.Add(c);
        FlaggedClimbsListView.ItemsSource = flaggedClimbs;
    }

    // Manage which listview is shown
    private void SetFlaggedVisible(object sender, EventArgs e)
    {
        FlaggedClimbs.IsVisible = true;
        AllClimbs.IsVisible = false;
    }
    private void SetAllVisible(object sender, EventArgs e)
    {
        FlaggedClimbs.IsVisible = false;
        AllClimbs.IsVisible = true;
    }

    // Delete the tapped climb
    private async void OnClimbTapped(object sender, ItemTappedEventArgs e)
    {
        // Convert tapped event arguments into a helpful format (climbdto) to get params
        if (e.Item is ClimbDto tappedClimb)
        {
            // Delete the climb
            var response = await client.GetAsync(AppSettings.absDeleteClimb + $"/{StateHelper.wallId}/{tappedClimb.Id}");

            if(!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Failed", "Try again later", "ok");
                return;
            }

            await DisplayAlert("Succes", "Climb deleted", "ok");

            await Shell.Current.GoToAsync("//" + nameof(Home));
        }
    }
}