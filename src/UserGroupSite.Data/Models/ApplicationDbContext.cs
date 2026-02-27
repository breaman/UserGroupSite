using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Interfaces;

namespace UserGroupSite.Data.Models;

public class ApplicationDbContext : AuthDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserService userService) :
        base(options, userService)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSpeaker> EventSpeakers => Set<EventSpeaker>();
    public DbSet<TopicSuggestion> TopicSuggestions => Set<TopicSuggestion>();
    public DbSet<TopicSuggestionLike> TopicSuggestionLikes => Set<TopicSuggestionLike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventSpeaker>()
            .HasKey(eventSpeaker => new { eventSpeaker.EventId, eventSpeaker.SpeakerId });

        modelBuilder.Entity<EventSpeaker>()
            .HasOne(eventSpeaker => eventSpeaker.Event)
            .WithMany(eventEntity => eventEntity.Speakers)
            .HasForeignKey(eventSpeaker => eventSpeaker.EventId);

        modelBuilder.Entity<EventSpeaker>()
            .HasOne(eventSpeaker => eventSpeaker.Speaker)
            .WithMany()
            .HasForeignKey(eventSpeaker => eventSpeaker.SpeakerId);

        modelBuilder.Entity<TopicSuggestionLike>()
            .HasKey(like => new { like.TopicSuggestionId, like.UserId });

        modelBuilder.Entity<TopicSuggestionLike>()
            .HasOne(like => like.TopicSuggestion)
            .WithMany(topic => topic.Likes)
            .HasForeignKey(like => like.TopicSuggestionId);

        modelBuilder.Entity<TopicSuggestionLike>()
            .HasOne(like => like.User)
            .WithMany()
            .HasForeignKey(like => like.UserId);

        modelBuilder.Entity<TopicSuggestion>()
            .HasOne(topic => topic.VolunteerSpeaker)
            .WithMany()
            .HasForeignKey(topic => topic.VolunteerSpeakerId)
            .IsRequired(false);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "6d2a7190-8c7a-4f5a-9d5c-9f2fd82ac94e"
            },
            new Role
            {
                Id = 2,
                Name = "Speaker",
                NormalizedName = "SPEAKER",
                ConcurrencyStamp = "0a0d2ef9-245b-4dbf-85d7-2d3c92f3c7a0"
            });
    }
}