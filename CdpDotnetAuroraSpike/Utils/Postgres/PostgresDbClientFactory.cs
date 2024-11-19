using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace CdpDotnetAuroraSpike.Utils.Postgres;

[ExcludeFromCodeCoverage]

public class PostgresDbClientFactory(string? connectionString) : IPostgresDbClientFactory
{
    private readonly PostgresContext _postgresDatabase = new(connectionString);

    public PostgresContext getContext()
    {
        return _postgresDatabase;
    }
    
    public class Foo
    {
        public int Id { get; set; }
        public string Bar { get; set; } = "Baz";
    }

    public class PostgresContext(string? connectionString) : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Console.WriteLine("Connection: " + connectionString);
            optionsBuilder.UseMySQL(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            base.OnConfiguring(optionsBuilder);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure default schema
            modelBuilder.Entity<Foo>().ToTable("foos");
            modelBuilder.HasDefaultSchema("default");
        }
        public DbSet<Foo> Foos { get; set; }
    }
}
