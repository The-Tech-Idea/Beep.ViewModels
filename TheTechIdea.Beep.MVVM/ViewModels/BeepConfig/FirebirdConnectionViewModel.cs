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
    public partial class FirebirdConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> firebirdDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedFirebirdDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string database;

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
        string role;

        [ObservableProperty]
        string charset;

        [ObservableProperty]
        int dialect;

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
        int packetSize;

        [ObservableProperty]
        string isolationLevel;

        [ObservableProperty]
        bool enlist;

        [ObservableProperty]
        bool returnRecordsAffected;

        [ObservableProperty]
        bool recordsAffected;

        [ObservableProperty]
        string applicationName;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public FirebirdConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            firebirdDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.FireBird)
                {
                    firebirdDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.FireBird))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                    packageVersions.Add(item.version);
                }
            }

            if (firebirdDatabaseTypes.Count > 0)
            {
                SelectedFirebirdDatabaseType = firebirdDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            Filters.Add(new AppFilter {FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.FireBird), Operator = "=" });
            DBWork.Get(Filters);

            Port = 3050;
            ConnectionTimeout = 15;
            CommandTimeout = 20;
            Pooling = true;
            MinPoolSize = 0;
            MaxPoolSize = 100;
            PacketSize = 8192;
            Dialect = 3;
            Charset = "UTF8";
            IsolationLevel = "ReadCommitted";
            Enlist = true;
            ReturnRecordsAffected = true;
            RecordsAffected = true;
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
            Connection.Database = Database;
            Connection.UserID = UserId;
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= DataSourceType.FireBird;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedFirebirdDatabaseType != null)
            {
                Connection.DriverName = SelectedFirebirdDatabaseType.PackageName;
                Connection.DriverVersion = SelectedFirebirdDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "Firebird");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            Database = "";
            UserId = "SYSDBA";
            Password = "masterkey";
            Host = "localhost";
            Port = 3050;
            Role = "";
            Charset = "UTF8";
            Dialect = 3;
            ConnectionTimeout = 15;
            CommandTimeout = 20;
            Pooling = true;
            MinPoolSize = 0;
            MaxPoolSize = 100;
            PacketSize = 8192;
            IsolationLevel = "ReadCommitted";
            Enlist = true;
            ReturnRecordsAffected = true;
            RecordsAffected = true;
            ApplicationName = "Beep Application";
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                Database = Connection.Database;
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
                builder["DataSource"] = Host;
            }

            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(Database))
            {
                builder["Database"] = Database;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["User"] = UserId;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            if (!string.IsNullOrEmpty(Role))
            {
                builder["Role"] = Role;
            }

            if (!string.IsNullOrEmpty(Charset))
            {
                builder["Charset"] = Charset;
            }

            builder["Dialect"] = Dialect;
            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Command Timeout"] = CommandTimeout;
            builder["Pooling"] = Pooling;

            if (Pooling)
            {
                builder["Min Pool Size"] = MinPoolSize;
                builder["Max Pool Size"] = MaxPoolSize;
            }

            builder["Packet Size"] = PacketSize;

            if (!string.IsNullOrEmpty(IsolationLevel))
            {
                builder["Isolation Level"] = IsolationLevel;
            }

            builder["Enlist"] = Enlist;
            builder["Return Records Affected"] = ReturnRecordsAffected;
            builder["Records Affected"] = RecordsAffected;

            if (!string.IsNullOrEmpty(ApplicationName))
            {
                builder["Application Name"] = ApplicationName;
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

                if (builder.ContainsKey("DataSource"))
                {
                    Host = builder["DataSource"].ToString();
                }

                if (builder.ContainsKey("Port"))
                {
                    Port = Convert.ToInt32(builder["Port"]);
                }

                if (builder.ContainsKey("Database"))
                {
                    Database = builder["Database"].ToString();
                }

                if (builder.ContainsKey("User"))
                {
                    UserId = builder["User"].ToString();
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("Role"))
                {
                    Role = builder["Role"].ToString();
                }

                if (builder.ContainsKey("Charset"))
                {
                    Charset = builder["Charset"].ToString();
                }

                if (builder.ContainsKey("Dialect"))
                {
                    Dialect = Convert.ToInt32(builder["Dialect"]);
                }

                if (builder.ContainsKey("Connection Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);
                }

                if (builder.ContainsKey("Command Timeout"))
                {
                    CommandTimeout = Convert.ToInt32(builder["Command Timeout"]);
                }

                if (builder.ContainsKey("Pooling"))
                {
                    Pooling = Convert.ToBoolean(builder["Pooling"]);
                }

                if (builder.ContainsKey("Min Pool Size"))
                {
                    MinPoolSize = Convert.ToInt32(builder["Min Pool Size"]);
                }

                if (builder.ContainsKey("Max Pool Size"))
                {
                    MaxPoolSize = Convert.ToInt32(builder["Max Pool Size"]);
                }

                if (builder.ContainsKey("Packet Size"))
                {
                    PacketSize = Convert.ToInt32(builder["Packet Size"]);
                }

                if (builder.ContainsKey("Isolation Level"))
                {
                    IsolationLevel = builder["Isolation Level"].ToString();
                }

                if (builder.ContainsKey("Enlist"))
                {
                    Enlist = Convert.ToBoolean(builder["Enlist"]);
                }

                if (builder.ContainsKey("Return Records Affected"))
                {
                    ReturnRecordsAffected = Convert.ToBoolean(builder["Return Records Affected"]);
                }

                if (builder.ContainsKey("Records Affected"))
                {
                    RecordsAffected = Convert.ToBoolean(builder["Records Affected"]);
                }

                if (builder.ContainsKey("Application Name"))
                {
                    ApplicationName = builder["Application Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"{Host}:{Port}/{Database}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.FireBird)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
