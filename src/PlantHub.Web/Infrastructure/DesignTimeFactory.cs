using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantHub.Web.Infrastructure
{
    // Provides a stable way for 'dotnet ef' to create the DbContext at design-time
    // without running the web host or relying on environment variables.
    public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PlantHubDbContext>
    {
        public PlantHubDbContext CreateDbContext(string[] args)
        {
            // Keep migrations simple and predictable: always use a local dev DB file.
            // This path is only for design-time commands (add/update migrations).
            var builder = new DbContextOptionsBuilder<PlantHubDbContext>()
                .UseSqlite("Data Source=planthub.dev.db");

            return new PlantHubDbContext(builder.Options);
        }
    }
}
