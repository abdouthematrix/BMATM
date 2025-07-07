namespace BMATM.Models;
public enum ATMType
{
    NCR,
    DN
}

public static class ATMTypeExtensions
{
    public static string GetImagePath(this ATMType type)
    {
        return type switch
        {
            ATMType.NCR => "pack://application:,,,/Resources/Images/NCR_ATM.png",
            ATMType.DN => "pack://application:,,,/Resources/Images/DN_ATM.png",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static string GetDisplayName(this ATMType type)
    {
        return type switch
        {
            ATMType.NCR => "NCR",
            ATMType.DN => "Diebold Nixdorf",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}