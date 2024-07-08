using Capstone_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Capstone_Backend.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "College" },
            new Category { Id = 2, Name = "Rent" },
            new Category { Id = 3, Name = "Sale" },
            new Category { Id = 4, Name = "Beds" },
            new Category { Id = 5, Name = "Cabin" },
            new Category { Id = 6, Name = "Studio" },
            new Category { Id = 7, Name = "Mansion" },
            new Category { Id = 8, Name = "Pool" }
         );

            modelBuilder.Entity<AppartmentWishList>()
               .HasKey(aw => new { aw.AppartmentId, aw.WishListId });

            modelBuilder.Entity<AppartmentWishList>()
                .HasOne(aw => aw.Appartment)
                .WithMany(a => a.Wishlist)
                .HasForeignKey(aw => aw.AppartmentId);

            modelBuilder.Entity<AppartmentWishList>()
                .HasOne(aw => aw.WishList)
                .WithMany(w => w.appartments)
                .HasForeignKey(aw => aw.WishListId);
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Appartments)
                .WithOne(a => a.Owner)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.Reviewer)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Feedbacks)
                .WithOne(f => f.Writer)
                .HasForeignKey(f => f.WriterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Reservations)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.requests)
                .WithOne(r => r.user)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.responses)
                .WithOne(r => r.user)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.list)
                .WithOne(w => w.user)
                .HasForeignKey<WishList>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appartment>()
                .HasOne(a => a.Owner)
                .WithMany(u => u.Appartments)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appartment>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Appartments)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appartment>()
                .HasOne(a => a.Location)
                .WithMany(l => l.Appartments)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appartment>()
                .HasMany(a => a.Reviews)
                .WithOne(r => r.Appartment)
                .HasForeignKey(r => r.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Appartment>()
                .HasMany(a => a.Reservations)
                .WithOne(r => r.Appartment)
                .HasForeignKey(r => r.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);

   

            modelBuilder.Entity<Appartment>()
                .HasMany(a => a.requests)
                .WithOne(r => r.appartment)
                .HasForeignKey(r => r.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appartment>()
              .HasMany(a => a.images) // An apartment can have many images
              .WithOne(i => i.Appartment) // Each image belongs to one apartment
              .HasForeignKey(i => i.AppartmentId) // Foreign key property in Image entity
              .OnDelete(DeleteBehavior.Restrict); // Define delete behavior if needed



            modelBuilder.Entity<Category>()
                .HasMany(c => c.Appartments)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Writer)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.WriterId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Location>()
                .HasMany(l => l.Appartments)
                .WithOne(a => a.Location)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Appartment)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.responses)
                .WithMany()
                .HasForeignKey(r => r.ResponsesId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Appartment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Responses>()
                .HasOne(res => res.user)
                .WithMany(u => u.responses)
                .HasForeignKey(res => res.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Responses>()
                .HasOne(res => res.reservation)
                .WithOne(r => r.responses)
                .HasForeignKey<Responses>(res => res.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Responses>()
                .HasOne(res => res.requests)
                .WithMany()
                .HasForeignKey(res => res.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Requests>()
                .HasOne(req => req.user)
                .WithMany(u => u.requests)
                .HasForeignKey(req => req.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Requests>()
                .HasOne(req => req.appartment)
                .WithMany(a => a.requests)
                .HasForeignKey(req => req.AppartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Requests>()
                .HasOne(req => req.responses)
                .WithOne(res => res.requests) // Specify the navigation property in Responses
                .HasForeignKey<Responses>(res => res.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WishList>()
                .HasOne(wl => wl.user)
                .WithOne(u => u.list)
                .HasForeignKey<WishList>(wl => wl.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Images>()
               .HasOne(i => i.Appartment) // Each image belongs to one apartment
               .WithMany(a => a.images) // An apartment can have many images
               .HasForeignKey(i => i.AppartmentId) // Foreign key property in Image entity
               .OnDelete(DeleteBehavior.Cascade); //
        }
        public DbSet<Appartment> Appartments { get; set; }
        public DbSet<AppUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Requests> Requests { get; set; }
        public DbSet<Responses> Responses { get; set; }

        public DbSet<WishList> WishList { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<AppartmentWishList> AppartmentWishLists { get; set; }

    }
}
