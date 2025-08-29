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
    public partial class CassandraConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> cassandraDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedCassandraDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string keyspace;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        string userId;

        [ObservableProperty]
        string contactPoints;

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
        int connectionTimeout;

        [ObservableProperty]
        int readTimeout;

        [ObservableProperty]
        int maxConnectionsPerHost;

        [ObservableProperty]
        int coreConnectionsPerHost;

        [ObservableProperty]
        int maxRequestsPerConnection;

        [ObservableProperty]
        string consistencyLevel;

        [ObservableProperty]
        string loadBalancingPolicy;

        [ObservableProperty]
        string reconnectionPolicy;

        [ObservableProperty]
        string retryPolicy;

        [ObservableProperty]
        string applicationName;

        [ObservableProperty]
        bool useCompression;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public CassandraConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            cassandraDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Cassandra)
                {
                    cassandraDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.Cassandra))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (cassandraDatabaseTypes.Count > 0)
            {
                SelectedCassandraDatabaseType = cassandraDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.NOSQL;
            SelectedCategoryValue = (int)DatasourceCategory.NOSQL;
            SelectedCategoryTextValue = DatasourceCategory.NOSQL.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Cassandra), Operator = "=" });
            DBWork.Get(Filters);

            Port = 9042;
            ConnectionTimeout = 5000;
            ReadTimeout = 12000;
            MaxConnectionsPerHost = 8;
            CoreConnectionsPerHost = 2;
            MaxRequestsPerConnection = 1024;
            ConsistencyLevel = "LOCAL_QUORUM";
            LoadBalancingPolicy = "DefaultLoadBalancingPolicy";
            ReconnectionPolicy = "ExponentialReconnectionPolicy";
            RetryPolicy = "DefaultRetryPolicy";
            ApplicationName = "Beep Application";
            Ssl = false;
            SslHostnameValidation = true;
            UseCompression = false;
        }

        [RelayCommand]
        public void SaveConnection()
        {
            if (Connection == null)
            {
                Connection = new ConnectionProperties();
            }
            Connection.ConnectionName = CurrentDataSourceName;
            Connection.Database = Keyspace;
            Connection.UserID = UserId;
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType = DataSourceType.Cassandra;
            Connection.Category = DatasourceCategory.NOSQL;
            if (SelectedCassandraDatabaseType != null)
            {
                Connection.DriverName = SelectedCassandraDatabaseType.PackageName;
                Connection.DriverVersion = SelectedCassandraDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "Cassandra");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            Keyspace = "";
            UserId = "";
            Password = "";
            ContactPoints = "localhost";
            Port = 9042;
            Ssl = false;
            SslCertificate = "";
            SslKey = "";
            SslCaCertificate = "";
            SslHostnameValidation = true;
            ConnectionTimeout = 5000;
            ReadTimeout = 12000;
            MaxConnectionsPerHost = 8;
            CoreConnectionsPerHost = 2;
            MaxRequestsPerConnection = 1024;
            ConsistencyLevel = "LOCAL_QUORUM";
            LoadBalancingPolicy = "DefaultLoadBalancingPolicy";
            ReconnectionPolicy = "ExponentialReconnectionPolicy";
            RetryPolicy = "DefaultRetryPolicy";
            ApplicationName = "Beep Application";
            UseCompression = false;
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                Keyspace = Connection.Database;
                UserId = Connection.UserID;
                Password = Connection.Password;
                ParseConnectionString(Connection.ConnectionString);
                SelectedconnectionGuid = Connection.GuidID;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            builder["Contact Points"] = ContactPoints;
            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(Keyspace))
            {
                builder["Default Keyspace"] = Keyspace;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["Username"] = UserId;
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

            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Read Timeout"] = ReadTimeout;
            builder["Max Connections Per Host"] = MaxConnectionsPerHost;
            builder["Core Connections Per Host"] = CoreConnectionsPerHost;
            builder["Max Requests Per Connection"] = MaxRequestsPerConnection;

            if (!string.IsNullOrEmpty(ConsistencyLevel))
            {
                builder["Consistency Level"] = ConsistencyLevel;
            }

            if (!string.IsNullOrEmpty(LoadBalancingPolicy))
            {
                builder["Load Balancing Policy"] = LoadBalancingPolicy;
            }

            if (!string.IsNullOrEmpty(ReconnectionPolicy))
            {
                builder["Reconnection Policy"] = ReconnectionPolicy;
            }

            if (!string.IsNullOrEmpty(RetryPolicy))
            {
                builder["Retry Policy"] = RetryPolicy;
            }

            if (!string.IsNullOrEmpty(ApplicationName))
            {
                builder["Application Name"] = ApplicationName;
            }

            builder["Compression"] = UseCompression ? "LZ4" : "NoCompression";

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

                if (builder.ContainsKey("Contact Points"))
                {
                    ContactPoints = builder["Contact Points"].ToString();
                }

                if (builder.ContainsKey("Port"))
                {
                    Port = Convert.ToInt32(builder["Port"]);
                }

                if (builder.ContainsKey("Default Keyspace"))
                {
                    Keyspace = builder["Default Keyspace"].ToString();
                }

                if (builder.ContainsKey("Username"))
                {
                    UserId = builder["Username"].ToString();
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

                if (builder.ContainsKey("Connection Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);
                }

                if (builder.ContainsKey("Read Timeout"))
                {
                    ReadTimeout = Convert.ToInt32(builder["Read Timeout"]);
                }

                if (builder.ContainsKey("Max Connections Per Host"))
                {
                    MaxConnectionsPerHost = Convert.ToInt32(builder["Max Connections Per Host"]);
                }

                if (builder.ContainsKey("Core Connections Per Host"))
                {
                    CoreConnectionsPerHost = Convert.ToInt32(builder["Core Connections Per Host"]);
                }

                if (builder.ContainsKey("Max Requests Per Connection"))
                {
                    MaxRequestsPerConnection = Convert.ToInt32(builder["Max Requests Per Connection"]);
                }

                if (builder.ContainsKey("Consistency Level"))
                {
                    ConsistencyLevel = builder["Consistency Level"].ToString();
                }

                if (builder.ContainsKey("Load Balancing Policy"))
                {
                    LoadBalancingPolicy = builder["Load Balancing Policy"].ToString();
                }

                if (builder.ContainsKey("Reconnection Policy"))
                {
                    ReconnectionPolicy = builder["Reconnection Policy"].ToString();
                }

                if (builder.ContainsKey("Retry Policy"))
                {
                    RetryPolicy = builder["Retry Policy"].ToString();
                }

                if (builder.ContainsKey("Application Name"))
                {
                    ApplicationName = builder["Application Name"].ToString();
                }

                if (builder.ContainsKey("Compression"))
                {
                    UseCompression = builder["Compression"].ToString() != "NoCompression";
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"cassandra://{ContactPoints}:{Port}/{Keyspace}";
        }

        private void GetInstallDataSources()
        {
            InstalledDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Cassandra)
                {
                    InstalledDataSources.Add(item);
                }
            }
        }
    }
}
