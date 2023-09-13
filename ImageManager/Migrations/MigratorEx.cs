using ImageManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImageManager.Migrations
{
    public static class MigratorEx
    {
        public static void MigrateEx(this IMigrator migrator, string targetMigration, ImageContext context)
        {
            var migrationsAssembly = context.GetService<IMigrationsAssembly>();
            var activeProvider = context.GetService<IDatabaseProvider>().Name;
            var migrationType = migrationsAssembly.Migrations
                .Where(m => string.Compare(m.Key, targetMigration, StringComparison.OrdinalIgnoreCase) == 0)
                .SingleOrDefault().Value ?? throw new Exception($"Migration {targetMigration} not found.");
            var migration = migrationsAssembly.CreateMigration(migrationType, activeProvider);
            if (migration is MigrationCustom<ImageContext> migrationCustom)
                migrationCustom.PreUp(context);
            migrator.Migrate(targetMigration);
            if (migration is MigrationCustom<ImageContext> migrationCustom2)
                migrationCustom2.PostUp(context);
        }
    }
}
