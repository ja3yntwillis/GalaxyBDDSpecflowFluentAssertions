using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Galaxy.Models.DBModels;
using DBOperations.Galaxy.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataGeneration.Galaxy.Models
{
    public partial class DevEHGalaxyContext : DbContext
    {
        public DevEHGalaxyContext()
        {

        }
        public DevEHGalaxyContext(DbContextOptions<DevEHGalaxyContext> options)
            : base(options)
        {
        }

        //public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        //{
        //    builder
        //        .AddFilter((category, level) =>
        //            category == DbLoggerCategory.Database.Command.Name
        //            && level == LogLevel.Error)
        //        .AddConsole();
        //});
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignSegment> Campaignsegments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=devsqlag.extendhealth.com;Database=DevEHGalaxy;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Partner>(entity =>
            //{
            //    entity.Property(e => e.CourseName)
            //        .HasMaxLength(50)
            //        .IsUnicode(false);

            //    entity.HasOne(d => d.Teacher)
            //        .WithMany(p => p.Course)
            //        .HasForeignKey(d => d.TeacherId)
            //        .OnDelete(DeleteBehavior.Cascade)
            //        .HasConstraintName("FK_Course_Teacher");
            //});
            // Map entity type with DB table
            #region Partner Table
            modelBuilder.Entity<Partner>().ToTable("PARTNER");

            // Set the primary key
            modelBuilder.Entity<Partner>().HasKey(pc => new { pc.PARTNERID});
            #endregion
            #region Campaign Table
            modelBuilder.Entity<Campaign>().ToTable("CAMPAIGN");

            // Set the primary key
            modelBuilder.Entity<Campaign>().HasKey(pc => new { pc.campaignId });
            #endregion
            #region CampaignSegemnt Table
            modelBuilder.Entity<CampaignSegment>().ToTable("CAMPAIGNSEGMENT");

            // Set the primary key
            modelBuilder.Entity<CampaignSegment>().HasKey(pc => new { pc.campaignSegmentId });
            #endregion
        }

    }
}
