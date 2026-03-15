using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class QuickLogViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ActionCategory> _categories = new(Enum.GetValues<ActionCategory>());

    [ObservableProperty]
    private ActionCategory _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<ActionTemplateItem> _actionTemplates = [];

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

    public QuickLogViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        SelectedCategory = ActionCategory.Transport;
        LoadTemplatesForCategory(SelectedCategory);
    }

    partial void OnSelectedCategoryChanged(ActionCategory value)
    {
        LoadTemplatesForCategory(value);
    }

    private void LoadTemplatesForCategory(ActionCategory category)
    {
        var templates = category switch
        {
            ActionCategory.Transport =>
            [
                new ActionTemplateItem(Guid.Parse("00000001-0001-0001-0001-000000000001"), "Biked to work", "🚲"),
                new ActionTemplateItem(Guid.Parse("00000001-0001-0001-0001-000000000002"), "Took public transit", "🚌"),
                new ActionTemplateItem(Guid.Parse("00000001-0001-0001-0001-000000000003"), "Carpooled", "🚗"),
                new ActionTemplateItem(Guid.Parse("00000001-0001-0001-0001-000000000004"), "Walked", "🚶"),
            ],
            ActionCategory.Food =>
            [
                new ActionTemplateItem(Guid.Parse("00000002-0002-0002-0002-000000000001"), "Plant-based meal", "🥗"),
                new ActionTemplateItem(Guid.Parse("00000002-0002-0002-0002-000000000002"), "No food waste today", "♻️"),
                new ActionTemplateItem(Guid.Parse("00000002-0002-0002-0002-000000000003"), "Cooked at home", "🍳"),
                new ActionTemplateItem(Guid.Parse("00000002-0002-0002-0002-000000000004"), "Local produce", "🌽"),
            ],
            ActionCategory.Energy =>
            [
                new ActionTemplateItem(Guid.Parse("00000003-0003-0003-0003-000000000001"), "Turned off lights", "💡"),
                new ActionTemplateItem(Guid.Parse("00000003-0003-0003-0003-000000000002"), "Used smart thermostat", "🌡️"),
                new ActionTemplateItem(Guid.Parse("00000003-0003-0003-0003-000000000003"), "Air-dried laundry", "👕"),
                new ActionTemplateItem(Guid.Parse("00000003-0003-0003-0003-000000000004"), "Shorter shower", "🚿"),
            ],
            ActionCategory.Shopping =>
            [
                new ActionTemplateItem(Guid.Parse("00000004-0004-0004-0004-000000000001"), "Bought second-hand", "🏷️"),
                new ActionTemplateItem(Guid.Parse("00000004-0004-0004-0004-000000000002"), "Repaired an item", "🔧"),
                new ActionTemplateItem(Guid.Parse("00000004-0004-0004-0004-000000000003"), "Brought reusable bag", "🛍️"),
                new ActionTemplateItem(Guid.Parse("00000004-0004-0004-0004-000000000004"), "Skipped unnecessary purchase", "🚫"),
            ],
            ActionCategory.Travel =>
            [
                new ActionTemplateItem(Guid.Parse("00000005-0005-0005-0005-000000000001"), "Took train instead of flying", "🚆"),
                new ActionTemplateItem(Guid.Parse("00000005-0005-0005-0005-000000000002"), "Staycation day", "🏠"),
                new ActionTemplateItem(Guid.Parse("00000005-0005-0005-0005-000000000003"), "Offset a flight", "✈️"),
            ],
            ActionCategory.Waste =>
            [
                new ActionTemplateItem(Guid.Parse("00000006-0006-0006-0006-000000000001"), "Recycled correctly", "♻️"),
                new ActionTemplateItem(Guid.Parse("00000006-0006-0006-0006-000000000002"), "Composted food waste", "🌱"),
                new ActionTemplateItem(Guid.Parse("00000006-0006-0006-0006-000000000003"), "Used reusable container", "📦"),
                new ActionTemplateItem(Guid.Parse("00000006-0006-0006-0006-000000000004"), "Refused single-use plastic", "🥤"),
            ],
            _ => []
        };

        ActionTemplates = new ObservableCollection<ActionTemplateItem>(templates);
    }

    [RelayCommand]
    private async Task LogActionAsync(Guid actionTemplateId)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ShowSuccess = false;

            var request = new LogActionRequest
            {
                Category = SelectedCategory,
                ActionTemplateId = actionTemplateId,
                IsDetailed = false
            };

            var response = await _apiService.LogActionAsync(request);

            LpAwarded = response.LPAwarded;
            Co2eSaved = response.CO2eSaved;
            ShowSuccess = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to log action: {ex.Message}";
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
}

public record ActionTemplateItem(Guid Id, string Name, string Icon);
