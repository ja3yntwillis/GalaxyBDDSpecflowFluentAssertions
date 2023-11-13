using FundingCacheClient = DataGeneration.Galaxy.Models.FundingCacheClientDBModels;
using Dbo = DataGeneration.Galaxy.Models.DboDBModels;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<FundingCacheClient.Partner> Partners { get; set; }
        public DbSet<Dbo.Partner> DboPartners { get; set; }
        public DbSet<FundingCacheClient.Campaign> Campaigns { get; set; }
        public DbSet<FundingCacheClient.CampaignSegment> Campaignsegments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=qasqlag.extendhealth.com;Database=QAExtendHealth;Trusted_Connection=True;");
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
            #region FundingCache Partner Table
            modelBuilder.Entity<FundingCacheClient.Partner>().ToTable("Partner", "FundingCache_Client");

            // Set the primary key
            modelBuilder.Entity<FundingCacheClient.Partner>().HasKey(pc => new { pc.PARTNERID });
            #endregion
            #region DBO Partner Table
            modelBuilder.Entity<Dbo.Partner>().ToTable("Partner", "dbo");

            // Set the primary key
            modelBuilder.Entity<Dbo.Partner>().HasKey(pc => new { pc.PARTNERID });
            #endregion
            #region Campaign Table
            modelBuilder.Entity<FundingCacheClient.Campaign>().ToTable("campaign", "FundingCache_Client");

            // Set the primary key
            modelBuilder.Entity<FundingCacheClient.Campaign>().HasKey(pc => new { pc.campaignId });
            #endregion
            #region CampaignSegemnt Table
            modelBuilder.Entity<FundingCacheClient.CampaignSegment>().ToTable("campaignsegment", "FundingCache_Client");

            // Set the primary key
            modelBuilder.Entity<FundingCacheClient.CampaignSegment>().HasKey(pc => new { pc.campaignSegmentId });
            #endregion
        }

    }
}
