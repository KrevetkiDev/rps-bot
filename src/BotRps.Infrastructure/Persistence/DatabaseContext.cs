using BotRpc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotRps.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> option) : base(option)
    {
    }
}