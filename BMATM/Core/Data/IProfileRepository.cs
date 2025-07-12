namespace BMATM.Data;

// ==================== PROFILE REPOSITORY ====================
public interface IProfileRepository
{
    Task InitializeAsync();
    Task<SupervisorProfile> GetByUsernameAsync(string username);
    Task<bool> CreateProfileAsync(SupervisorProfile profile);
    Task<bool> UpdateProfileAsync(SupervisorProfile profile);
    Task<List<SupervisorProfile>> GetAllProfilesAsync();
}