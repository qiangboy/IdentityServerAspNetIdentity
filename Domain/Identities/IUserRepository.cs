namespace Domain.Identities;

public interface IUserRepository
{
    Task<List<ApplicationUser>> GetListAsync();
}