using Domain.Identities;

namespace Application;

public class UserService(IUserRepository userRepository)
{
    public Task<List<ApplicationUser>> GetListAsync() => userRepository.GetListAsync();
}