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
    public partial class RedisConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> redisDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedRedisDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string database;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        bool ssl;

        [ObservableProperty]
        string sslCertificate;

        [ObservableProperty]
        string sslKey;

        [ObservableProperty]
        string sslCaCertificate;

        [ObservableProperty]
        bool sslHostnameValidation;

        [ObservableProperty]
        int connectTimeout;

        [ObservableProperty]
        int syncTimeout;

        [ObservableProperty]
        int connectRetry;

        [ObservableProperty]
        int keepAlive;

        [ObservableProperty]
        bool abortOnConnectFail;

        [ObservableProperty]
        string clientName;

        [ObservableProperty]
        bool allowAdmin;

        [ObservableProperty]
        bool highPriorityThreads;

        [ObservableProperty]
        string tieBreaker;

        [ObservableProperty]
        string configurationChannel;

        [ObservableProperty]
        string passwordFile;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public RedisConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            PackageNames = new List<string>();
            packageVersions = new List<string>();
            redisDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Redis)
                {
                    redisDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.Redis))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    PackageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (redisDatabaseTypes.Count > 0)
            {
                SelectedRedisDatabaseType = redisDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.NOSQL;
            SelectedCategoryValue = (int)DatasourceCategory.NOSQL;
            SelectedCategoryTextValue = DatasourceCategory.NOSQL.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Redis), Operator = "=" });
            DBWork.Get(Filters);

            Port = 6379;
            ConnectTimeout = 5000;
            SyncTimeout = 1000;
            ConnectRetry = 3;
            KeepAlive = 60;
            AbortOnConnectFail = true;
            AllowAdmin = false;
            HighPriorityThreads = true;
            Ssl = false;
            SslHostnameValidation = true;
            ClientName = "Beep Application";
        }

        [RelayCommand]
        public void SaveConnection()
        {
            if (Connection == null)
            {
                Connection = new ConnectionProperties();
            }
            Connection.ConnectionName = CurrentDataSourceName;
            Connection.Database = Database;
            Connection.UserID = "";
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= DataSourceType.Redis;
            Connection.Category = DatasourceCategory.NOSQL;
            if (SelectedRedisDatabaseType != null)
            {
                Connection.DriverName = SelectedRedisDatabaseType.PackageName;
                Connection.DriverVersion = SelectedRedisDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "Redis");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            Database = "0";
            Password = "";
            Host = "localhost";
            Port = 6379;
            Ssl = false;
            SslCertificate = "";
            SslKey = "";
            SslCaCertificate = "";
            SslHostnameValidation = true;
            ConnectTimeout = 5000;
            SyncTimeout = 1000;
            ConnectRetry = 3;
            KeepAlive = 60;
            AbortOnConnectFail = true;
            ClientName = "Beep Application";
            AllowAdmin = false;
            HighPriorityThreads = true;
            TieBreaker = "";
            ConfigurationChannel = "";
            PasswordFile = "";
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                Database = Connection.Database;
                Password = Connection.Password;
                ParseConnectionString(Connection.ConnectionString);
                SelectedconnectionGuid = Connection.GuidID;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            builder["Host"] = Host;
            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(Database))
            {
                builder["Database"] = Database;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            builder["SSL"] = Ssl;

            if (Ssl)
            {
                if (!string.IsNullOrEmpty(SslCertificate))
                {
                    builder["SSL Certificate"] = SslCertificate;
                }

                if (!string.IsNullOrEmpty(SslKey))
                {
                    builder["SSL Key"] = SslKey;
                }

                if (!string.IsNullOrEmpty(SslCaCertificate))
                {
                    builder["SSL CA Certificate"] = SslCaCertificate;
                }

                builder["SSL Hostname Validation"] = SslHostnameValidation;
            }

            builder["Connect Timeout"] = ConnectTimeout;
            builder["Sync Timeout"] = SyncTimeout;
            builder["Connect Retry"] = ConnectRetry;
            builder["Keep Alive"] = KeepAlive;
            builder["Abort On Connect Fail"] = AbortOnConnectFail;

            if (!string.IsNullOrEmpty(ClientName))
            {
                builder["Client Name"] = ClientName;
            }

            builder["Allow Admin"] = AllowAdmin;
            builder["High Priority Threads"] = HighPriorityThreads;

            if (!string.IsNullOrEmpty(TieBreaker))
            {
                builder["Tie Breaker"] = TieBreaker;
            }

            if (!string.IsNullOrEmpty(ConfigurationChannel))
            {
                builder["Configuration Channel"] = ConfigurationChannel;
            }

            if (!string.IsNullOrEmpty(PasswordFile))
            {
                builder["Password File"] = PasswordFile;
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

                if (builder.ContainsKey("Host"))
                {
                    Host = builder["Host"].ToString();
                }

                if (builder.ContainsKey("Port"))
                {
                    Port = Convert.ToInt32(builder["Port"]);
                }

                if (builder.ContainsKey("Database"))
                {
                    Database = builder["Database"].ToString();
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("SSL"))
                {
                    Ssl = Convert.ToBoolean(builder["SSL"]);
                }

                if (builder.ContainsKey("SSL Certificate"))
                {
                    SslCertificate = builder["SSL Certificate"].ToString();
                }

                if (builder.ContainsKey("SSL Key"))
                {
                    SslKey = builder["SSL Key"].ToString();
                }

                if (builder.ContainsKey("SSL CA Certificate"))
                {
                    SslCaCertificate = builder["SSL CA Certificate"].ToString();
                }

                if (builder.ContainsKey("SSL Hostname Validation"))
                {
                    SslHostnameValidation = Convert.ToBoolean(builder["SSL Hostname Validation"]);
                }

                if (builder.ContainsKey("Connect Timeout"))
                {
                    ConnectTimeout = Convert.ToInt32(builder["Connect Timeout"]);
                }

                if (builder.ContainsKey("Sync Timeout"))
                {
                    SyncTimeout = Convert.ToInt32(builder["Sync Timeout"]);
                }

                if (builder.ContainsKey("Connect Retry"))
                {
                    ConnectRetry = Convert.ToInt32(builder["Connect Retry"]);
                }

                if (builder.ContainsKey("Keep Alive"))
                {
                    KeepAlive = Convert.ToInt32(builder["Keep Alive"]);
                }

                if (builder.ContainsKey("Abort On Connect Fail"))
                {
                    AbortOnConnectFail = Convert.ToBoolean(builder["Abort On Connect Fail"]);
                }

                if (builder.ContainsKey("Client Name"))
                {
                    ClientName = builder["Client Name"].ToString();
                }

                if (builder.ContainsKey("Allow Admin"))
                {
                    AllowAdmin = Convert.ToBoolean(builder["Allow Admin"]);
                }

                if (builder.ContainsKey("High Priority Threads"))
                {
                    HighPriorityThreads = Convert.ToBoolean(builder["High Priority Threads"]);
                }

                if (builder.ContainsKey("Tie Breaker"))
                {
                    TieBreaker = builder["Tie Breaker"].ToString();
                }

                if (builder.ContainsKey("Configuration Channel"))
                {
                    ConfigurationChannel = builder["Configuration Channel"].ToString();
                }

                if (builder.ContainsKey("Password File"))
                {
                    PasswordFile = builder["Password File"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"redis://{Host}:{Port}/{Database}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Redis)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
