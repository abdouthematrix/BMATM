# BMATM Prototype - Complete Project Roadmap

## 📂 Project Structure

```
BMATM/ (.NET Framework 4.8 WPF Project)
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── BMATM.csproj (targeting .NET Framework 4.8)
├── packages.config (System.Data.SQLite only)
├── Core/
│   ├── Entities/
│   │   ├── Supervisor.cs
│   │   ├── ATM.cs
│   │   ├── ATMTransaction.cs
│   │   └── Enums.cs
│   └── Constants/
│       └── AppConstants.cs
├── Data/
│   ├── SQLiteConnectionFactory.cs
│   ├── QueryBuilder.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── SupervisorRepository.cs
│   │   ├── ATMRepository.cs
│   │   └── TransactionRepository.cs
│   └── Schema/
│       └── DatabaseInitializer.cs
├── Services/
│   ├── Reports/
│   │   ├── HtmlReportReader.cs
│   │   └── ReportParser.cs
│   ├── Reconciliation/
│   │   ├── CashReconciliationService.cs
│   │   └── GLReconciliationService.cs
│   └── Navigation/
│       └── NavigationService.cs
├── ViewModels/
│   ├── Base/
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── LoginViewModel.cs
│   ├── MainWindowViewModel.cs
│   ├── ATMCarouselViewModel.cs
│   ├── CashReconciliationViewModel.cs
│   └── GLReconciliationViewModel.cs
├── Views/
│   ├── LoginView.xaml
│   ├── LoginView.xaml.cs
│   ├── ATMCarouselView.xaml
│   ├── ATMCarouselView.xaml.cs
│   ├── CashReconciliationView.xaml
│   ├── CashReconciliationView.xaml.cs
│   ├── GLReconciliationView.xaml
│   └── GLReconciliationView.xaml.cs
└── Reports/
    └── (Local HTML report files)
```

## 🔧 .NET Framework 4.8 Configuration

### Project File (BMATM.csproj)
```xml
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <UseWPF>true</UseWPF>
    <OutputType>WinExe</OutputType>
    <StartupObject>BMATM.App</StartupObject>
  </PropertyGroup>
  
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Data" />
    <Reference Include="System.Data.SQLite">
      <HintPath>..\packages\System.Data.SQLite.Core.1.0.118.0\lib\net48\System.Data.SQLite.dll</HintPath>
    </Reference>
    <Reference Include="System.Xaml" />
    <Reference Include="System.Xml" />
    <Reference Include="System.Core" />
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System.Net.Http" />
    <Reference Include="WindowsBase" />
    <Reference Include="PresentationCore" />
    <Reference Include="PresentationFramework" />
  </ItemGroup>
</Project>
```

### packages.config
```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="System.Data.SQLite.Core" version="1.0.118.0" targetFramework="net48" />
</packages>
```

---

## 📋 SQLite Database Schema

