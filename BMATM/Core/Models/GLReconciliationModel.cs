namespace BMATM.Models;
public class GLReconciliationModel : ViewModelBase
{
    private decimal _remainingCash;
    private decimal _glBalance;
    private ObservableCollection<GLAdjustmentModel> _glAdjustments;

    public GLReconciliationModel()
    {
        GLAdjustments = new ObservableCollection<GLAdjustmentModel>();
        InitializeDefaultAdjustments();
        
    }
    public decimal RemainingCash
    {
        get => _remainingCash;
        set 
        {
            if (SetProperty(ref _remainingCash, value))
                CalculateGLBalance();
        }
    }

    public decimal GLBalance
    {
        get => _glBalance;
        private set => SetProperty(ref _glBalance, value);
    }

    public ObservableCollection<GLAdjustmentModel> GLAdjustments
    {
        get => _glAdjustments;
        set => SetProperty(ref _glAdjustments, value);
    }

    public decimal TotalAdjustments => GLAdjustments?.Sum(adj => adj.AdjustmentValue) ?? 0;

    public void CalculateGLBalance()
    {
        GLBalance = RemainingCash + TotalAdjustments;
    }

    private void InitializeDefaultAdjustments()
    {
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "سحب الكروت الائتمانية", LabelEnglish = "HPs Withdrawals", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "حركات حسابات تخص اليوم التالي", LabelEnglish = "ATM Previous Day", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداعات الكروت الائتمانية", LabelEnglish = "Credit Card Deposits", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "أيداعات المحفظة الاكترونية", LabelEnglish = "E-Wallet Deposits", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "سحب المحفظة الاكترونية", LabelEnglish = "E-Wallet Withdrawals", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "تمويل اليوم التالي", LabelEnglish = "Next Day Funding", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداعات اليوم التالي", LabelEnglish = "Next Day Deposits", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداعات اليوم الحالي", LabelEnglish = "Current Day Deposits", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "استبدال غير مرحل", LabelEnglish = "Non-Transferred Exchange", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "استبدال مرحل بالزيادة", LabelEnglish = "Transferred Exchange by Increase", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "قيود سحب مرحلة بالزيادة", LabelEnglish = "Withdrawal Entries by Increase", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "سحب غير مرحل", LabelEnglish = "Non-Transferred Withdrawals", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداع غير مرحل", LabelEnglish = "Non-Transferred Deposits", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداع مرحل بالزيادة", LabelEnglish = "Transferred Deposits by Increase", Operation = "+" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "تسويات", LabelEnglish = "Adjustments", Operation = "-" });
        GLAdjustments.Add(new GLAdjustmentModel { LabelArabic = "ايداع ميزة يوم التالي", LabelEnglish = "Next Day Feature Deposits", Operation = "+" });

        // Subscribe to property changes for auto-calculation
        foreach (var adjustment in GLAdjustments)        
            adjustment.PropertyChanged += OnAdjustmentPropertyChanged;
    }

    private void OnAdjustmentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GLAdjustmentModel.Amount))
        {
            CalculateGLBalance();
            OnPropertyChanged(nameof(TotalAdjustments));
        }
    }
    public void Refresh()
    {
        foreach (var adjustment in GLAdjustments)
            adjustment.Refresh();
    }
}