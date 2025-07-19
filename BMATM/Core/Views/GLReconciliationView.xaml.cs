namespace BMATM.Views;

/// <summary>
/// Interaction logic for ATMCollectionView.xaml
/// </summary>
public partial class GLReconciliationView : UserControl
{
    public GLReconciliationView(GLReconciliationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}