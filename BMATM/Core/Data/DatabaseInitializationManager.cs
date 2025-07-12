namespace BMATM.Data;

// ==================== INITIALIZATION MANAGER ====================
public class DatabaseInitializationManager
{
    public readonly IUserService _userService;
    public readonly IProfileService _profileService;
    public readonly IATMService _atmService;

    public DatabaseInitializationManager(IUserService userService, IProfileService profileService, IATMService atmService)
    {
        _userService = userService;
        _profileService = profileService;
        _atmService = atmService;
    }

    public async Task InitializeAllAsync()
    {
        try
        {
            // Initialize in correct order due to dependencies
            await _userService.InitializeAsync().ConfigureAwait(false);
            await _profileService.InitializeAsync().ConfigureAwait(false);
            await _atmService.InitializeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize database", ex);
        }
    }
}