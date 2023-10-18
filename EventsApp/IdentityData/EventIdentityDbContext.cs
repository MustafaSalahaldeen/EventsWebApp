using EventsApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Day11Identity.IdentityData
{
    public class EventIdentityDbContext : IdentityDbContext
    {

        public EventIdentityDbContext(DbContextOptions<EventIdentityDbContext> options)
            : base(options)
        {

        }

        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<EventImage> Images { get; set; }
        public DbSet<EventUserRelation> EventUserRelations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EventUserRelation>()
    .HasOne(eur => eur.User)
    .WithMany()
    .HasForeignKey(eur => eur.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventUserRelation>()
                .HasOne(eur => eur.Event)
                .WithMany()
                .HasForeignKey(eur => eur.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<EventsApp.Models.RoleModel>? RoleModel { get; set; }

    }
}
