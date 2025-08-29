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
    public partial class SqlServerConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> sqlServerDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedSqlServerDatabaseType;

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
        string serverName;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string instanceName;

        [ObservableProperty]
        bool useIntegratedSecurity;

        [ObservableProperty]
        bool useConnectionPooling;

        [ObservableProperty]
        int minPoolSize;

        [ObservableProperty]
        int maxPoolSize;

        [ObservableProperty]
        int connectionLifetime;

        [ObservableProperty]
        bool encryptConnection;

        [ObservableProperty]
        bool trustServerCertificate;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        string applicationName;

        [ObservableProperty]
        string workstationId;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public SqlServerConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            sqlServerDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.SqlServer)
                {
                    sqlServerDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.SqlServer))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (sqlServerDatabaseTypes.Count > 0)
            {
                SelectedSqlServerDatabaseType = sqlServerDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.SqlServer), Operator = "=" });
            DBWork.Get(Filters);

            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ConnectionTimeout = 30;
            UseConnectionPooling = true;
            EncryptConnection = false;
            TrustServerCertificate = true;
            Port = 1433;
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
            Connection.DatabaseType= DataSourceType.SqlServer;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedSqlServerDatabaseType != null)
            {
                Connection.DriverName = SelectedSqlServerDatabaseType.PackageName;
                Connection.DriverVersion = SelectedSqlServerDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "SQL Server connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"SQL Server connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "SQL Server connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"SQL Server connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
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
            ServerName = "localhost";
            InstanceName = "";
            Port = 1433;
            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ConnectionTimeout = 30;
            UseConnectionPooling = true;
            UseIntegratedSecurity = false;
            EncryptConnection = false;
            TrustServerCertificate = true;
            ApplicationName = "Beep Application";
            WorkstationId = "";
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

            // Build server name with instance if specified
            string serverPart = ServerName;
            if (!string.IsNullOrEmpty(InstanceName))
            {
                serverPart = $"{ServerName}\\{InstanceName}";
            }
            else if (Port != 1433)
            {
                serverPart = $"{ServerName},{Port}";
            }

            builder["Server"] = serverPart;

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                builder["Database"] = DatabaseName;
            }

            if (UseIntegratedSecurity)
            {
                builder["Integrated Security"] = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(UserId))
                {
                    builder["User Id"] = UserId;
                }
                if (!string.IsNullOrEmpty(Password))
                {
                    builder["Password"] = Password;
                }
            }

            if (UseConnectionPooling)
            {
                builder["Pooling"] = true;
                builder["Min Pool Size"] = MinPoolSize;
                builder["Max Pool Size"] = MaxPoolSize;
                builder["Connection Lifetime"] = ConnectionLifetime;
            }

            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Encrypt"] = EncryptConnection;
            builder["TrustServerCertificate"] = TrustServerCertificate;

            if (!string.IsNullOrEmpty(ApplicationName))
            {
                builder["Application Name"] = ApplicationName;
            }

            if (!string.IsNullOrEmpty(WorkstationId))
            {
                builder["Workstation ID"] = WorkstationId;
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
                    string serverValue = builder["Server"].ToString();
                    if (serverValue.Contains("\\"))
                    {
                        // Named instance format: server\instance
                        var parts = serverValue.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                        ServerName = parts[0];
                        InstanceName = parts.Length > 1 ? parts[1] : "";
                    }
                    else if (serverValue.Contains(","))
                    {
                        // Server with port format: server,port
                        var parts = serverValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        ServerName = parts[0];
                        Port = int.TryParse(parts[1], out var p) ? p : 1433;
                    }
                    else
                    {
                        ServerName = serverValue;
                    }
                }

                if (builder.ContainsKey("Database"))
                {
                    DatabaseName = builder["Database"].ToString();
                }

                if (builder.ContainsKey("User Id"))
                {
                    UserId = builder["User Id"].ToString();
                    UseIntegratedSecurity = false;
                }

                if (builder.ContainsKey("Integrated Security"))
                {
                    UseIntegratedSecurity = Convert.ToBoolean(builder["Integrated Security"]);
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("Pooling"))
                {
                    UseConnectionPooling = Convert.ToBoolean(builder["Pooling"]);
                }

                if (builder.ContainsKey("Min Pool Size"))
                {
                    MinPoolSize = Convert.ToInt32(builder["Min Pool Size"]);
                }

                if (builder.ContainsKey("Max Pool Size"))
                {
                    MaxPoolSize = Convert.ToInt32(builder["Max Pool Size"]);
                }

                if (builder.ContainsKey("Connection Lifetime"))
                {
                    ConnectionLifetime = Convert.ToInt32(builder["Connection Lifetime"]);
                }

                if (builder.ContainsKey("Connection Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);
                }

                if (builder.ContainsKey("Encrypt"))
                {
                    EncryptConnection = Convert.ToBoolean(builder["Encrypt"]);
                }

                if (builder.ContainsKey("TrustServerCertificate"))
                {
                    TrustServerCertificate = Convert.ToBoolean(builder["TrustServerCertificate"]);
                }

                if (builder.ContainsKey("Application Name"))
                {
                    ApplicationName = builder["Application Name"].ToString();
                }

                if (builder.ContainsKey("Workstation ID"))
                {
                    WorkstationId = builder["Workstation ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            string serverPart = ServerName;
            if (!string.IsNullOrEmpty(InstanceName))
            {
                serverPart = $"{ServerName}\\{InstanceName}";
            }
            else if (Port != 1433)
            {
                serverPart = $"{ServerName}:{Port}";
            }

            return $"sqlserver://{serverPart}/{DatabaseName}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.SqlServer)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
