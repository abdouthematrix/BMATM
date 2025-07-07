namespace BMATM.Views;

/// <summary>
/// Interaction logic for ATMCollectionView.xaml
/// </summary>
public partial class AddATMView : UserControl
{
    public AddATMView(AddATMViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}