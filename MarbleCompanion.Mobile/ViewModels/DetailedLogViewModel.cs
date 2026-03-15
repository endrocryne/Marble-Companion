using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
[QueryProperty(nameof(CategoryName), "category")]
public partial class DetailedLogViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string? _categoryName;

    [ObservableProperty]
    private ActionCategory _category;

    // Transport fields
    [ObservableProperty]
    private double _distanceKm;

    [ObservableProperty]
    private string _vehicleType = string.Empty;

    // Food fields
    [ObservableProperty]
    private string _mealType = string.Empty;

    [ObservableProperty]
    private string _proteinSource = string.Empty;

    // Energy fields
    [ObservableProperty]
    private double _kWh;

    // Shopping fields
    [ObservableProperty]
    private string _itemCategory = string.Empty;

    // Travel fields
    [ObservableProperty]
    private string _originAirport = string.Empty;

    [ObservableProperty]
    private string _destinationAirport = string.Empty;

    // Waste fields
    [ObservableProperty]
    private string _wasteType = string.Empty;

    [ObservableProperty]
    private double _weightKg;

    [ObservableProperty]
    private int _lpAwarded;

    [ObservableProperty]
    private double _co2eSaved;

    [ObservableProperty]
    private bool _showSuccess;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public List<string> VehicleTypes { get; } =
        ["Petrol Car", "Diesel Car", "Hybrid", "Electric Vehicle", "Motorcycle", "Bus", "Train", "Bicycle"];

    public List<string> MealTypes { get; } =
        ["Breakfast", "Lunch", "Dinner", "Snack"];

    public List<string> ProteinSources { get; } =
        ["Plant-based", "Poultry", "Fish", "Beef", "Pork", "Dairy", "Eggs", "None"];

    public List<string> ItemCategories { get; } =
        ["Clothing", "Electronics", "Furniture", "Books", "Household", "Personal Care", "Other"];

    public List<string> WasteTypes { get; } =
        ["Recyclable", "Compostable", "Electronic", "Textile", "Hazardous", "General"];

    public DetailedLogViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    partial void OnCategoryNameChanged(string? value)
    {
        if (Enum.TryParse<ActionCategory>(value, true, out var category))
            Category = category;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ShowSuccess = false;

            var detailedData = BuildDetailedData();

            var request = new LogActionRequest
            {
                Category = Category,
                ActionTemplateId = Guid.Empty,
                IsDetailed = true,
                DetailedData = detailedData
            };

            var response = await _apiService.LogActionAsync(request);

            LpAwarded = response.LPAwarded;
            Co2eSaved = response.CO2eSaved;
            ShowSuccess = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to submit: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DismissSuccessAsync()
    {
        ShowSuccess = false;
        await _navigationService.GoBackAsync();
    }

    private Dictionary<string, string> BuildDetailedData()
    {
        return Category switch
        {
            ActionCategory.Transport => new Dictionary<string, string>
            {
                ["distanceKm"] = DistanceKm.ToString("F1"),
                ["vehicleType"] = VehicleType
            },
            ActionCategory.Food => new Dictionary<string, string>
            {
                ["mealType"] = MealType,
                ["proteinSource"] = ProteinSource
            },
            ActionCategory.Energy => new Dictionary<string, string>
            {
                ["kWh"] = KWh.ToString("F2")
            },
            ActionCategory.Shopping => new Dictionary<string, string>
            {
                ["itemCategory"] = ItemCategory
            },
            ActionCategory.Travel => new Dictionary<string, string>
            {
                ["originAirport"] = OriginAirport,
                ["destinationAirport"] = DestinationAirport
            },
            ActionCategory.Waste => new Dictionary<string, string>
            {
                ["wasteType"] = WasteType,
                ["weightKg"] = WeightKg.ToString("F2")
            },
            _ => new Dictionary<string, string>()
        };
    }
}
