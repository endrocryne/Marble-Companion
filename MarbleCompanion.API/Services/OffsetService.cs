using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class OffsetService : IOffsetService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    // Catalog of available offset credits
    private static readonly List<OffsetCreditDefinition> CreditCatalog =
    [
        new(Guid.Parse("a0000000-0000-0000-0000-000000000001"), OffsetTier.PlantTree, "Plant a Tree", "Symbolically plant a tree to offset carbon. One tree absorbs ~22 kg CO2/year.", 22.0, (int)OffsetTier.PlantTree),
        new(Guid.Parse("a0000000-0000-0000-0000-000000000002"), OffsetTier.OffsetCarbon, "Carbon Offset Credit", "Fund verified carbon offset projects removing 100 kg CO2e from the atmosphere.", 100.0, (int)OffsetTier.OffsetCarbon),
        new(Guid.Parse("a0000000-0000-0000-0000-000000000003"), OffsetTier.FundWind, "Fund Wind Energy", "Support wind energy generation equivalent to 500 kWh, offsetting ~200 kg CO2e.", 200.0, (int)OffsetTier.FundWind),
    ];

    public OffsetService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Task<List<OffsetCreditDto>> GetCreditsAsync(string userId)
    {
        var credits = CreditCatalog.Select(c => new OffsetCreditDto
        {
            Id = c.Id,
            Tier = c.Tier,
            Name = c.Name,
            Description = c.Description,
            CO2eOffsetKg = c.CO2eOffsetKg,
            LPCost = c.LPCost
        }).ToList();

        return Task.FromResult(credits);
    }

    public async Task<OffsetHistoryDto> RedeemAsync(string userId, RedeemOffsetRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var credit = CreditCatalog.FirstOrDefault(c => c.Id == request.OffsetCreditId)
            ?? throw new KeyNotFoundException("Offset credit not found.");

        if (user.OffsetCredits < credit.LPCost)
            throw new InvalidOperationException("Insufficient offset credits.");

        user.OffsetCredits -= credit.LPCost;
        user.SymbolicCO2eOffset += (decimal)credit.CO2eOffsetKg;

        // Apply tier-specific symbolic effects
        switch (credit.Tier)
        {
            case OffsetTier.PlantTree:
                user.SymbolicTreesPlanted++;
                break;
            case OffsetTier.FundWind:
                user.SymbolicWindHours++;
                break;
        }

        var transaction = new OffsetTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Tier = credit.Tier,
            CreditsSpent = credit.LPCost,
            Description = credit.Name,
            RedeemedAt = DateTime.UtcNow
        };

        _db.OffsetTransactions.Add(transaction);
        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        return new OffsetHistoryDto
        {
            Id = transaction.Id,
            OffsetCreditName = credit.Name,
            Tier = credit.Tier,
            CO2eOffsetKg = credit.CO2eOffsetKg,
            LPSpent = credit.LPCost,
            RedeemedAt = transaction.RedeemedAt
        };
    }

    public async Task<List<OffsetHistoryDto>> GetHistoryAsync(string userId)
    {
        var transactions = await _db.OffsetTransactions
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.RedeemedAt)
            .ToListAsync();

        return transactions.Select(o =>
        {
            var credit = CreditCatalog.FirstOrDefault(c => c.Tier == o.Tier);
            return new OffsetHistoryDto
            {
                Id = o.Id,
                OffsetCreditName = o.Description,
                Tier = o.Tier,
                CO2eOffsetKg = credit?.CO2eOffsetKg ?? 0,
                LPSpent = o.CreditsSpent,
                RedeemedAt = o.RedeemedAt
            };
        }).ToList();
    }

    private sealed record OffsetCreditDefinition(Guid Id, OffsetTier Tier, string Name, string Description, double CO2eOffsetKg, int LPCost);
}
