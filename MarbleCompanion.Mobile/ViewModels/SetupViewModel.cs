using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class SetupViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private int _selectedAvatarIndex;

    [ObservableProperty]
    private string? _selectedContinent;

    [ObservableProperty]
    private string? _selectedCountry;

    [ObservableProperty]
    private string? _dietType;

    [ObservableProperty]
    private string? _transportMode;

    [ObservableProperty]
    private string? _energySource;

    [ObservableProperty]
    private string? _flightFrequency;

    [ObservableProperty]
    private string? _shoppingHabits;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public List<string> Continents { get; } =
    [
        "Africa", "Asia", "Europe", "North America",
        "Oceania", "South America"
    ];

    public List<string> DietOptions { get; } =
        ["Vegan", "Vegetarian", "Pescatarian", "Flexitarian", "Omnivore", "High Meat"];

    public List<string> TransportOptions { get; } =
        ["Walk/Bike", "Public Transit", "Carpool", "Solo Car (Petrol)", "Solo Car (Diesel)", "Electric Vehicle"];

    public List<string> EnergyOptions { get; } =
        ["Renewable", "Mixed", "Fossil Fuel", "Unsure"];

    public List<string> FlightOptions { get; } =
        ["Never", "1-2 per year", "3-5 per year", "6+ per year"];

    public List<string> ShoppingOptions { get; } =
        ["Minimal", "Second-hand Focused", "Average", "Frequent New Purchases"];

    public SetupViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task CompleteSetupAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "Please enter a display name.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var quizAnswers = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(DietType))
                quizAnswers["dietType"] = DietType;
            if (!string.IsNullOrEmpty(TransportMode))
                quizAnswers["transportMode"] = TransportMode;
            if (!string.IsNullOrEmpty(EnergySource))
                quizAnswers["energySource"] = EnergySource;
            if (!string.IsNullOrEmpty(FlightFrequency))
                quizAnswers["flightFrequency"] = FlightFrequency;
            if (!string.IsNullOrEmpty(ShoppingHabits))
                quizAnswers["shoppingHabits"] = ShoppingHabits;

            var dto = new UserSetupDto
            {
                DisplayName = DisplayName.Trim(),
                AvatarIndex = SelectedAvatarIndex,
                RegionContinent = SelectedContinent,
                RegionCountry = SelectedCountry,
                BaselineQuizAnswers = quizAnswers
            };

            await _apiService.SetupAccountAsync(dto);
            await _navigationService.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Setup failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
