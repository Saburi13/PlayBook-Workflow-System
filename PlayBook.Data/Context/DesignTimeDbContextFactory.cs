using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PlayBook.Data.Context;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PlayBookDbContext>
{
    public PlayBookDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "PlayBook.API"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=PlayBookDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder =
            new DbContextOptionsBuilder<PlayBookDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new PlayBookDbContext(optionsBuilder.Options);
    }
}