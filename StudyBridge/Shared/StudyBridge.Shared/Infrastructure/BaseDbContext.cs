using Microsoft.EntityFrameworkCore;

namespace StudyBridge.Shared.Infrastructure;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Common configurations can go here
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure snake_case naming convention
            entityType.SetTableName(entityType.GetTableName()?.ToSnakeCase());
            
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }
        }
    }
}

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = string.Empty;
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
                result += "_";
            result += char.ToLower(input[i]);
        }
        return result;
    }
}
