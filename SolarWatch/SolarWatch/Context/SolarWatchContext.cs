using Microsoft.EntityFrameworkCore;
using SolarWatch.Model.DbModel;

namespace SolarWatch.Context;

public class SolarWatchContext(DbContextOptions<SolarWatchContext> options) : DbContext(options)
{
    public DbSet<City> Cities { get; set; }
    public DbSet<SolarData> SolarTimes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SolarData>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd();

        base.OnModelCreating(modelBuilder);
    }
}