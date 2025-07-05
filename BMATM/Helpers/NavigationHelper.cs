namespace BMATM.Helpers;

public static class NavigationHelper
{
    public static event Action<UserControl>? NavigationRequested;

    public static void NavigateTo<T>() where T : UserControl, new()
    {
        var control = new T();
        NavigationRequested?.Invoke(control);
    }

    public static void NavigateTo(UserControl control)
    {
        NavigationRequested?.Invoke(control);
    }
}