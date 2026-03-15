using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class ChallengeService : IChallengeService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITreeService _treeService;

    public ChallengeService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ITreeService treeService)
    {
        _db = db;
        _userManager = userManager;
        _treeService = treeService;
    }

    public async Task<List<ChallengeDto>> GetCuratedAsync()
    {
        var challenges = await _db.Challenges
            .Include(c => c.Participants)
            .Where(c => c.IsCurated && c.EndsAt > DateTime.UtcNow)
            .OrderBy(c => c.StartsAt)
            .ToListAsync();

        return challenges.Select(MapToDto).ToList();
    }

    public async Task<List<ChallengeDto>> GetUserChallengesAsync(string userId)
    {
        var challenges = await _db.ChallengeParticipants
            .Include(cp => cp.Challenge)
                .ThenInclude(c => c.Participants)
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.Challenge)
            .ToListAsync();

        return challenges.Select(MapToDto).ToList();
    }

    public async Task<ChallengeDto> CreateChallengeAsync(string userId, CreateChallengeDto dto)
    {
        var challenge = new Challenge
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Difficulty = dto.Difficulty,
            Category = dto.Category,
            MetricKey = dto.Category.ToString().ToLowerInvariant(),
            GoalValue = dto.TargetValue,
            DurationDays = dto.DurationDays,
            LeafPointsReward = dto.Difficulty switch
            {
                ChallengeDifficulty.Easy => LPAwards.ChallengeEasy,
                ChallengeDifficulty.Medium => LPAwards.ChallengeMedium,
                ChallengeDifficulty.Hard => LPAwards.ChallengeHard,
                _ => LPAwards.ChallengeEasy
            },
            IsCurated = false,
            CreatorUserId = userId,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(dto.DurationDays)
        };

        _db.Challenges.Add(challenge);

        // Auto-join the creator
        _db.ChallengeParticipants.Add(new ChallengeParticipant
        {
            ChallengeId = challenge.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return MapToDto(challenge);
    }

    public async Task<ChallengeDto> GetChallengeAsync(Guid id)
    {
        var challenge = await _db.Challenges
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Challenge not found.");

        return MapToDto(challenge);
    }

    public async Task<ChallengeParticipantDto> JoinChallengeAsync(string userId, Guid challengeId)
    {
        var challenge = await _db.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId)
            ?? throw new KeyNotFoundException("Challenge not found.");

        if (challenge.EndsAt < DateTime.UtcNow)
            throw new InvalidOperationException("Challenge has already ended.");

        var existing = await _db.ChallengeParticipants
            .AnyAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);
        if (existing)
            throw new InvalidOperationException("Already participating in this challenge.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var participant = new ChallengeParticipant
        {
            ChallengeId = challengeId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _db.ChallengeParticipants.Add(participant);
        await _db.SaveChangesAsync();

        return new ChallengeParticipantDto
        {
            UserId = Guid.Parse(userId),
            DisplayName = user.DisplayName,
            AvatarIndex = user.AvatarIndex,
            CurrentValue = 0,
            IsCompleted = false,
            Rank = 0
        };
    }

    public async Task<ChallengeProgressDto> UpdateProgressAsync(string userId, Guid challengeId, decimal progress)
    {
        var participant = await _db.ChallengeParticipants
            .Include(cp => cp.Challenge)
            .FirstOrDefaultAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId)
            ?? throw new KeyNotFoundException("Not participating in this challenge.");

        participant.CurrentProgress = progress;

        bool justCompleted = false;
        if (progress >= participant.Challenge.GoalValue && participant.CompletedAt == null)
        {
            participant.CompletedAt = DateTime.UtcNow;
            justCompleted = true;
        }

        await _db.SaveChangesAsync();

        if (justCompleted)
        {
            await _treeService.AwardLeafPointsAsync(userId, participant.Challenge.LeafPointsReward);

            _db.FeedEvents.Add(new FeedEvent
            {
                UserId = userId,
                EventType = FeedEventType.ChallengeCompletion,
                Title = "Challenge Completed!",
                Description = $"Completed the \"{participant.Challenge.Title}\" challenge!",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.FeedExpiryDays)
            });
            await _db.SaveChangesAsync();
        }

        double percentComplete = participant.Challenge.GoalValue > 0
            ? Math.Min(100.0, (double)(progress / participant.Challenge.GoalValue) * 100)
            : 0;

        return new ChallengeProgressDto
        {
            ChallengeId = challengeId,
            CurrentValue = (int)progress,
            TargetValue = (int)participant.Challenge.GoalValue,
            PercentComplete = percentComplete,
            IsCompleted = participant.CompletedAt != null,
            CompletedAt = participant.CompletedAt
        };
    }

    public async Task DeleteChallengeAsync(string userId, Guid challengeId)
    {
        var challenge = await _db.Challenges
            .FirstOrDefaultAsync(c => c.Id == challengeId && c.CreatorUserId == userId)
            ?? throw new KeyNotFoundException("Challenge not found or you are not the creator.");

        _db.Challenges.Remove(challenge);
        await _db.SaveChangesAsync();
    }

    private static ChallengeDto MapToDto(Challenge c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Description = c.Description,
        Type = c.Type,
        Difficulty = c.Difficulty,
        Category = c.Category ?? ActionCategory.Transport,
        TargetValue = (int)c.GoalValue,
        LPReward = c.LeafPointsReward,
        StartsAt = c.StartsAt,
        EndsAt = c.EndsAt,
        ParticipantCount = c.Participants.Count
    };
}
