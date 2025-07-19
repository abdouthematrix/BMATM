namespace BMATM.Models;
public class GLAdjustmentModel : ViewModelBase
{    
    private string _labelArabic;
    private string _labelEnglish;
    private string _operation;
    private decimal _amount;  
    public string AdjustmentLabel => Settings.Default.Language == "ar" ? LabelArabic : LabelEnglish;
    public string LabelArabic
    {
        get => _labelArabic;
        set
        {
            if (SetProperty(ref _labelArabic, value)) 
                OnPropertyChanged(nameof(AdjustmentLabel));
        }
    }
    public string LabelEnglish
    {
        get => _labelEnglish;
        set
        {
            if (SetProperty(ref _labelEnglish, value)) 
                OnPropertyChanged(nameof(AdjustmentLabel));
        }
    }
    public string Operation
    {
        get => _operation;
        set => SetProperty(ref _operation, value);
    }
    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }      
    public bool IsAddOperation => Operation == "+";
    public bool IsSubtractOperation => Operation == "-";
    public decimal AdjustmentValue => IsAddOperation ? Amount : -Amount;
    
    public void Refresh()
    {
        OnPropertyChanged(nameof(AdjustmentLabel));
    }
}