```sql
-- Supervisors Table
CREATE TABLE Supervisors (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Email TEXT,
    IsActive BOOLEAN DEFAULT 1,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLoginDate DATETIME
);

-- ATMs Table
CREATE TABLE ATMs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SupervisorId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Branch TEXT NOT NULL,
    GLNumber TEXT NOT NULL,
    ATMType TEXT NOT NULL,
    CassetteCount INTEGER NOT NULL,
    IsActive BOOLEAN DEFAULT 1,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SupervisorId) REFERENCES Supervisors(Id)
);

-- ATM Transactions Table
CREATE TABLE ATMTransactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ATMId INTEGER NOT NULL,
    TransactionDate DATE NOT NULL,
    BeginningCash DECIMAL(15,2),
    AddedCash DECIMAL(15,2),
    RecycledCash DECIMAL(15,2),
    EndingCash DECIMAL(15,2),
    DepositedCash DECIMAL(15,2),
    GLBalance DECIMAL(15,2),
    IsReconciled BOOLEAN DEFAULT 0,
    ReconciliationStatus TEXT, -- 'Balanced', 'Shortage', 'Over'
    Variance DECIMAL(15,2),
    Notes TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ATMId) REFERENCES ATMs(Id)
);

-- Audit Log Table (Optional)
CREATE TABLE AuditLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TableName TEXT NOT NULL,
    RecordId INTEGER NOT NULL,
    Action TEXT NOT NULL, -- 'INSERT', 'UPDATE', 'DELETE'
    OldValues TEXT,
    NewValues TEXT,
    UserId INTEGER,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

---

## 🎯 Phase Structure

### Phase 1: Core Infrastructure & Database Foundation
**Duration**: 2-3 days  
**Goal**: Build the entire data access layer and core entities  
**Manual Testing**: Launch app, verify database creation and initialization

**Components to Build:**
- Core entities (Supervisor, ATM, ATMTransaction, Enums)
- SQLiteConnectionFactory with database initialization
- Custom QueryBuilder with fluent interface
- All repository interfaces and implementations
- DatabaseInitializer with schema creation and sample data

**Deliverables:**
- Working database layer with full CRUD operations
- Sample data automatically inserted on first run
- Database file created in application directory

---

### Phase 2: MVVM Foundation & Application Shell
**Duration**: 2-3 days  
**Goal**: Establish WPF MVVM architecture and navigation framework  
**Manual Testing**: Navigate between all views (even if empty), verify view switching

**Components to Build:**
- BaseViewModel with INotifyPropertyChanged
- RelayCommand implementation
- NavigationService for view switching
- MainWindow with ContentControl and ViewModelLocator
- MainWindowViewModel with current view management
- All View files (empty but navigable)
- App.xaml with startup configuration

**Deliverables:**
- Complete navigation system working
- All views accessible via navigation
- MVVM pattern properly established

---

### Phase 3: Authentication & Login System
**Duration**: 1-2 days  
**Goal**: Complete login functionality with database validation  
**Manual Testing**: Login with valid/invalid credentials, verify supervisor session

**Components to Build:**
- LoginView with modern UI design
- LoginViewModel with credential validation
- Password hashing utility methods
- Session management in MainWindowViewModel
- Login form validation and error display

**Deliverables:**
- Functional login screen
- Database credential validation
- Proper session handling
- Navigation to ATM carousel after successful login

---

### Phase 4: ATM Management & Carousel Interface
**Duration**: 3-4 days  
**Goal**: Complete ATM CRUD operations with carousel-style UI  
**Manual Testing**: Add, edit, delete ATMs; verify data persistence and UI updates

**Components to Build:**
- ATMCarouselView with scrollable card layout
- ATMCarouselViewModel with ObservableCollection management
- ATM add/edit dialog or inline editing
- ATM deletion with confirmation
- Card-based UI design with ATM details display
- Supervisor-specific ATM filtering

**Deliverables:**
- Fully functional ATM management interface
- Carousel-style layout with smooth scrolling
- Complete CRUD operations working
- Data binding and UI updates working correctly

---

### Phase 5: Report Processing Engine
**Duration**: 3-4 days  
**Goal**: HTML report parsing and data extraction system  
**Manual Testing**: Load sample HTML reports, verify data extraction accuracy

**Components to Build:**
- HtmlReportReader with regex parsing patterns
- ReportParser with business logic for all report types
- Report data models (ReportData, TransactionDetail, etc.)
- File loading and validation utilities
- Report integrity checking algorithms
- Support for all report formats:
  - totals_{branch}_{ddMMyy}.html
  - bna_tot_{branch}_{ddMMyy}.html  
  - sysrpt_{branch}_{ddMMyy}.html
  - uncrtdsp_{branch}_{ddMMyy}.html

**Deliverables:**
- Complete report processing engine
- Accurate data extraction from HTML files
- Report validation and integrity checking
- Sample HTML report files for testing

---

### Phase 6: Cash Reconciliation Workflow
**Duration**: 4-5 days  
**Goal**: Complete cash reconciliation with counter entry and report comparison  
**Manual Testing**: Enter cash counters, load reports, verify reconciliation calculations

**Components to Build:**
- CashReconciliationView with counter input forms
- CashReconciliationViewModel with calculation logic
- CashReconciliationService for business operations
- Report loading and processing integration
- Variance calculation and status determination
- Results display with detailed breakdown
- Integration with TransactionRepository for saving results

**Deliverables:**
- Complete cash reconciliation interface
- Accurate variance calculations
- Report comparison functionality
- Reconciliation status determination (Balanced/Shortage/Over)

---

### Phase 7: GL Reconciliation & Final Integration
**Duration**: 3-4 days  
**Goal**: GL reconciliation functionality and final system integration  
**Manual Testing**: Complete end-to-end workflows, verify all integrations

**Components to Build:**
- GLReconciliationView with GL balance input
- GLReconciliationViewModel with comparison logic
- GLReconciliationService for GL operations
- CSV/Excel GL report parsing
- Category-wise comparison (Visa, Master, etc.)
- Final integration testing and bug fixes
- Polish UI/UX across all views

**Deliverables:**
- Complete GL reconciliation functionality
- Full end-to-end application workflow
- All integrations working smoothly
- Production-ready prototype

---

## 🚀 Class Skeletons & Method Signatures

### Core Entities

#### Supervisor.cs
```csharp
namespace BMATM.Core.Entities
{
    public class Supervisor
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
```

#### ATM.cs
```csharp
namespace BMATM.Core.Entities
{
    public class ATM
    {
        public int Id { get; set; }
        public int SupervisorId { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string GLNumber { get; set; }
        public string ATMType { get; set; }
        public int CassetteCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
```

#### ATMTransaction.cs
```csharp
namespace BMATM.Core.Entities
{
    public class ATMTransaction
    {
        public int Id { get; set; }
        public int ATMId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? BeginningCash { get; set; }
        public decimal? AddedCash { get; set; }
        public decimal? RecycledCash { get; set; }
        public decimal? EndingCash { get; set; }
        public decimal? DepositedCash { get; set; }
        public decimal? GLBalance { get; set; }
        public bool IsReconciled { get; set; }
        public string ReconciliationStatus { get; set; }
        public decimal? Variance { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
```

### Database Layer

#### SQLiteConnectionFactory.cs
```csharp
namespace BMATM.Data
{
    public class SQLiteConnectionFactory
    {
        private readonly string _connectionString;
        
        public SQLiteConnectionFactory(string databasePath) { }
        public SQLiteConnection CreateConnection() { }
        public void InitializeDatabase() { }
        public bool DatabaseExists() { }
    }
}
```

#### QueryBuilder.cs
```csharp
namespace BMATM.Data
{
    public class QueryBuilder
    {
        private StringBuilder _query;
        private List<SQLiteParameter> _parameters;
        
        public QueryBuilder() { }
        public QueryBuilder Select(params string[] columns) { }
        public QueryBuilder From(string table) { }
        public QueryBuilder Where(string condition, object value = null) { }
        public QueryBuilder And(string condition, object value = null) { }
        public QueryBuilder Or(string condition, object value = null) { }
        public QueryBuilder OrderBy(string column, bool descending = false) { }
        public QueryBuilder Limit(int count) { }
        public QueryBuilder InsertInto(string table) { }
        public QueryBuilder Values(Dictionary<string, object> values) { }
        public QueryBuilder Update(string table) { }
        public QueryBuilder Set(Dictionary<string, object> values) { }
        public QueryBuilder DeleteFrom(string table) { }
        public string Build() { }
        public SQLiteParameter[] GetParameters() { }
        public void Clear() { }
    }
}
```

### Repository Layer

#### IRepository.cs
```csharp
namespace BMATM.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        int Insert(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }
}
```

#### SupervisorRepository.cs
```csharp
namespace BMATM.Data.Repositories
{
    public class SupervisorRepository : IRepository<Supervisor>
    {
        private readonly SQLiteConnectionFactory _connectionFactory;
        
        public SupervisorRepository(SQLiteConnectionFactory connectionFactory) { }
        
        public Supervisor GetById(int id) { }
        public IEnumerable<Supervisor> GetAll() { }
        public int Insert(Supervisor entity) { }
        public bool Update(Supervisor entity) { }
        public bool Delete(int id) { }
        public Supervisor GetByUsername(string username) { }
        public bool ValidateCredentials(string username, string passwordHash) { }
        public void UpdateLastLoginDate(int supervisorId) { }
    }
}
```

#### ATMRepository.cs
```csharp
namespace BMATM.Data.Repositories
{
    public class ATMRepository : IRepository<ATM>
    {
        private readonly SQLiteConnectionFactory _connectionFactory;
        
        public ATMRepository(SQLiteConnectionFactory connectionFactory) { }
        
        public ATM GetById(int id) { }
        public IEnumerable<ATM> GetAll() { }
        public int Insert(ATM entity) { }
        public bool Update(ATM entity) { }
        public bool Delete(int id) { }
        public IEnumerable<ATM> GetBySupervisorId(int supervisorId) { }
        public bool ExistsWithGLNumber(string glNumber, int? excludeId = null) { }
    }
}
```

### MVVM Base Classes

#### BaseViewModel.cs
```csharp
namespace BMATM.ViewModels.Base
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) { }
        protected virtual void OnViewLoaded() { }
        protected virtual void OnViewUnloaded() { }
    }
}
```

#### RelayCommand.cs
```csharp
namespace BMATM.ViewModels.Base
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) { }
        public RelayCommand(Action execute, Func<bool> canExecute = null) { }
        
