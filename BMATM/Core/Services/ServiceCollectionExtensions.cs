namespace BMATM.Services;

// ==================== DEPENDENCY INJECTION SETUP ====================
public static class ServiceCollectionExtensions
{
    public static void AddBMATMDataServices(this IServiceCollection services, string connectionString = "Data Source=bmatm.db;Version=3;")
    {
        // Register DatabaseContext as singleton
        services.AddSingleton(provider => DatabaseContext.Instance(connectionString));
        // Register repositories
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        services.AddSingleton<IATMRepository, ATMRepository>();

        // Register services
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IATMService, ATMService>();

        services.AddSingleton<DatabaseInitializationManager, DatabaseInitializationManager>();
    }
}