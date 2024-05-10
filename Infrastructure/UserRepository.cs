using Domain.Identities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<List<ApplicationUser>> GetListAsync()
    {
        return dbContext.Users.ToListAsync();
    }
}