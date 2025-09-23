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
    public partial class RDBMSConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> rdbmsDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedRdbmsDatabaseType;

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
        string instanceName;

        [ObservableProperty]
        string schemaName;

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
        bool validateConnection;

        [ObservableProperty]
        string additionalParameters;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public RDBMSConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            PackageNames = new List<string>();
            packageVersions = new List<string>();
            rdbmsDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for RDBMS data sources (excluding Oracle which has its own ViewModel)
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x =>
                x.DatasourceCategory == DatasourceCategory.RDBMS &&
                x.DatasourceType != DataSourceType.Oracle))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        PackageNames.Add(item.PackageName);
                        rdbmsDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x =>
                x.DatasourceCategory == DatasourceCategory.RDBMS &&
                x.DatasourceType != DataSourceType.Oracle))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            if (rdbmsDatabaseTypes.Count > 0)
            {
                SelectedRdbmsDatabaseType = rdbmsDatabaseTypes[0];
                SelectedDataSourceType = rdbmsDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            // Filter for RDBMS category excluding Oracle
            Filters.Add(new AppFilter { FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue = Enum.GetName(DatasourceCategory.RDBMS), Operator = "=" });
            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Oracle), Operator = "!=" });
            DBWork.Get(Filters);

            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ValidateConnection = true;
            UseConnectionPooling = true;
            Port = 1433; // Default SQL Server port
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
            Connection.DatabaseType= SelectedDataSourceType;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedRdbmsDatabaseType != null)
            {
                Connection.DriverName = SelectedRdbmsDatabaseType.PackageName;
                Connection.DriverVersion = SelectedRdbmsDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "RDBMS connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"RDBMS connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "RDBMS connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"RDBMS connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
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
            InstanceName = "";
            SchemaName = "";
            Host = "";
            Port = GetDefaultPort(SelectedDataSourceType);
            AdditionalParameters = "";
            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ValidateConnection = true;
            UseConnectionPooling = true;
            UseIntegratedSecurity = false;
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
                SelectedDataSourceType = Connection.DatabaseType;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            if (!string.IsNullOrEmpty(Host))
            {
                if (!string.IsNullOrEmpty(InstanceName))
                {
                    builder["Server"] = $"{Host}\\{InstanceName}";
                }
                else
                {
                    builder["Server"] = $"{Host},{Port}";
                }
            }

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
                builder["Validate Connection"] = ValidateConnection;
            }

            if (!string.IsNullOrEmpty(AdditionalParameters))
            {
                // Parse and add additional parameters
                var additionalBuilder = new System.Data.Common.DbConnectionStringBuilder();
                additionalBuilder.ConnectionString = AdditionalParameters;
                foreach (var key in additionalBuilder.Keys)
                {
                    builder[key.ToString()] = additionalBuilder[key.ToString()];
                }
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
                    var server = builder["Server"].ToString();
                    if (server.Contains("\\"))
                    {
                        // Instance name format: host\instance
                        var parts = server.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            Host = parts[0];
                            InstanceName = parts[1];
                        }
                    }
                    else if (server.Contains(","))
                    {
                        // Host,port format
                        var parts = server.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            Host = parts[0];
                            Port = int.TryParse(parts[1], out var p) ? p : GetDefaultPort(SelectedDataSourceType);
                        }
                    }
                    else
                    {
                        Host = server;
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

                if (builder.ContainsKey("Validate Connection"))
                {
                    ValidateConnection = Convert.ToBoolean(builder["Validate Connection"]);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"rdbms://{Host}:{Port}/{DatabaseName}";
        }

        private int GetDefaultPort(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.SqlServer:
                    return 1433;
                case DataSourceType.Mysql:
                    return 3306;
                case DataSourceType.Postgre:
                    return 5432;
                case DataSourceType.DB2:
                    return 50000;
                case DataSourceType.SnowFlake:
                    return 443;
                case DataSourceType.Hana:
                    return 39015;
                case DataSourceType.AzureSQL:
                    return 1433;
                default:
                    return 1433;
            }
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<AssemblyClassDefinition>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x =>
                x.DatasourceCategory == DatasourceCategory.RDBMS &&
                x.DatasourceType != DataSourceType.Oracle))
            {
                installedDataSources.Add(new AssemblyClassDefinition
                {
                    className = item.classHandler,
                    PackageName = item.PackageName,
                    Version = item.version
                });
            }
        }
    }
}
