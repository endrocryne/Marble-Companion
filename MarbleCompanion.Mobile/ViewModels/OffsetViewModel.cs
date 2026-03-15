using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class OffsetViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<OffsetCreditDto> _availableCredits = [];

    [ObservableProperty]
    private ObservableCollection<OffsetHistoryDto> _history = [];

    [ObservableProperty]
    private int _userTotalLP;

    [ObservableProperty]
    private double _totalCO2eOffset;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public OffsetViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var creditsTask = _apiService.GetOffsetCreditsAsync();
            var historyTask = _apiService.GetOffsetHistoryAsync();
            var profileTask = _apiService.GetMyProfileAsync();

            await Task.WhenAll(creditsTask, historyTask, profileTask);

            AvailableCredits = new ObservableCollection<OffsetCreditDto>(creditsTask.Result);
            History = new ObservableCollection<OffsetHistoryDto>(
                historyTask.Result.OrderByDescending(h => h.RedeemedAt));

            UserTotalLP = profileTask.Result.TotalLP;
            TotalCO2eOffset = historyTask.Result.Sum(h => h.CO2eOffsetKg);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load offsets: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RedeemAsync(Guid offsetCreditId)
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            var credit = AvailableCredits.FirstOrDefault(c => c.Id == offsetCreditId);
            if (credit is null)
            {
                ErrorMessage = "Credit not found.";
                return;
            }

            if (credit.LPCost > UserTotalLP)
            {
                ErrorMessage = $"Not enough LP. You need {credit.LPCost} LP but have {UserTotalLP}.";
                return;
            }

            var request = new RedeemOffsetRequest { OffsetCreditId = offsetCreditId };
            var result = await _apiService.RedeemOffsetAsync(request);

            SuccessMessage = $"Redeemed! {result.CO2eOffsetKg:F1} kg CO₂e offset.";

            // Refresh data
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to redeem: {ex.Message}";
        }
    }
}
