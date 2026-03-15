using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Tests;

public class TreeGrowthConstantsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(49, 0)]
    [InlineData(50, 1)]
    [InlineData(149, 1)]
    [InlineData(150, 2)]
    [InlineData(350, 3)]
    [InlineData(700, 4)]
    [InlineData(1200, 5)]
    [InlineData(2000, 6)]
    [InlineData(3200, 7)]
    [InlineData(5000, 8)]
    [InlineData(7500, 9)]
    [InlineData(11000, 10)]
    [InlineData(16000, 11)]
    [InlineData(99999, 11)]
    public void GetStageForLP_ReturnsCorrectStage(int lp, int expectedStage)
    {
        var stage = TreeGrowthConstants.GetStageForLP(lp);
        Assert.Equal(expectedStage, stage);
    }

    [Theory]
    [InlineData(100, TreeHealthState.Healthy)]
    [InlineData(80, TreeHealthState.Healthy)]
    [InlineData(79, TreeHealthState.Stressed)]
    [InlineData(50, TreeHealthState.Stressed)]
    [InlineData(49, TreeHealthState.Withering)]
    [InlineData(20, TreeHealthState.Withering)]
    [InlineData(19, TreeHealthState.Dormant)]
    [InlineData(0, TreeHealthState.Dormant)]
    public void GetHealthState_ReturnsCorrectState(int score, TreeHealthState expected)
    {
        var state = TreeGrowthConstants.GetHealthState(score);
        Assert.Equal(expected, state);
    }

    [Fact]
    public void StageThresholds_HasCorrectCount()
    {
        Assert.Equal(12, TreeGrowthConstants.StageThresholds.Length);
    }

    [Fact]
    public void StageNames_HasCorrectCount()
    {
        Assert.Equal(12, TreeGrowthConstants.StageNames.Length);
    }

    [Fact]
    public void StageThresholds_AreAscending()
    {
        for (int i = 1; i < TreeGrowthConstants.StageThresholds.Length; i++)
        {
            Assert.True(TreeGrowthConstants.StageThresholds[i] > TreeGrowthConstants.StageThresholds[i - 1]);
        }
    }
}
