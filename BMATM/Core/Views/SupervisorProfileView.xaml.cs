namespace BMATM.Views;

/// <summary>
/// Interaction logic for SupervisorProfileView.xaml
/// </summary>
public partial class SupervisorProfileView : UserControl, INavigatable<User>
{
    public SupervisorProfileView(SupervisorProfileViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
    public async void SetNavigationParameter(User user)
    {
        if (DataContext is SupervisorProfileViewModel viewModel)
        {
            viewModel.CurrentUser = user;
            await viewModel.LoadUserDataAsync();
        }           
    }
}