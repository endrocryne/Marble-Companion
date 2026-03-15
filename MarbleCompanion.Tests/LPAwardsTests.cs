using MarbleCompanion.Shared.Constants;

namespace MarbleCompanion.Tests;

public class LPAwardsTests
{
    [Fact]
    public void QuickLog_Returns5()
    {
        Assert.Equal(5, LPAwards.QuickLog);
    }

    [Fact]
    public void DetailedLog_Returns10()
    {
        Assert.Equal(10, LPAwards.DetailedLog);
    }

    [Fact]
    public void HabitCheckin_Returns8()
    {
        Assert.Equal(8, LPAwards.HabitCheckin);
    }

    [Fact]
    public void ArticleRead_Returns3()
    {
        Assert.Equal(3, LPAwards.ArticleRead);
    }

    [Theory]
    [InlineData(3, 50)]
    [InlineData(7, 100)]
    [InlineData(14, 150)]
    [InlineData(30, 250)]
    [InlineData(60, 350)]
    [InlineData(100, 500)]
    [InlineData(365, 500)]
    public void GetStreakMilestoneLP_ReturnsCorrectValue(int days, int expectedLP)
    {
        var lp = LPAwards.GetStreakMilestoneLP(days);
        Assert.Equal(expectedLP, lp);
    }

    [Fact]
    public void GetStreakMilestoneLP_NonMilestone_ReturnsZero()
    {
        Assert.Equal(0, LPAwards.GetStreakMilestoneLP(5));
        Assert.Equal(0, LPAwards.GetStreakMilestoneLP(15));
    }
}