        public event EventHandler CanExecuteChanged;
        
        public bool CanExecute(object parameter) { }
        public void Execute(object parameter) { }
        public void RaiseCanExecuteChanged() { }
    }
}
```

### Navigation Service

#### NavigationService.cs
```csharp
namespace BMATM.Services.Navigation
{
    public class NavigationService
    {
        public event Action<BaseViewModel> NavigationRequested;
        
        public void NavigateTo<T>() where T : BaseViewModel, new() { }
        public void NavigateTo<T>(object parameter) where T : BaseViewModel, new() { }
        public void NavigateToLogin() { }
        public void NavigateToATMCarousel(Supervisor supervisor) { }
        public void NavigateToCashReconciliation(ATM atm, DateTime date) { }
        public void NavigateToGLReconciliation(ATM atm, DateTime date) { }
        public void NavigateBack() { }
    }
}
```

### ViewModels

#### MainWindowViewModel.cs
```csharp
namespace BMATM.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private readonly NavigationService _navigationService;
        
        public MainWindowViewModel(NavigationService navigationService) { }
        
        public BaseViewModel CurrentViewModel { get; set; }
        public string WindowTitle { get; set; }
        public Supervisor CurrentSupervisor { get; set; }
        
        private void OnNavigationRequested(BaseViewModel viewModel) { }
        private void UpdateWindowTitle() { }
    }
}
```

#### LoginViewModel.cs
```csharp
namespace BMATM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoading;
        
        public LoginViewModel(SupervisorRepository supervisorRepository, NavigationService navigationService) { }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsLoading { get; set; }
        public bool IsLoginEnabled { get; }
        
        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }
        
        private void ExecuteLogin() { }
        private bool CanExecuteLogin() { }
        private void ExecuteExit() { }
        private string HashPassword(string password) { }
        private void ClearForm() { }
    }
}
```

#### ATMCarouselViewModel.cs
```csharp
namespace BMATM.ViewModels
{
    public class ATMCarouselViewModel : BaseViewModel
    {
        private ObservableCollection<ATM> _atms;
        private ATM _selectedATM;
        private Supervisor _currentSupervisor;
        
