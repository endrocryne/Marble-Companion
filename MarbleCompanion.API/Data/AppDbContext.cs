using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CarbonAction> CarbonActions => Set<CarbonAction>();
    public DbSet<EmissionFactor> EmissionFactors => Set<EmissionFactor>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitCheckin> HabitCheckins => Set<HabitCheckin>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<TreeCosmetic> TreeCosmetics => Set<TreeCosmetic>();
    public DbSet<UserCosmetic> UserCosmetics => Set<UserCosmetic>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<FeedEvent> FeedEvents => Set<FeedEvent>();
    public DbSet<FeedReaction> FeedReactions => Set<FeedReaction>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeParticipant> ChallengeParticipants => Set<ChallengeParticipant>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<UserContentBookmark> UserContentBookmarks => Set<UserContentBookmark>();
    public DbSet<UserContentRead> UserContentReads => Set<UserContentRead>();
    public DbSet<OffsetTransaction> OffsetTransactions => Set<OffsetTransaction>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ──────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.TreeSpecies).HasConversion<string>();
        });

        // ── CarbonAction ────────────────────────────────────────────
        builder.Entity<CarbonAction>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Category).HasConversion<string>();
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.LoggedAt);

            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.EmissionFactor)
             .WithMany()
             .HasForeignKey(a => a.EmissionFactorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── EmissionFactor ──────────────────────────────────────────
        builder.Entity<EmissionFactor>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => new { f.Category, f.ActionKey });
        });

        // ── Habit ───────────────────────────────────────────────────
        builder.Entity<Habit>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Category).HasConversion<string>();
            e.Property(h => h.Frequency).HasConversion<string>();
            e.HasIndex(h => h.UserId);

            e.HasOne(h => h.User)
             .WithMany()
             .HasForeignKey(h => h.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── HabitCheckin ────────────────────────────────────────────
        builder.Entity<HabitCheckin>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => new { c.HabitId, c.CheckedInAt });

            e.HasOne(c => c.Habit)
             .WithMany(h => h.Checkins)
             .HasForeignKey(c => c.HabitId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.User)
             .WithMany()
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Achievement ─────────────────────────────────────────────
        builder.Entity<Achievement>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Key).IsUnique();
            e.Property(a => a.UnlockSpecies).HasConversion<string>();
        });

        // ── UserAchievement ─────────────────────────────────────────
        builder.Entity<UserAchievement>(e =>
        {
            e.HasKey(ua => ua.Id);
            e.HasIndex(ua => ua.UserId);
            e.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();

            e.HasOne(ua => ua.User)
             .WithMany()
             .HasForeignKey(ua => ua.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ua => ua.Achievement)
             .WithMany()
             .HasForeignKey(ua => ua.AchievementId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TreeCosmetic ────────────────────────────────────────────
        builder.Entity<TreeCosmetic>(e =>
        {
            e.HasKey(tc => tc.Id);
            e.HasIndex(tc => tc.Key).IsUnique();
        });

        // ── UserCosmetic ────────────────────────────────────────────
        builder.Entity<UserCosmetic>(e =>
        {
            e.HasKey(uc => uc.Id);
            e.HasIndex(uc => uc.UserId);
            e.HasIndex(uc => new { uc.UserId, uc.CosmeticId }).IsUnique();

            e.HasOne(uc => uc.User)
             .WithMany()
             .HasForeignKey(uc => uc.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(uc => uc.Cosmetic)
             .WithMany()
             .HasForeignKey(uc => uc.CosmeticId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Friend ──────────────────────────────────────────────────
        builder.Entity<Friend>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Status).HasConversion<string>();
            e.HasIndex(f => f.Status);
            e.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();

            e.HasOne(f => f.Requester)
             .WithMany()
             .HasForeignKey(f => f.RequesterId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.Addressee)
             .WithMany()
             .HasForeignKey(f => f.AddresseeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── FeedEvent ───────────────────────────────────────────────
        builder.Entity<FeedEvent>(e =>
        {
            e.HasKey(fe => fe.Id);
            e.Property(fe => fe.EventType).HasConversion<string>();
            e.HasIndex(fe => fe.UserId);
            e.HasIndex(fe => fe.ExpiresAt);

            e.HasOne(fe => fe.User)
             .WithMany()
             .HasForeignKey(fe => fe.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── FeedReaction ────────────────────────────────────────────
        builder.Entity<FeedReaction>(e =>
        {
            e.HasKey(fr => fr.Id);
            e.Property(fr => fr.ReactionType).HasConversion<string>();
            e.HasIndex(fr => new { fr.FeedEventId, fr.UserId }).IsUnique();

            e.HasOne(fr => fr.FeedEvent)
             .WithMany(fe => fe.Reactions)
             .HasForeignKey(fr => fr.FeedEventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(fr => fr.User)
             .WithMany()
             .HasForeignKey(fr => fr.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Challenge ───────────────────────────────────────────────
        builder.Entity<Challenge>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Type).HasConversion<string>();
            e.Property(c => c.Difficulty).HasConversion<string>();
            e.Property(c => c.Category).HasConversion<string>();

            e.HasOne(c => c.Creator)
             .WithMany()
             .HasForeignKey(c => c.CreatorUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ChallengeParticipant ────────────────────────────────────
        builder.Entity<ChallengeParticipant>(e =>
        {
            e.HasKey(cp => cp.Id);
            e.HasIndex(cp => cp.UserId);
            e.HasIndex(cp => new { cp.ChallengeId, cp.UserId }).IsUnique();

            e.HasOne(cp => cp.Challenge)
             .WithMany(c => c.Participants)
             .HasForeignKey(cp => cp.ChallengeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cp => cp.User)
             .WithMany()
             .HasForeignKey(cp => cp.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Content ─────────────────────────────────────────────────
        builder.Entity<Content>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Difficulty).HasConversion<string>();
        });

        // ── UserContentBookmark ─────────────────────────────────────
        builder.Entity<UserContentBookmark>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.UserId);
            e.HasIndex(b => new { b.UserId, b.ContentId }).IsUnique();

            e.HasOne(b => b.User)
             .WithMany()
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Content)
             .WithMany()
             .HasForeignKey(b => b.ContentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserContentRead ─────────────────────────────────────────
        builder.Entity<UserContentRead>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => new { r.UserId, r.ContentId }).IsUnique();

            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Content)
             .WithMany()
             .HasForeignKey(r => r.ContentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── OffsetTransaction ───────────────────────────────────────
        builder.Entity<OffsetTransaction>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Tier).HasConversion<string>();
            e.HasIndex(o => o.UserId);

            e.HasOne(o => o.User)
             .WithMany()
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── NotificationPreference ──────────────────────────────────
        builder.Entity<NotificationPreference>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.UserId).IsUnique();

            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── DeviceToken ─────────────────────────────────────────────
        builder.Entity<DeviceToken>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => d.UserId);
            e.HasIndex(d => d.Token).IsUnique();

            e.HasOne(d => d.User)
             .WithMany()
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RefreshToken ────────────────────────────────────────────
        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.UserId);
            e.HasIndex(r => r.Token).IsUnique();

            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
