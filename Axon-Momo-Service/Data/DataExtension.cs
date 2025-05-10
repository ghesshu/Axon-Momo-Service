using System;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Data;

public static class DataExtension
{
    public static async Task MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        
        // Migrate database
        await dbContext.Database.MigrateAsync();
        
        // Seed data only if database is empty
        if (!await dbContext.Users.AnyAsync())
        {
            await DataSeeder.SeedData(dbContext);
        }
    }
}