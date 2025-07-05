namespace BMATM.Helpers;
public static class NavigationHelper
{
    public static event Action<UserControl>? NavigationRequested;

    public static void NavigateTo<T>() where T : UserControl
    {
        var control = App.AppHost.Services.GetRequiredService<T>();
        NavigationRequested?.Invoke(control);
    }

    public static void NavigateTo(UserControl control)
    {
        NavigationRequested?.Invoke(control);
    }
}
