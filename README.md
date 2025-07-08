# BMATM
<pre>```mermaidgraph TD
    %% User Journey Start
    Start([User Opens BMATM App]) --> Login{Login Screen}
    
    %% Login Process
    Login -->|Enter Domain Credentials| ValidateLogin[Validate Username/Password]
    ValidateLogin -->|Invalid| LoginError[Show Error Message]
    LoginError --> Login
    ValidateLogin -->|Valid| LoadProfile[Load Supervisor Profile]
    
    %% Main Dashboard
    LoadProfile --> Dashboard[Main Dashboard<br/>Show Profile & ATM Collection]
    Dashboard --> CheckATMs{Has ATMs?}
    
    %% ATM Management
    CheckATMs -->|No ATMs| AddFirstATM[Add New ATM]
    CheckATMs -->|Has ATMs| ATMActions{User Action}
    AddFirstATM --> ATMForm[ATM Configuration Form]
    ATMActions -->|Add ATM| ATMForm
    ATMActions -->|Select ATM| SelectATM[Select ATM from Collection]
    ATMForm --> SaveATM[Save ATM Configuration]
    SaveATM --> Dashboard
    
    %% ATM Day Selection
    SelectATM --> DaySelection[Select ATM Day for Review]
    DaySelection --> LoadDayData[Load ATM Day Data]
    LoadDayData --> CalculationChoice{Choose Calculation Type}
    
    %% Calculation Processes
    CalculationChoice -->|Dispensed/Deposited| DispensedCalc[Calculate Dispensed Amounts]
    CalculationChoice -->|GL Reconciliation| GLReconciliation[GL Reconciliation Process]
    
    %% Dispensed Calculation Flow
    DispensedCalc --> LoadCassetteData[Load Cassette Counter Data]
    LoadCassetteData --> CalculateDispensed[Calculate Total Dispensed<br/>عهدة الالة + التمويل = اجمالي العهدة<br/>اجمالي العهدة - النقدية المتبقية = اجمالي المنصرف]
    CalculateDispensed --> CompareATMReports[Compare with ATM Reports]
    CompareATMReports --> ShowDispensedResults[Show Calculation Results]
    ShowDispensedResults --> PrintOption{Print Results?}
    
    %% GL Reconciliation Flow
    GLReconciliation --> LoadGLData[Load GL Data]
    LoadGLData --> PerformReconciliation[Perform GL Reconciliation]
    PerformReconciliation --> ShowGLResults[Show Reconciliation Results]
    ShowGLResults --> PrintOption
    
    %% Print Process
    PrintOption -->|Yes| GenerateReport[Generate ATM Report]
    PrintOption -->|No| BackToDashboard[Back to Dashboard]
    GenerateReport --> PrintReport[Print Report]
    PrintReport --> BackToDashboard
    BackToDashboard --> Dashboard
    
    %% Code Components for Each Process
    
    %% Login Components
    Login -.->|Uses| LoginCode["`**Login Components:**
    - LoginView.xaml
    - LoginViewModel.cs
    - AuthenticationService.cs
    - User.cs
    - StringResources (AR/EN)`"]
    
    %% Dashboard Components
    Dashboard -.->|Uses| DashboardCode["`**Dashboard Components:**
    - MainView.xaml
    - MainViewModel.cs
    - SupervisorProfile.cs
    - ProfileService.cs
    - ATMCollectionView.xaml`"]
    
    %% ATM Management Components
    ATMForm -.->|Uses| ATMCode["`**ATM Management:**
    - AddATMView.xaml
    - AddATMViewModel.cs
    - ATM.cs
    - ATMType.cs
    - Cassette.cs
    - ATMService.cs
    - ATMCard.xaml`"]
    
    %% Day Selection Components
    DaySelection -.->|Uses| DayCode["`**Day Selection:**
    - ATMDayView.xaml
    - ATMDayViewModel.cs
    - ATMDay.cs
    - TransactionData.cs
    - ATMDataService.cs`"]
    
    %% Calculation Components
    CalculateDispensed -.->|Uses| CalcCode["`**Dispensed Calculation:**
    - DispensedCalculationView.xaml
    - DispensedCalculationViewModel.cs
    - DispensedCalculation.cs
    - CalculationService.cs
    - CalculationHelper.cs
    - CassetteCounter.cs`"]
    
    %% GL Reconciliation Components
    PerformReconciliation -.->|Uses| GLCode["`**GL Reconciliation:**
    - GLReconciliationView.xaml
    - GLReconciliationViewModel.cs
    - GLReconciliation.cs
    - GLReconciliationService.cs`"]
    
    %% Reporting Components
    GenerateReport -.->|Uses| ReportCode["`**Reporting:**
    - ReportView.xaml
    - ReportViewModel.cs
    - ATMReport.cs
    - ReportService.cs
    - PrintService.cs
    - ReportTemplate.xaml`"]
    
    %% Data Integration Components
    LoadCassetteData -.->|Uses| DataCode["`**Data Integration:**
    - DataIntegrationService.cs
    - ATMReportData.cs
    - ApiResponse.cs
    - ApiHelper.cs`"]
    
    %% Language Switching
    subgraph "Language Support"
        LangSwitch[Language Switch AR/EN]
        LangSwitch -.->|Uses| LangCode["`**Localization:**
        - LocalizationHelper.cs
        - StringResources.en.xaml
        - StringResources.ar.xaml`"]
    end
    
    %% User Actions and Decision Points
    Dashboard --> LangSwitch
    ATMActions --> LangSwitch
    CalculationChoice --> LangSwitch
    
    %% Styling for different process types
    classDef userAction fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef systemProcess fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef calculation fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef decision fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef codeComponent fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    classDef arabicProcess fill:#e0f2f1,stroke:#00695c,stroke-width:2px
    
    class Start,Login,ATMForm,DaySelection,PrintReport userAction
    class ValidateLogin,LoadProfile,LoadDayData,LoadCassetteData,LoadGLData,GenerateReport systemProcess
    class CalculateDispensed,PerformReconciliation,CompareATMReports calculation
    class CheckATMs,ATMActions,CalculationChoice,PrintOption decision
    class LoginCode,DashboardCode,ATMCode,DayCode,CalcCode,GLCode,ReportCode,DataCode,LangCode codeComponent
    class CalculateDispensed arabicProcess
```</pre>
GitHub will automatically render this as a diagram when viewing the file in the repository.
📌 Notes:
- No installation or configuration is needed—just commit the .md file with the Mermaid code.
- Mermaid is supported in Markdown files, issues, pull requests, and discussions.
- Some advanced Mermaid features (like tooltips or icons) may not render fully on GitHub.
For more details, check out GitHub’s official diagram documentation or this freeCodeCamp guide.
Would you like me to optimize your full diagram for GitHub rendering (e.g., trimming or splitting it for better readability)?
