namespace BMATM.Views;

/// <summary>
/// Interaction logic for ATMCollectionView.xaml
/// </summary>
public partial class AddATMView : UserControl, INavigatable<KeyValuePair<SupervisorProfile, ATMInfo>>
{
    public AddATMView(ATMViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
    public async void SetNavigationParameter(KeyValuePair<SupervisorProfile, ATMInfo> parameter)
    {
        if (DataContext is ATMViewModel viewModel)
        {
            if (parameter.Key is SupervisorProfile profile)
            {
                viewModel.SupervisorProfile = profile;
            }
            if (parameter.Value is ATMInfo atm)
            {
                viewModel.IsEditMode = true;
                viewModel.LoadATMData(atm);
            }
        }
    }
}