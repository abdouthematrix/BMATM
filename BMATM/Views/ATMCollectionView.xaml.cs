namespace BMATM.Views;

/// <summary>
/// Interaction logic for ATMCollectionView.xaml
/// </summary>
public partial class ATMCollectionView : UserControl
{
    public ATMCollectionView(ATMCollectionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}