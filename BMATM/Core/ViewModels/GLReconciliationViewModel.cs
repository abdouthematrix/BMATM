
namespace BMATM.ViewModels;

public class GLReconciliationViewModel : ViewModelBase
{
    private GLReconciliationModel _glReconciliation = new GLReconciliationModel();    
    public GLReconciliationModel GLReconciliation
    {
        get => _glReconciliation;
        set => SetProperty(ref _glReconciliation, value);
    }
    public void Refresh()
    {
        GLReconciliation.Refresh();
    }
}