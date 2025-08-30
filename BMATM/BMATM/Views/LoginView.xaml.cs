using System.Windows;
using System.Windows.Controls;
using BMATM.ViewModels;

namespace BMATM.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to username textbox when view loads
            UsernameTextBox.Focus();

            // Wire up password box to viewmodel since PasswordBox.Password is not bindable
            if (DataContext is LoginViewModel viewModel)
            {
                PasswordBox.PasswordChanged += (s, args) =>
                {
                    viewModel.Password = PasswordBox.Password;
                };

                // Clear password when viewmodel is reset
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(LoginViewModel.Password) &&
                        string.IsNullOrEmpty(viewModel.Password))
                    {
                        PasswordBox.Password = string.Empty;
                    }
                };
            }
        }
    }
}