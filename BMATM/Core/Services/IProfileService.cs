namespace BMATM.Services;
public interface IProfileService
{
    Task InitializeAsync();
    Task<SupervisorProfile> GetProfileByUsernameAsync(string username);
    Task<bool> CreateProfileAsync(SupervisorProfile profile);
    Task<bool> UpdateProfileAsync(SupervisorProfile profile);
    Task<List<SupervisorProfile>> GetAllProfilesAsync();
}