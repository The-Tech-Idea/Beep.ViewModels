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
using System.Threading.Tasks;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class InMemoryConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> inMemoryDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedInMemoryDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string databaseName;

        [ObservableProperty]
        int databaseIndex;

        [ObservableProperty]
        bool useSSL;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        string filePath;

        [ObservableProperty]
        bool enablePersistence;

        [ObservableProperty]
        string persistencePath;

        [ObservableProperty]
        int maxMemory;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        bool enableCompression;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public InMemoryConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            inMemoryDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for INMEMORY data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.INMEMORY))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        inMemoryDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.INMEMORY))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            if (inMemoryDatabaseTypes.Count > 0)
            {
                SelectedInMemoryDatabaseType = inMemoryDatabaseTypes[0];
                SelectedDataSourceType = inMemoryDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.INMEMORY;
            SelectedCategoryValue = (int)DatasourceCategory.INMEMORY;
            SelectedCategoryTextValue = DatasourceCategory.INMEMORY.ToString();

            // Filter for INMEMORY category
            Filters.Add(new AppFilter { FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue = DatasourceCategory.INMEMORY, Operator = "=" });
            DBWork.Get(Filters);

            Port = GetDefaultPort(SelectedDataSourceType);
            DatabaseIndex = 0;
            UseSSL = false;
            EnablePersistence = false;
            MaxMemory = 512; // MB
            ConnectionTimeout = 5000;
            EnableCompression = false;
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
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= SelectedDataSourceType;
            Connection.Category = DatasourceCategory.INMEMORY;
            if (SelectedInMemoryDatabaseType != null)
            {
                Connection.DriverName = SelectedInMemoryDatabaseType.PackageName;
                Connection.DriverVersion = SelectedInMemoryDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "In-Memory connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"In-Memory connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "In-Memory connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"In-Memory connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            Host = "localhost";
            Port = GetDefaultPort(SelectedDataSourceType);
            Password = "";
            DatabaseName = "";
            DatabaseIndex = 0;
            UseSSL = false;
            FilePath = "";
            EnablePersistence = false;
            PersistencePath = "";
            MaxMemory = 512;
            ConnectionTimeout = 5000;
            EnableCompression = false;
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                DatabaseName = Connection.Database;
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
                builder["Host"] = Host;
            }

            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                builder["Database"] = DatabaseName;
            }

            builder["DatabaseIndex"] = DatabaseIndex;
            builder["UseSSL"] = UseSSL;
            builder["EnablePersistence"] = EnablePersistence;

            if (!string.IsNullOrEmpty(PersistencePath))
            {
                builder["PersistencePath"] = PersistencePath;
            }

            if (!string.IsNullOrEmpty(FilePath))
            {
                builder["FilePath"] = FilePath;
            }

            builder["MaxMemory"] = MaxMemory;
            builder["ConnectionTimeout"] = ConnectionTimeout;
            builder["EnableCompression"] = EnableCompression;

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

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("Database"))
                {
                    DatabaseName = builder["Database"].ToString();
                }

                if (builder.ContainsKey("DatabaseIndex"))
                {
                    DatabaseIndex = Convert.ToInt32(builder["DatabaseIndex"]);
                }

                if (builder.ContainsKey("UseSSL"))
                {
                    UseSSL = Convert.ToBoolean(builder["UseSSL"]);
                }

                if (builder.ContainsKey("EnablePersistence"))
                {
                    EnablePersistence = Convert.ToBoolean(builder["EnablePersistence"]);
                }

                if (builder.ContainsKey("PersistencePath"))
                {
                    PersistencePath = builder["PersistencePath"].ToString();
                }

                if (builder.ContainsKey("FilePath"))
                {
                    FilePath = builder["FilePath"].ToString();
                }

                if (builder.ContainsKey("MaxMemory"))
                {
                    MaxMemory = Convert.ToInt32(builder["MaxMemory"]);
                }

                if (builder.ContainsKey("ConnectionTimeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["ConnectionTimeout"]);
                }

                if (builder.ContainsKey("EnableCompression"))
                {
                    EnableCompression = Convert.ToBoolean(builder["EnableCompression"]);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"{SelectedDataSourceType.ToString().ToLower()}://{Host}:{Port}/{DatabaseIndex}";
        }

        private int GetDefaultPort(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Redis:
                    return 6379;
                case DataSourceType.LiteDB:
                    return 0; // File-based
                case DataSourceType.RealIM:
                    return 8181;
                case DataSourceType.Petastorm:
                    return 0; // File-based
                case DataSourceType.RocketSet:
                    return 8080;
                default:
                    return 6379;
            }
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<AssemblyClassDefinition>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.INMEMORY))
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
