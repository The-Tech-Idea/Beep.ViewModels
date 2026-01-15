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
using System.Data;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class PostgreSqlConnectionViewModel : DataConnectionViewModel
    {
        [ObservableProperty]
        List<ConnectionDriversConfig> postgreSqlDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedPostgreSqlDatabaseType;

        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string schema;

        [ObservableProperty]
        bool sslMode;

        [ObservableProperty]
        string sslCertificate;

        [ObservableProperty]
        string sslKey;

        [ObservableProperty]
        string sslRootCertificate;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        int commandTimeout;

        [ObservableProperty]
        bool pooling;

        [ObservableProperty]
        int minPoolSize;

        [ObservableProperty]
        int maxPoolSize;

        [ObservableProperty]
        string searchPath;

        [ObservableProperty]
        string applicationName;

        [ObservableProperty]
        bool includeErrorDetail;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public PostgreSqlConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            postgreSqlDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Postgre)
                {
                    postgreSqlDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.Postgre))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    PackageNames.Add(item.PackageName);
                    PackageVersions.Add(item.version);
                }
            }

            if (postgreSqlDatabaseTypes.Count > 0)
            {
                SelectedPostgreSqlDatabaseType = postgreSqlDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            Filters.Add(new AppFilter {FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Postgre), Operator = "=" });
            DBWork.Get(Filters);

            Port = 5432;
            ConnectionTimeout = 15;
            CommandTimeout = 20;
            Pooling = true;
            MinPoolSize = 1;
            MaxPoolSize = 20;
            SslMode = false;
            IncludeErrorDetail = false;
            Schema = "public";
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
            Connection.DatabaseType= DataSourceType.Postgre;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedPostgreSqlDatabaseType != null)
            {
                Connection.DriverName = SelectedPostgreSqlDatabaseType.PackageName;
                Connection.DriverVersion = SelectedPostgreSqlDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                try
                {
                    var dataSource = Editor.CreateNewDataSourceConnection(Connection, Connection.ConnectionName);
                    if (dataSource != null)
                    {
                        var connectionState = dataSource.Openconnection();
                        if (connectionState == ConnectionState.Open)
                        {
                            Editor.AddLogMessage("Beep", "PostgreSQL connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"PostgreSQL connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "PostgreSQL connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"PostgreSQL connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            DatabaseName = "";
            UserId = "postgres";
            Password = "";
            Host = "localhost";
            Port = 5432;
            Schema = "public";
            SslMode = false;
            SslCertificate = "";
            SslKey = "";
            SslRootCertificate = "";
            ConnectionTimeout = 15;
            CommandTimeout = 20;
            Pooling = true;
            MinPoolSize = 1;
            MaxPoolSize = 20;
            SearchPath = "";
            ApplicationName = "Beep Application";
            IncludeErrorDetail = false;
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

            if (!string.IsNullOrEmpty(Host))
            {
                builder["Host"] = Host;
            }

            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                builder["Database"] = DatabaseName;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["Username"] = UserId;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            builder["SSL Mode"] = SslMode ? "Require" : "Disable";

            if (!string.IsNullOrEmpty(SslCertificate))
            {
                builder["SSL Certificate"] = SslCertificate;
            }

            if (!string.IsNullOrEmpty(SslKey))
            {
                builder["SSL Key"] = SslKey;
            }

            if (!string.IsNullOrEmpty(SslRootCertificate))
            {
                builder["Root Certificate"] = SslRootCertificate;
            }

            builder["Timeout"] = ConnectionTimeout;
            builder["Command Timeout"] = CommandTimeout;
            builder["Pooling"] = Pooling;

            if (Pooling)
            {
                builder["Minimum Pool Size"] = MinPoolSize;
                builder["Maximum Pool Size"] = MaxPoolSize;
            }

            if (!string.IsNullOrEmpty(SearchPath))
            {
                builder["Search Path"] = SearchPath;
            }

            if (!string.IsNullOrEmpty(ApplicationName))
            {
                builder["Application Name"] = ApplicationName;
            }

            builder["Include Error Detail"] = IncludeErrorDetail;

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
                    DatabaseName = builder["Database"].ToString();
                }

                if (builder.ContainsKey("Username"))
                {
                    UserId = builder["Username"].ToString();
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("SSL Mode"))
                {
                    SslMode = builder["SSL Mode"].ToString() != "Disable";
                }

                if (builder.ContainsKey("SSL Certificate"))
                {
                    SslCertificate = builder["SSL Certificate"].ToString();
                }

                if (builder.ContainsKey("SSL Key"))
                {
                    SslKey = builder["SSL Key"].ToString();
                }

                if (builder.ContainsKey("Root Certificate"))
                {
                    SslRootCertificate = builder["Root Certificate"].ToString();
                }

                if (builder.ContainsKey("Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Timeout"]);
                }

                if (builder.ContainsKey("Command Timeout"))
                {
                    CommandTimeout = Convert.ToInt32(builder["Command Timeout"]);
                }

                if (builder.ContainsKey("Pooling"))
                {
                    Pooling = Convert.ToBoolean(builder["Pooling"]);
                }

                if (builder.ContainsKey("Minimum Pool Size"))
                {
                    MinPoolSize = Convert.ToInt32(builder["Minimum Pool Size"]);
                }

                if (builder.ContainsKey("Maximum Pool Size"))
                {
                    MaxPoolSize = Convert.ToInt32(builder["Maximum Pool Size"]);
                }

                if (builder.ContainsKey("Search Path"))
                {
                    SearchPath = builder["Search Path"].ToString();
                }

                if (builder.ContainsKey("Application Name"))
                {
                    ApplicationName = builder["Application Name"].ToString();
                }

                if (builder.ContainsKey("Include Error Detail"))
                {
                    IncludeErrorDetail = Convert.ToBoolean(builder["Include Error Detail"]);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"postgresql://{Host}:{Port}/{DatabaseName}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Postgre)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
