namespace BMATM.Models;

public class SupervisorProfile : ViewModelBase
{
    private User _user;
    public User User
    {
        get => _user;
        set => SetProperty(ref _user, value);
    }    
    public ObservableCollection<ATMInfo> ATMs
    { 
        get;
        set;
    } = new ObservableCollection<ATMInfo>();
}