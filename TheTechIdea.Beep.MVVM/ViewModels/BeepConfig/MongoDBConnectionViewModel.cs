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
    public partial class MongoDBConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> mongoDBDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedMongoDBDatabaseType;

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
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string replicaSet;

        [ObservableProperty]
        bool ssl;

        [ObservableProperty]
        string sslCertificate;

        [ObservableProperty]
        string sslCertificatePassword;

        [ObservableProperty]
        bool allowInvalidCertificates;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        int serverSelectionTimeout;

        [ObservableProperty]
        int maxPoolSize;

        [ObservableProperty]
        int minPoolSize;

        [ObservableProperty]
        bool retryWrites;

        [ObservableProperty]
        string authenticationDatabase;

        [ObservableProperty]
        string authenticationMechanism;

        [ObservableProperty]
        string applicationName;

        [ObservableProperty]
        bool directConnection;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public MongoDBConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            mongoDBDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.MongoDB)
                {
                    mongoDBDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.MongoDB))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (mongoDBDatabaseTypes.Count > 0)
            {
                SelectedMongoDBDatabaseType = mongoDBDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.NOSQL;
            SelectedCategoryValue = (int)DatasourceCategory.NOSQL;
            SelectedCategoryTextValue = DatasourceCategory.NOSQL.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = DataSourceType.MongoDB, Operator = "=" });
            DBWork.Get(Filters);

            Port = 27017;
            ConnectionTimeout = 5000;
            ServerSelectionTimeout = 5000;
            MaxPoolSize = 100;
            MinPoolSize = 0;
            RetryWrites = true;
            Ssl = false;
            AllowInvalidCertificates = false;
            DirectConnection = false;
            AuthenticationMechanism = "SCRAM-SHA-256";
            ApplicationName = "Beep Application";
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
            Connection.DatabaseType= DataSourceType.MongoDB;
            Connection.Category = DatasourceCategory.NOSQL;
            if (SelectedMongoDBDatabaseType != null)
            {
                Connection.DriverName = SelectedMongoDBDatabaseType.PackageName;
                Connection.DriverVersion = SelectedMongoDBDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "MongoDB");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            DatabaseName = "";
            UserId = "";
            Password = "";
            Host = "localhost";
            Port = 27017;
            ReplicaSet = "";
            Ssl = false;
            SslCertificate = "";
            SslCertificatePassword = "";
            AllowInvalidCertificates = false;
            ConnectionTimeout = 5000;
            ServerSelectionTimeout = 5000;
            MaxPoolSize = 100;
            MinPoolSize = 0;
            RetryWrites = true;
            AuthenticationDatabase = "";
            AuthenticationMechanism = "SCRAM-SHA-256";
            ApplicationName = "Beep Application";
            DirectConnection = false;
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

            string connectionString = "mongodb://";

            if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Password))
            {
                connectionString += $"{Uri.EscapeDataString(UserId)}:{Uri.EscapeDataString(Password)}@";
            }

            connectionString += $"{Host}:{Port}";

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                connectionString += $"/{DatabaseName}";
            }

            List<string> parameters = new List<string>();

            if (!string.IsNullOrEmpty(ReplicaSet))
            {
                parameters.Add($"replicaSet={ReplicaSet}");
            }

            if (Ssl)
            {
                parameters.Add("ssl=true");
                if (!string.IsNullOrEmpty(SslCertificate))
                {
                    parameters.Add($"ssl_cert_file={SslCertificate}");
                }
                if (!string.IsNullOrEmpty(SslCertificatePassword))
                {
                    parameters.Add($"ssl_cert_password={SslCertificatePassword}");
                }
                if (AllowInvalidCertificates)
                {
                    parameters.Add("ssl_verify=false");
                }
            }

            parameters.Add($"connectTimeoutMS={ConnectionTimeout}");
            parameters.Add($"serverSelectionTimeoutMS={ServerSelectionTimeout}");
            parameters.Add($"maxPoolSize={MaxPoolSize}");
            parameters.Add($"minPoolSize={MinPoolSize}");
            parameters.Add($"retryWrites={RetryWrites}");

            if (!string.IsNullOrEmpty(AuthenticationDatabase))
            {
                parameters.Add($"authSource={AuthenticationDatabase}");
            }

            if (!string.IsNullOrEmpty(AuthenticationMechanism))
            {
                parameters.Add($"authMechanism={AuthenticationMechanism}");
            }

            if (!string.IsNullOrEmpty(ApplicationName))
            {
                parameters.Add($"appName={Uri.EscapeDataString(ApplicationName)}");
            }

            if (DirectConnection)
            {
                parameters.Add("directConnection=true");
            }

            if (parameters.Count > 0)
            {
                connectionString += "?" + string.Join("&", parameters);
            }

            return connectionString;
        }

        private void ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;

            try
            {
                var uri = new Uri(connectionString);

                Host = uri.Host;
                Port = uri.Port;

                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var userInfo = uri.UserInfo.Split(':');
                    if (userInfo.Length >= 1)
                    {
                        UserId = Uri.UnescapeDataString(userInfo[0]);
                    }
                    if (userInfo.Length >= 2)
                    {
                        Password = Uri.UnescapeDataString(userInfo[1]);
                    }
                }

                var path = uri.AbsolutePath.TrimStart('/');
                if (!string.IsNullOrEmpty(path))
                {
                    DatabaseName = path;
                }

                var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);

                if (!string.IsNullOrEmpty(queryParameters["replicaSet"]))
                {
                    ReplicaSet = queryParameters["replicaSet"];
                }

                if (!string.IsNullOrEmpty(queryParameters["ssl"]))
                {
                    Ssl = queryParameters["ssl"] == "true";
                }

                if (!string.IsNullOrEmpty(queryParameters["ssl_cert_file"]))
                {
                    SslCertificate = queryParameters["ssl_cert_file"];
                }

                if (!string.IsNullOrEmpty(queryParameters["ssl_cert_password"]))
                {
                    SslCertificatePassword = queryParameters["ssl_cert_password"];
                }

                if (!string.IsNullOrEmpty(queryParameters["ssl_verify"]))
                {
                    AllowInvalidCertificates = queryParameters["ssl_verify"] == "false";
                }

                if (!string.IsNullOrEmpty(queryParameters["connectTimeoutMS"]))
                {
                    ConnectionTimeout = int.Parse(queryParameters["connectTimeoutMS"]);
                }

                if (!string.IsNullOrEmpty(queryParameters["serverSelectionTimeoutMS"]))
                {
                    ServerSelectionTimeout = int.Parse(queryParameters["serverSelectionTimeoutMS"]);
                }

                if (!string.IsNullOrEmpty(queryParameters["maxPoolSize"]))
                {
                    MaxPoolSize = int.Parse(queryParameters["maxPoolSize"]);
                }

                if (!string.IsNullOrEmpty(queryParameters["minPoolSize"]))
                {
                    MinPoolSize = int.Parse(queryParameters["minPoolSize"]);
                }

                if (!string.IsNullOrEmpty(queryParameters["retryWrites"]))
                {
                    RetryWrites = queryParameters["retryWrites"] == "true";
                }

                if (!string.IsNullOrEmpty(queryParameters["authSource"]))
                {
                    AuthenticationDatabase = queryParameters["authSource"];
                }

                if (!string.IsNullOrEmpty(queryParameters["authMechanism"]))
                {
                    AuthenticationMechanism = queryParameters["authMechanism"];
                }

                if (!string.IsNullOrEmpty(queryParameters["appName"]))
                {
                    ApplicationName = Uri.UnescapeDataString(queryParameters["appName"]);
                }

                if (!string.IsNullOrEmpty(queryParameters["directConnection"]))
                {
                    DirectConnection = queryParameters["directConnection"] == "true";
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"mongodb://{Host}:{Port}/{DatabaseName}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.MongoDB)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
