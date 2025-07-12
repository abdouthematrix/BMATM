namespace BMATM.Helpers;
public static class NavigationHelper
{
    public static event Action<UserControl, object?>? NavigationRequested;

    // Navigate without parameters
    public static void NavigateTo<T>() where T : UserControl
    {
        var control = App.AppHost.Services.GetRequiredService<T>();
        NavigationRequested?.Invoke(control, null);
    }

    // Navigate with object parameter
    public static void NavigateTo<T>(object? parameter) where T : UserControl
    {
        var control = App.AppHost.Services.GetRequiredService<T>();

        // If the control implements INavigatable, pass the parameter
        if (control is INavigatable navigatable)
        {
            navigatable.SetNavigationParameter(parameter);
        }

        NavigationRequested?.Invoke(control, parameter);
    }

    // Navigate with existing control instance
    public static void NavigateTo(UserControl control, object? parameter = null)
    {
        if (control is INavigatable navigatable)
        {
            navigatable.SetNavigationParameter(parameter);
        }

        NavigationRequested?.Invoke(control, parameter);
    }

    // Navigate with strongly typed parameter
    public static void NavigateTo<T, TParam>(TParam parameter)
        where T : UserControl, INavigatable<TParam>
    {
        var control = App.AppHost.Services.GetRequiredService<T>();
        control.SetNavigationParameter(parameter);
        NavigationRequested?.Invoke(control, parameter);
    }
}

// Interface for controls that can receive navigation parameters
public interface INavigatable
{
    void SetNavigationParameter(object? parameter);
}
// Generic interface for strongly typed parameters
public interface INavigatable<T> : INavigatable
{
    void SetNavigationParameter(T parameter);

    // Explicit interface implementation to avoid conflicts
    async void INavigatable.SetNavigationParameter(object? parameter)
    {
        if (parameter is T typedParam)
        {
            SetNavigationParameter(typedParam);
        }
    }
}
