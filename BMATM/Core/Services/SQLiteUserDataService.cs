namespace BMATM.Services;

// Updated User Data Service using Repository
public class SQLiteUserDataService : IUserDataService
{
    private readonly IUserRepository _userRepository;
    private readonly ISupervisorProfileRepository _profileRepository;
    private readonly DatabaseContext _context;

    public SQLiteUserDataService(IUserRepository userRepository, ISupervisorProfileRepository profileRepository, DatabaseContext context)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _context = context;
    }

    public async Task<SupervisorProfile?> GetSupervisorProfileAsync(string username)
    {
        try
        {
            return await _profileRepository.GetByUsernameAsync(username);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SaveSupervisorProfileAsync(SupervisorProfile profile)
    {
        try
        {
            if (await _profileRepository.ExistsByUsernameAsync(profile.User?.Username ?? string.Empty))
            {
                // Update existing profile
                var result = await _profileRepository.UpdateAsync(profile);
                return result != null;
            }
            else
            {
                // Add new profile
                var result = await _profileRepository.AddAsync(profile);
                return result != null;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateSupervisorProfileAsync(SupervisorProfile profile)
    {
        return await SaveSupervisorProfileAsync(profile);
    }

    public async Task<bool> DeleteSupervisorProfileAsync(string username)
    {
        try
        {
            return await _profileRepository.DeleteByUsernameAsync(username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SupervisorProfile>> GetAllSupervisorProfilesAsync()
    {
        try
        {
            var profiles = await _profileRepository.GetAllProfilesWithUserDataAsync();
            return profiles.ToList();
        }
        catch
        {
            return new List<SupervisorProfile>();
        }
    }

    public async Task<bool> ProfileExistsAsync(string username)
    {
        try
        {
            return await _profileRepository.ExistsByUsernameAsync(username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<DateTime?> GetLastLoginDateAsync(string username)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user?.LastLogin;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginDateAsync(string username, DateTime loginDate)
    {
        try
        {
            return await _userRepository.UpdateLastLoginAsync(username, loginDate);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ClearAllDataAsync()
    {
        try
        {
            // This would need to be implemented based on requirements
            // Could truncate tables or delete specific data
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Additional user management methods
    public async Task<User?> GetUserAsync(string username)
    {
        try
        {
            return await _userRepository.GetByUsernameAsync(username);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> AddUserAsync(User user)
    {
        try
        {
            if (await _userRepository.ExistsByUsernameAsync(user.Username))
            {
                return false; // User already exists
            }

            var result = await _userRepository.AddAsync(user);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            var result = await _userRepository.UpdateAsync(user);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(string username)
    {
        try
        {
            return await _userRepository.DeactivateUserAsync(username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActivateUserAsync(string username)
    {
        try
        {
            return await _userRepository.ActivateUserAsync(username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<User>> GetSupervisorsAsync()
    {
        try
        {
            var users = await _userRepository.GetByRoleAsync(UserRole.Supervisor);
            return users.ToList();
        }
        catch
        {
            return new List<User>();
        }
    }

    public async Task<List<User>> GetUsersByDepartmentAsync(string department)
    {
        try
        {
            var users = await _userRepository.GetByDepartmentAsync(department);
            return users.ToList();
        }
        catch
        {
            return new List<User>();
        }
    }
}
