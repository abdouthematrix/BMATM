using System;

namespace BMATM.Core.Entities
{
    public enum ReconciliationStatus
    {
        Pending,
        Balanced,
        Shortage,
        Over
    }

    public enum ATMType
    {
        DN, // Diebold Nixdorf
        NCR, // NCR Corporation
        Wincor, // Wincor Nixdorf
        Hyosung // Hyosung
    }

    public enum TransactionType
    {
        CashReconciliation,
        GLReconciliation,
        ManualAdjustment
    }

    public enum ReportType
    {
        Totals, // totals_{branch}_{ddMMyy}.html
        BNA,    // bna_tot_{branch}_{ddMMyy}.html
        SysRpt, // sysrpt_{branch}_{ddMMyy}.html
        Uncrtdsp // uncrtdsp_{branch}_{ddMMyy}.html
    }

    public enum AuditAction
    {
        Insert,
        Update,
        Delete
    }
}