using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using EventPlanner.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EventPlanner.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public ICollection<Event> CreatedEvents { get; set; }
        public ICollection<UserEvent> EventUsersSigned { get; set; }

        public ApplicationUser()
        {
            this.EventUsersSigned = new List<UserEvent>();
            this.CreatedEvents = new List<Event>();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<UserEvent> UserEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Field> Fields { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Event>()
              .HasRequired<ApplicationUser>(s => s.UserCreator)
              .WithMany(g => g.CreatedEvents)
              .WillCascadeOnDelete(false);

            builder.Entity<UserEvent>()
               .HasKey(t => new
               {
                   t.EventId,
                   t.UserId
               });

            builder.Entity<UserEvent>()
                .HasRequired(sc => sc.Event)
                .WithMany(s => s.EventUsersSigned)
                .HasForeignKey(sc => sc.EventId);

            builder.Entity<UserEvent>()
                .HasRequired(sc => sc.User)
                .WithMany(c => c.EventUsersSigned)
                .HasForeignKey(sc => sc.UserId);
        }
    }
}