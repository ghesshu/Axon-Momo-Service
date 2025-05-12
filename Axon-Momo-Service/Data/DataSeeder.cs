using Axon_Momo_Service.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Data;

public static class DataSeeder
{
    public static async Task SeedData(DataContext context)
    {
        // Check if database already has users
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var users = new List<User>();
        
        string[] fixedPhoneNumbers = 
        {
            "0123456789",
            "0241234567",
            "0559876543",
            "0274567890",
            "0591239876",
            "0267891234",
            "0506543210",
            "0573216547",
            "0298765432",
            "0547894561",
            "0209871234"
        };

        // Seed fixed users
        foreach (var phoneNumber in fixedPhoneNumbers)
        {
            users.Add(new User { Tel = phoneNumber });
        }

        // Save all users to database
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}