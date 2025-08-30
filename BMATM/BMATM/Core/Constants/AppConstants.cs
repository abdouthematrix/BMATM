namespace BMATM.Core.Constants
{
    public static class AppConstants
    {
        // Database Configuration
        public const string DATABASE_FILENAME = "BMATM.db";
        public const string CONNECTION_STRING_TEMPLATE = "Data Source={0};Version=3;";

        // Application Settings
        public const string APP_TITLE = "BMATM - Bank Misr ATM Reconciliation System";
        public const string APP_VERSION = "1.0.0";

        // Reconciliation Settings
        public const decimal DEFAULT_CASH_TOLERANCE = 1.00m; // 1 Egyptian Pound
        public const decimal DEFAULT_GL_TOLERANCE = 0.01m;   // 1 Piaster

        // ATM Types
        public const string ATM_TYPE_DN = "DN";         // Diebold Nixdorf
        public const string ATM_TYPE_NCR = "NCR";       // NCR Corporation
        public const string ATM_TYPE_WINCOR = "Wincor"; // Wincor Nixdorf
        public const string ATM_TYPE_HYOSUNG = "Hyosung"; // Hyosung

        // Reconciliation Status Values
        public const string STATUS_PENDING = "Pending";
        public const string STATUS_BALANCED = "Balanced";
        public const string STATUS_SHORTAGE = "Shortage";
        public const string STATUS_OVER = "Over";

        // Report File Patterns
        public const string REPORT_TOTALS_PATTERN = "totals_{0}_{1}.html";
        public const string REPORT_BNA_PATTERN = "bna_tot_{0}_{1}.html";
        public const string REPORT_SYSRPT_PATTERN = "sysrpt_{0}_{1}.html";
        public const string REPORT_UNCRTDSP_PATTERN = "uncrtdsp_{0}_{1}.html";

        // Date Formats
        public const string DATE_FORMAT_FILENAME = "ddMMyy";
        public const string DATE_FORMAT_DISPLAY = "dd/MM/yyyy";
        public const string DATETIME_FORMAT_DISPLAY = "dd/MM/yyyy HH:mm";

        // UI Settings
        public const int CAROUSEL_ITEM_WIDTH = 300;
        public const int CAROUSEL_ITEM_HEIGHT = 200;
        public const double UI_ANIMATION_DURATION = 0.3;

        // Security Settings
        public const int PASSWORD_MIN_LENGTH = 6;
        public const int SESSION_TIMEOUT_MINUTES = 30;

        // Error Messages
        public const string ERROR_DATABASE_CONNECTION = "Unable to connect to database. Please check your installation.";
        public const string ERROR_INVALID_CREDENTIALS = "Invalid username or password.";
        public const string ERROR_ATM_NOT_FOUND = "ATM not found or access denied.";
        public const string ERROR_RECONCILIATION_FAILED = "Reconciliation process failed. Please try again.";
        public const string ERROR_REPORT_NOT_FOUND = "Report file not found or invalid format.";

        // Success Messages
        public const string SUCCESS_LOGIN = "Login successful. Welcome!";
        public const string SUCCESS_ATM_SAVED = "ATM information saved successfully.";
        public const string SUCCESS_RECONCILIATION_COMPLETE = "Reconciliation completed successfully.";
        public const string SUCCESS_REPORT_PROCESSED = "Report processed successfully.";

        // Default Values
        public const int DEFAULT_CASSETTE_COUNT = 4;
        public const string DEFAULT_BRANCH = "707"; // Sheraton Cairo Hotel branch
        public const string DEFAULT_ATM_TYPE = ATM_TYPE_DN;
    }
}