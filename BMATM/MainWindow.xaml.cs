using BMATM.Views;

namespace BMATM;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        // Show login view by default
        MainContent.Content = new LoginView();
    }
}