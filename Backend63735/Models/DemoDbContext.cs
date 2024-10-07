using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Backend63735.Models
{
    public class DemoDbContext : IdentityDbContext<IdentityUser>
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Pill> Pills { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<CalendarRecord> CalendarRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CalendarRecordPill>()
        .HasKey(crp => new { crp.CalendarRecordId, crp.PillId });

            builder.Entity<CalendarRecordPill>()
                .HasOne(crp => crp.CalendarRecord)
                .WithMany(cr => cr.CalendarRecordPills)
                .HasForeignKey(crp => crp.CalendarRecordId);

            builder.Entity<CalendarRecordPill>()
                .HasOne(crp => crp.Pill)
                .WithMany(p => p.CalendarRecordPills)
                .HasForeignKey(crp => crp.PillId);


            // SeedRoles(builder);
        }


        // private static void SeedRoles(ModelBuilder builder)
        // {
        //     builder.Entity<IdentityRole>().HasData(
        //         new IdentityRole { Name = "Admin", NormalizedName = "Admin".ToUpper() },
        //         new IdentityRole { Name = "User", NormalizedName = "User".ToUpper() },
        //         new IdentityRole { Name = "HR", NormalizedName = "HR".ToUpper() }
        //     );
        // }

        // Uncomment this method if you want to configure the DB context options
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseMySql("Server=localhost;Database=adhd_w63735;Uid=root;Pwd=Karina682557!", 
        //                            new MySqlServerVersion(new Version(8, 0, 20)));
        // }
    }
}
