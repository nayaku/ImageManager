using Microsoft.EntityFrameworkCore;
using ImageManager.Data.Model;
using HandyControl.Tools.Extension;

namespace ImageManager.Data
{
    public class ImageContext : DbContext
    {
        public ImageContext()
        {
        }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Label> Labels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Picture>()
                .HasMany(p => p.Labels)
                .WithMany();
            modelBuilder.Entity<Picture>()
                .Property(p => p.AddTime)
                .HasDefaultValueSql("datetime('now','localtime')");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite("Data Source=Image.db");
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var pictureLabelRefCount = new Dictionary<Label, int>();
            ChangeTracker.Entries<Dictionary<String, Object>>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted)
                .ForEach(e =>
                {
                    var labelId = (int)e.CurrentValues["LabelsId"];
                    var label = ChangeTracker.Entries<Label>().Single(le => (int)le.CurrentValues["Id"] == labelId).Entity;
                    label.Num += e.State == EntityState.Added ? 1 : -1;
                });
            if (UserSettingData.Default.ClearUnUsedLabel)
                ChangeTracker.Entries<Label>()
                    .Where(e => e.State == EntityState.Modified && (int)e.CurrentValues["Num"] == 0)
                    .ForEach(e => e.State = EntityState.Deleted);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }
}
