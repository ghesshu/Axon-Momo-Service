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
        // var random = new Random();

        // Fixed list of 10 Ghanaian phone numbers
        string[] fixedPhoneNumbers = 
        {
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


        // string[] ghanaPrefixes = { "020", "023", "024", "026", "027", "028", "029", "050", "054", "055", "056", "057", "059" };

        // // Generate 40 random users to total 50
        // for (int i = 0; i < 40; i++)
        // {
        //     string phoneNumber;
        //     do
        //     {
        //         // Pick a random prefix
        //         string prefix = ghanaPrefixes[random.Next(ghanaPrefixes.Length)];
        //         // Generate 7 random digits
        //         string digits = random.Next(0, 9999999).ToString("D7");
        //         // Combine prefix and digits
        //         phoneNumber = prefix + digits;
        //     } while (fixedPhoneNumbers.Contains(phoneNumber)); // Avoid duplicates

        //     users.Add(new User { Tel = phoneNumber });
        // }

        // Save all users to database
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}