        public ATMCarouselViewModel(ATMRepository atmRepository, NavigationService navigationService) { }
        
        public ObservableCollection<ATM> ATMs { get; set; }
        public ATM SelectedATM { get; set; }
        public Supervisor CurrentSupervisor { get; set; }
        public string WelcomeMessage { get; }
        
        public ICommand AddATMCommand { get; }
        public ICommand EditATMCommand { get; }
        public ICommand DeleteATMCommand { get; }
        public ICommand StartReconciliationCommand { get; }
        public ICommand LogoutCommand { get; }
        
        public void LoadATMs(int supervisorId) { }
        private void ExecuteAddATM() { }
        private void ExecuteEditATM() { }
        private void ExecuteDeleteATM() { }
        private void ExecuteStartReconciliation() { }
        private void ExecuteLogout() { }
        private bool CanEditOrDelete() { }
        private bool CanStartReconciliation() { }
    }
}
```

### Services Layer

#### HtmlReportReader.cs
```csharp
namespace BMATM.Services.Reports
{
    public class HtmlReportReader
    {
        public string ReadReportContent(string filePath) { }
        public Dictionary<string, string> ExtractTableData(string htmlContent, string tableIdentifier) { }
        public List<Dictionary<string, string>> ParseTableRows(string htmlContent, string tableSelector) { }
        public string ExtractTotalValue(string htmlContent, string totalPattern) { }
        public bool ValidateReportFormat(string htmlContent, string expectedFormat) { }
        
