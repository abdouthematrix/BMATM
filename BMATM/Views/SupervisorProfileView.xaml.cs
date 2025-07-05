using BMATM.Services;
using BMATM.ViewModels;
using System.Windows.Controls;

namespace BMATM.Views;

public partial class SupervisorProfileView : UserControl
{
    public SupervisorProfileView(SupervisorProfileViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}