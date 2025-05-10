using Axon_Momo_Service.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace Axon_Momo_Service.Data;

public static class DataSeeder
{
    public static async Task SeedData(DataContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var random = new Random();
            var users = new List<User>();

            // Ghana mobile number prefixes (valid as of 2024)
            string[] ghanaPrefixes = { "020", "023", "024", "026", "027", "028", "029", "050", "054", "055", "056", "057", "059" };

            // Generate 50 random users with Ghana phone numbers
            for (int i = 0; i < 50; i++)
            {
                // Select a random prefix
                string prefix = ghanaPrefixes[random.Next(ghanaPrefixes.Length)];
                
                // Generate remaining 7 digits
                string remainingDigits = random.Next(0, 9999999).ToString("D7");
                
                // Combine to form the complete phone number
                string phoneNumber = prefix + remainingDigits;

                var user = new User
                {
                    Tel = phoneNumber
                };

                users.Add(user);
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}