using System;
using Axon_Momo_Service.Features.Auth;
using Microsoft.EntityFrameworkCore;


namespace Axon_Momo_Service.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuthToken> AuthTokens => Set<AuthToken>();
}
