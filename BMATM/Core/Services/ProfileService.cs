namespace BMATM.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IATMRepository _atmRepository;

    public ProfileService(IProfileRepository profileRepository, IATMRepository atmRepository)
    {
        _profileRepository = profileRepository;
        _atmRepository = atmRepository;
    }

    public async Task InitializeAsync()
    {
        await _profileRepository.InitializeAsync().ConfigureAwait(false);
    }

    public async Task<SupervisorProfile> GetProfileByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var profile = await _profileRepository.GetByUsernameAsync(username).ConfigureAwait(false);
        if (profile != null)
        {
            // Load ATMs for this supervisor's branch
            var atms = await _atmRepository.GetByBranchCodeAsync(profile.User.BranchNumber).ConfigureAwait(false);
            profile.ATMs = new ObservableCollection<ATMInfo>(atms);
        }
        return profile;
    }

    public async Task<bool> CreateProfileAsync(SupervisorProfile profile)
    {
        if (profile?.User == null)
            return false;

        return await _profileRepository.CreateProfileAsync(profile).ConfigureAwait(false);
    }

    public async Task<bool> UpdateProfileAsync(SupervisorProfile profile)
    {
        if (profile?.User == null)
            return false;

        return await _profileRepository.UpdateProfileAsync(profile).ConfigureAwait(false);
    }

    public async Task<List<SupervisorProfile>> GetAllProfilesAsync()
    {
        return await _profileRepository.GetAllProfilesAsync().ConfigureAwait(false);
    }
}