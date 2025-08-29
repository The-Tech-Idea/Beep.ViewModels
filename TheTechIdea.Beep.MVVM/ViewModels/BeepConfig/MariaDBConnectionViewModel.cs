using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.MVVM.Utilities;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class MariaDBConnectionViewModel : BaseViewModel
    {
        [ObservableProperty]
        UnitofWork<ConnectionProperties> dBWork;

        [ObservableProperty]
        ConnectionProperties connection;

        [ObservableProperty]
        List<AppFilter> filters;

        [ObservableProperty]
        string selectedCategoryTextValue;

        [ObservableProperty]
        int selectedCategoryValue;

        [ObservableProperty]
        DatasourceCategory selectedCategoryItem;

        [ObservableProperty]
        Array datasourcesCategorys;

        [ObservableProperty]
        int selectedconnectionidx;

        [ObservableProperty]
        string selectedconnectionGuid;

        [ObservableProperty]
        List<string> packageNames;

        [ObservableProperty]
        List<string> packageVersions;

        [ObservableProperty]
        string selectedpackage;

        [ObservableProperty]
        string selectedversion;

        [ObservableProperty]
        public List<EntityField> fields;

        [ObservableProperty]
        List<ConnectionDriversConfig> mariaDBDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedMariaDBDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string databaseName;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        string userId;

        [ObservableProperty]
        string server;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        bool allowBatch;

        [ObservableProperty]
        bool allowLoadLocalInfile;

        [ObservableProperty]
        bool allowPublicKeyRetrieval;

        [ObservableProperty]
        bool allowUserVariables;

        [ObservableProperty]
        bool autoEnlist;

        [ObservableProperty]
        string certificateFile;

        [ObservableProperty]
        string certificatePassword;

        [ObservableProperty]
        string certificateStoreLocation;

        [ObservableProperty]
        string certificateThumbprint;

        [ObservableProperty]
        string characterSet;

        [ObservableProperty]
        int commandTimeout;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        bool convertZeroDateTime;

        [ObservableProperty]
        string database;

        [ObservableProperty]
        string defaultCommandTimeout;

        [ObservableProperty]
        bool enforcePooledConnection;

        [ObservableProperty]
        bool ignoreCommandTransaction;

        [ObservableProperty]
        bool ignorePrepare;

        [ObservableProperty]
        bool interactiveSession;

        [ObservableProperty]
        bool logging;

        [ObservableProperty]
        int maximumPoolSize;

        [ObservableProperty]
        int minimumPoolSize;

        [ObservableProperty]
        string passwordChar;

        [ObservableProperty]
        string pipeName;

        [ObservableProperty]
        bool pooling;

        [ObservableProperty]
        int portNumber;

        [ObservableProperty]
        string procedureCacheSize;

        [ObservableProperty]
        string protocol;

        [ObservableProperty]
        bool respectBinaryFlags;

        [ObservableProperty]
        string serverRsaPublicKeyFile;

        [ObservableProperty]
        string sharedMemoryName;

        [ObservableProperty]
        bool sslMode;

        [ObservableProperty]
        string sslCa;

        [ObservableProperty]
        string sslCert;

        [ObservableProperty]
        string sslKey;

        [ObservableProperty]
        bool treatTinyAsBoolean;

        [ObservableProperty]
        bool useAffectedRows;

        [ObservableProperty]
        bool useCompression;

        [ObservableProperty]
        bool useDefaultCommandTimeoutForRead;

        [ObservableProperty]
        bool usePerformanceMonitor;

        [ObservableProperty]
        string userIdChar;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public MariaDBConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            mariaDBDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.MariaDB)
                {
                    mariaDBDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.MariaDB))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (mariaDBDatabaseTypes.Count > 0)
            {
                SelectedMariaDBDatabaseType = mariaDBDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.MariaDB), Operator = "=" });
            DBWork.Get(Filters);

            Port = 3306;
            ConnectionTimeout = 15;
            CommandTimeout = 30;
            Pooling = true;
            MinimumPoolSize = 0;
            MaximumPoolSize = 100;
            AllowBatch = true;
            AllowLoadLocalInfile = false;
            AllowPublicKeyRetrieval = false;
            AllowUserVariables = false;
            AutoEnlist = true;
            ConvertZeroDateTime = false;
            EnforcePooledConnection = false;
            IgnoreCommandTransaction = false;
            IgnorePrepare = false;
            InteractiveSession = false;
            Logging = false;
            RespectBinaryFlags = true;
            SslMode = false;
            TreatTinyAsBoolean = true;
            UseAffectedRows = false;
            UseCompression = false;
            UseDefaultCommandTimeoutForRead = false;
            UsePerformanceMonitor = false;
            CharacterSet = "utf8mb4";
            Protocol = "socket";
        }

        [RelayCommand]
        public void SaveConnection()
        {
            if (Connection == null)
            {
                Connection = new ConnectionProperties();
            }
            Connection.ConnectionName = CurrentDataSourceName;
            Connection.Database = DatabaseName;
            Connection.UserID = UserId;
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= DataSourceType.MariaDB;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedMariaDBDatabaseType != null)
            {
                Connection.DriverName = SelectedMariaDBDatabaseType.PackageName;
                Connection.DriverVersion = SelectedMariaDBDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "MariaDB");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            DatabaseName = "";
            UserId = "root";
            Password = "";
            Server = "localhost";
            Port = 3306;
            AllowBatch = true;
            AllowLoadLocalInfile = false;
            AllowPublicKeyRetrieval = false;
            AllowUserVariables = false;
            AutoEnlist = true;
            CertificateFile = "";
            CertificatePassword = "";
            CertificateStoreLocation = "";
            CertificateThumbprint = "";
            CharacterSet = "utf8mb4";
            CommandTimeout = 30;
            ConnectionTimeout = 15;
            ConvertZeroDateTime = false;
            Database = "";
            DefaultCommandTimeout = "";
            EnforcePooledConnection = false;
            IgnoreCommandTransaction = false;
            IgnorePrepare = false;
            InteractiveSession = false;
            Logging = false;
            MaximumPoolSize = 100;
            MinimumPoolSize = 0;
            PasswordChar = "";
            PipeName = "";
            Pooling = true;
            PortNumber = 3306;
            ProcedureCacheSize = "";
            Protocol = "socket";
            RespectBinaryFlags = true;
            ServerRsaPublicKeyFile = "";
            SharedMemoryName = "";
            SslMode = false;
            SslCa = "";
            SslCert = "";
            SslKey = "";
            TreatTinyAsBoolean = true;
            UseAffectedRows = false;
            UseCompression = false;
            UseDefaultCommandTimeoutForRead = false;
            UsePerformanceMonitor = false;
            UserIdChar = "";
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                DatabaseName = Connection.Database;
                UserId = Connection.UserID;
                Password = Connection.Password;
                ParseConnectionString(Connection.ConnectionString);
                SelectedconnectionGuid = Connection.GuidID;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            if (!string.IsNullOrEmpty(Server))
            {
                builder["Server"] = Server;
            }

            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                builder["Database"] = DatabaseName;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["Uid"] = UserId;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Pwd"] = Password;
            }

            builder["Allow Batch"] = AllowBatch;
            builder["Allow Load Local Infile"] = AllowLoadLocalInfile;
            builder["Allow Public Key Retrieval"] = AllowPublicKeyRetrieval;
            builder["Allow User Variables"] = AllowUserVariables;
            builder["Auto Enlist"] = AutoEnlist;

            if (!string.IsNullOrEmpty(CertificateFile))
            {
                builder["Certificate File"] = CertificateFile;
            }

            if (!string.IsNullOrEmpty(CertificatePassword))
            {
                builder["Certificate Password"] = CertificatePassword;
            }

            if (!string.IsNullOrEmpty(CertificateStoreLocation))
            {
                builder["Certificate Store Location"] = CertificateStoreLocation;
            }

            if (!string.IsNullOrEmpty(CertificateThumbprint))
            {
                builder["Certificate Thumbprint"] = CertificateThumbprint;
            }

            if (!string.IsNullOrEmpty(CharacterSet))
            {
                builder["Character Set"] = CharacterSet;
            }

            builder["Command Timeout"] = CommandTimeout;
            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Convert Zero DateTime"] = ConvertZeroDateTime;
            builder["Enforce Pooled Connection"] = EnforcePooledConnection;
            builder["Ignore Command Transaction"] = IgnoreCommandTransaction;
            builder["Ignore Prepare"] = IgnorePrepare;
            builder["Interactive Session"] = InteractiveSession;
            builder["Logging"] = Logging;
            builder["Maximum Pool Size"] = MaximumPoolSize;
            builder["Minimum Pool Size"] = MinimumPoolSize;

            if (!string.IsNullOrEmpty(PasswordChar))
            {
                builder["Password Char"] = PasswordChar;
            }

            if (!string.IsNullOrEmpty(PipeName))
            {
                builder["Pipe Name"] = PipeName;
            }

            builder["Pooling"] = Pooling;
            builder["Port Number"] = PortNumber;

            if (!string.IsNullOrEmpty(ProcedureCacheSize))
            {
                builder["Procedure Cache Size"] = ProcedureCacheSize;
            }

            if (!string.IsNullOrEmpty(Protocol))
            {
                builder["Protocol"] = Protocol;
            }

            builder["Respect Binary Flags"] = RespectBinaryFlags;

            if (!string.IsNullOrEmpty(ServerRsaPublicKeyFile))
            {
                builder["Server RSA Public Key File"] = ServerRsaPublicKeyFile;
            }

            if (!string.IsNullOrEmpty(SharedMemoryName))
            {
                builder["Shared Memory Name"] = SharedMemoryName;
            }

            builder["SSL Mode"] = SslMode ? "Required" : "None";

            if (!string.IsNullOrEmpty(SslCa))
            {
                builder["SSL CA"] = SslCa;
            }

            if (!string.IsNullOrEmpty(SslCert))
            {
                builder["SSL Cert"] = SslCert;
            }

            if (!string.IsNullOrEmpty(SslKey))
            {
                builder["SSL Key"] = SslKey;
            }

            builder["Treat Tiny As Boolean"] = TreatTinyAsBoolean;
            builder["Use Affected Rows"] = UseAffectedRows;
            builder["Use Compression"] = UseCompression;
            builder["Use Default Command Timeout For Read"] = UseDefaultCommandTimeoutForRead;
            builder["Use Performance Monitor"] = UsePerformanceMonitor;

            if (!string.IsNullOrEmpty(UserIdChar))
            {
                builder["User Id Char"] = UserIdChar;
            }

            return builder.ToString();
        }

        private void ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;

            try
            {
                var builder = new System.Data.Common.DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                if (builder.ContainsKey("Server"))
                {
                    Server = builder["Server"].ToString();
                }

                if (builder.ContainsKey("Port"))
                {
                    Port = Convert.ToInt32(builder["Port"]);
                }

                if (builder.ContainsKey("Database"))
                {
                    DatabaseName = builder["Database"].ToString();
                }

                if (builder.ContainsKey("Uid"))
                {
                    UserId = builder["Uid"].ToString();
                }

                if (builder.ContainsKey("Pwd"))
                {
                    Password = builder["Pwd"].ToString();
                }

                if (builder.ContainsKey("Allow Batch"))
                {
                    AllowBatch = Convert.ToBoolean(builder["Allow Batch"]);
                }

                if (builder.ContainsKey("Allow Load Local Infile"))
                {
                    AllowLoadLocalInfile = Convert.ToBoolean(builder["Allow Load Local Infile"]);
                }

                if (builder.ContainsKey("Allow Public Key Retrieval"))
                {
                    AllowPublicKeyRetrieval = Convert.ToBoolean(builder["Allow Public Key Retrieval"]);
                }

                if (builder.ContainsKey("Allow User Variables"))
                {
                    AllowUserVariables = Convert.ToBoolean(builder["Allow User Variables"]);
                }

                if (builder.ContainsKey("Auto Enlist"))
                {
                    AutoEnlist = Convert.ToBoolean(builder["Auto Enlist"]);
                }

                if (builder.ContainsKey("Certificate File"))
                {
                    CertificateFile = builder["Certificate File"].ToString();
                }

                if (builder.ContainsKey("Certificate Password"))
                {
                    CertificatePassword = builder["Certificate Password"].ToString();
                }

                if (builder.ContainsKey("Certificate Store Location"))
                {
                    CertificateStoreLocation = builder["Certificate Store Location"].ToString();
                }

                if (builder.ContainsKey("Certificate Thumbprint"))
                {
                    CertificateThumbprint = builder["Certificate Thumbprint"].ToString();
                }

                if (builder.ContainsKey("Character Set"))
                {
                    CharacterSet = builder["Character Set"].ToString();
                }

                if (builder.ContainsKey("Command Timeout"))
                {
                    CommandTimeout = Convert.ToInt32(builder["Command Timeout"]);
                }

                if (builder.ContainsKey("Connection Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);
                }

                if (builder.ContainsKey("Convert Zero DateTime"))
                {
                    ConvertZeroDateTime = Convert.ToBoolean(builder["Convert Zero DateTime"]);
                }

                if (builder.ContainsKey("Enforce Pooled Connection"))
                {
                    EnforcePooledConnection = Convert.ToBoolean(builder["Enforce Pooled Connection"]);
                }

                if (builder.ContainsKey("Ignore Command Transaction"))
                {
                    IgnoreCommandTransaction = Convert.ToBoolean(builder["Ignore Command Transaction"]);
                }

                if (builder.ContainsKey("Ignore Prepare"))
                {
                    IgnorePrepare = Convert.ToBoolean(builder["Ignore Prepare"]);
                }

                if (builder.ContainsKey("Interactive Session"))
                {
                    InteractiveSession = Convert.ToBoolean(builder["Interactive Session"]);
                }

                if (builder.ContainsKey("Logging"))
                {
                    Logging = Convert.ToBoolean(builder["Logging"]);
                }

                if (builder.ContainsKey("Maximum Pool Size"))
                {
                    MaximumPoolSize = Convert.ToInt32(builder["Maximum Pool Size"]);
                }

                if (builder.ContainsKey("Minimum Pool Size"))
                {
                    MinimumPoolSize = Convert.ToInt32(builder["Minimum Pool Size"]);
                }

                if (builder.ContainsKey("Password Char"))
                {
                    PasswordChar = builder["Password Char"].ToString();
                }

                if (builder.ContainsKey("Pipe Name"))
                {
                    PipeName = builder["Pipe Name"].ToString();
                }

                if (builder.ContainsKey("Pooling"))
                {
                    Pooling = Convert.ToBoolean(builder["Pooling"]);
                }

                if (builder.ContainsKey("Port Number"))
                {
                    PortNumber = Convert.ToInt32(builder["Port Number"]);
                }

                if (builder.ContainsKey("Procedure Cache Size"))
                {
                    ProcedureCacheSize = builder["Procedure Cache Size"].ToString();
                }

                if (builder.ContainsKey("Protocol"))
                {
                    Protocol = builder["Protocol"].ToString();
                }

                if (builder.ContainsKey("Respect Binary Flags"))
                {
                    RespectBinaryFlags = Convert.ToBoolean(builder["Respect Binary Flags"]);
                }

                if (builder.ContainsKey("Server RSA Public Key File"))
                {
                    ServerRsaPublicKeyFile = builder["Server RSA Public Key File"].ToString();
                }

                if (builder.ContainsKey("Shared Memory Name"))
                {
                    SharedMemoryName = builder["Shared Memory Name"].ToString();
                }

                if (builder.ContainsKey("SSL Mode"))
                {
                    SslMode = builder["SSL Mode"].ToString() != "None";
                }

                if (builder.ContainsKey("SSL CA"))
                {
                    SslCa = builder["SSL CA"].ToString();
                }

                if (builder.ContainsKey("SSL Cert"))
                {
                    SslCert = builder["SSL Cert"].ToString();
                }

                if (builder.ContainsKey("SSL Key"))
                {
                    SslKey = builder["SSL Key"].ToString();
                }

                if (builder.ContainsKey("Treat Tiny As Boolean"))
                {
                    TreatTinyAsBoolean = Convert.ToBoolean(builder["Treat Tiny As Boolean"]);
                }

                if (builder.ContainsKey("Use Affected Rows"))
                {
                    UseAffectedRows = Convert.ToBoolean(builder["Use Affected Rows"]);
                }

                if (builder.ContainsKey("Use Compression"))
                {
                    UseCompression = Convert.ToBoolean(builder["Use Compression"]);
                }

                if (builder.ContainsKey("Use Default Command Timeout For Read"))
                {
                    UseDefaultCommandTimeoutForRead = Convert.ToBoolean(builder["Use Default Command Timeout For Read"]);
                }

                if (builder.ContainsKey("Use Performance Monitor"))
                {
                    UsePerformanceMonitor = Convert.ToBoolean(builder["Use Performance Monitor"]);
                }

                if (builder.ContainsKey("User Id Char"))
                {
                    UserIdChar = builder["User Id Char"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"mariadb://{Server}:{Port}/{DatabaseName}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.MariaDB)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