        private string GetTotalsReportPattern() { }
        private string GetBNAReportPattern() { }
        private string GetSysReportPattern() { }
        private string GetUncertainReportPattern() { }
        private decimal ParseCurrencyValue(string value) { }
    }
}
```

#### CashReconciliationService.cs
```csharp
namespace BMATM.Services.Reconciliation
{
    public class CashReconciliationService
    {
        private readonly ReportParser _reportParser;
        private readonly TransactionRepository _transactionRepository;
        
        public CashReconciliationService(ReportParser reportParser, TransactionRepository transactionRepository) { }
        
        public ReconciliationResult ReconcileCash(CashReconciliationRequest request) { }
        public ReconciliationResult CompareCountersWithReports(CashCounters counters, List<ReportData> reports) { }
        public List<string> ValidateCounterInputs(CashCounters counters) { }
        public decimal CalculateExpectedCash(CashCounters counters, List<ReportData> reports) { }
        public string DetermineReconciliationStatus(decimal variance, decimal tolerance = 1.00m) { }
        public bool SaveReconciliation(ATMTransaction transaction) { }
        
        private bool ValidateReportConsistency(List<ReportData> reports) { }
        private decimal CalculateNetCashFlow(List<ReportData> reports) { }
    }
}
```

---

## 📅 Implementation Timeline

**Total Estimated Duration: 18-24 days**

| Phase | Duration | Key Milestone |
|-------|----------|---------------|
| Phase 1 | 2-3 days | Database layer complete |
| Phase 2 | 2-3 days | Navigation system working |
| Phase 3 | 1-2 days | Login functionality complete |
| Phase 4 | 3-4 days | ATM management complete |
| Phase 5 | 3-4 days | Report processing complete |
| Phase 6 | 4-5 days | Cash reconciliation complete |
| Phase 7 | 3-4 days | Full system integration |

Each phase builds upon the previous one and can be manually tested independently before proceeding to the next phase. This ensures steady progress and early identification of any architectural issues.

## Sample Data Requirements
```sql
-- Test Supervisor
INSERT INTO Supervisors (Username, PasswordHash, FullName, Email)
VALUES ('abdelrahmanhas', 'hashedpassword123', 'Abdelrahman Hassan', 'abdelrahmanhas@banquemisr.com');

-- Test ATMs
INSERT INTO ATMs (SupervisorId, Name, Branch, GLNumber, ATMType, CassetteCount)
VALUES 
(1, 'Sheraton Cairo Hotel ATM', '707', '101103576', 'DN', 4),
```