using MarbleCompanion.Shared.Constants;

namespace MarbleCompanion.Tests;

public class AppConstantsTests
{
    [Fact]
    public void MaxActiveHabits_Is10()
    {
        Assert.Equal(10, AppConstants.MaxActiveHabits);
    }

    [Fact]
    public void FeedExpiryDays_Is7()
    {
        Assert.Equal(7, AppConstants.FeedExpiryDays);
    }

    [Fact]
    public void StreakFreezeIntervalDays_Is14()
    {
        Assert.Equal(14, AppConstants.StreakFreezeIntervalDays);
    }

    [Fact]
    public void PrimaryColor_IsDeepTeal()
    {
        Assert.Equal("#0D7377", AppConstants.PrimaryColor);
    }

    [Fact]
    public void BackgroundLight_IsGreenTinted()
    {
        Assert.Equal("#F4F9F4", AppConstants.BackgroundLight);
    }
}
