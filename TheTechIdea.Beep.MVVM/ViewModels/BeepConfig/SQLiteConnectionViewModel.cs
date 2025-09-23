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
using System.Windows.Forms;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class SQLiteConnectionViewModel : DataConnectionViewModel
    {
        [ObservableProperty]
        List<ConnectionDriversConfig> sqliteDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedSQLiteDatabaseType;

        [ObservableProperty]
        string databaseFilePath;

        [ObservableProperty]
        bool useUri;

        [ObservableProperty]
        bool readOnly;

        [ObservableProperty]
        bool foreignKeys;

        [ObservableProperty]
        bool recursiveTriggers;

        [ObservableProperty]
        int cacheSize;

        [ObservableProperty]
        int pageSize;

        [ObservableProperty]
        int maxPageCount;

        [ObservableProperty]
        string journalMode;

        [ObservableProperty]
        string synchronous;

        [ObservableProperty]
        string tempStore;

        [ObservableProperty]
        int busyTimeout;

        [ObservableProperty]
        bool failIfMissing;

        [ObservableProperty]
        bool poolConnection;

        [ObservableProperty]
        int maxPoolSize;

        [ObservableProperty]
        int minPoolSize;

        [ObservableProperty]
        int poolLifetime;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public SQLiteConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            sqliteDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.SqlLite)
                {
                    sqliteDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.SqlLite))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    PackageNames.Add(item.PackageName);
                    PackageVersions.Add(item.version);
                }
            }

            if (sqliteDatabaseTypes.Count > 0)
            {
                SelectedSQLiteDatabaseType = sqliteDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.FILE;
            SelectedCategoryValue = (int)DatasourceCategory.FILE;
            SelectedCategoryTextValue = DatasourceCategory.FILE.ToString();

            Filters.Add(new AppFilter { FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.SqlLite), Operator = "=" });
            DBWork.Get(Filters);

            UseUri = false;
            ReadOnly = false;
            ForeignKeys = true;
            RecursiveTriggers = false;
            CacheSize = -2000;
            PageSize = 4096;
            MaxPageCount = 1073741823;
            JournalMode = "WAL";
            Synchronous = "NORMAL";
            TempStore = "DEFAULT";
            BusyTimeout = 30000;
            FailIfMissing = false;
            PoolConnection = false;
            MaxPoolSize = 100;
            MinPoolSize = 0;
            PoolLifetime = 0;
        }

        [RelayCommand]
        public void SaveConnection()
        {
            if (Connection == null)
            {
                Connection = new ConnectionProperties();
            }
            Connection.ConnectionName = CurrentDataSourceName;
            Connection.Database = Path.GetFileName(DatabaseFilePath);
            Connection.UserID = "";
            Connection.Password = Password;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= DataSourceType.SqlLite;
            Connection.Category = DatasourceCategory.FILE;
            if (SelectedSQLiteDatabaseType != null)
            {
                Connection.DriverName = SelectedSQLiteDatabaseType.PackageName;
                Connection.DriverVersion = SelectedSQLiteDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "SQLite");
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            DatabaseFilePath = "";
            Password = "";
            UseUri = false;
            ReadOnly = false;
            ForeignKeys = true;
            RecursiveTriggers = false;
            CacheSize = -2000;
            PageSize = 4096;
            MaxPageCount = 1073741823;
            JournalMode = "WAL";
            Synchronous = "NORMAL";
            TempStore = "DEFAULT";
            BusyTimeout = 30000;
            FailIfMissing = false;
            PoolConnection = false;
            MaxPoolSize = 100;
            MinPoolSize = 0;
            PoolLifetime = 0;
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                Password = Connection.Password;
                ParseConnectionString(Connection.ConnectionString);
                SelectedconnectionGuid = Connection.GuidID;
            }
        }

        [RelayCommand]
        public void BrowseDatabaseFile()
        {
            // TODO: Implement file dialog through proper service or UI layer
            // For now, this is a placeholder to avoid compilation errors
            // The DatabaseFilePath should be set through data binding or other means
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            if (!string.IsNullOrEmpty(DatabaseFilePath))
            {
                builder["Data Source"] = DatabaseFilePath;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            builder["UseUri"] = UseUri;
            builder["Read Only"] = ReadOnly;
            builder["Foreign Keys"] = ForeignKeys;
            builder["Recursive Triggers"] = RecursiveTriggers;
            builder["Cache Size"] = CacheSize;
            builder["Page Size"] = PageSize;
            builder["Max Page Count"] = MaxPageCount;

            if (!string.IsNullOrEmpty(JournalMode))
            {
                builder["Journal Mode"] = JournalMode;
            }

            if (!string.IsNullOrEmpty(Synchronous))
            {
                builder["Synchronous"] = Synchronous;
            }

            if (!string.IsNullOrEmpty(TempStore))
            {
                builder["Temp Store"] = TempStore;
            }

            builder["Busy Timeout"] = BusyTimeout;
            builder["FailIfMissing"] = FailIfMissing;
            builder["Pooling"] = PoolConnection;

            if (PoolConnection)
            {
                builder["Max Pool Size"] = MaxPoolSize;
                builder["Min Pool Size"] = MinPoolSize;
                builder["Pool Lifetime"] = PoolLifetime;
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

                if (builder.ContainsKey("Data Source"))
                {
                    DatabaseFilePath = builder["Data Source"].ToString();
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("UseUri"))
                {
                    UseUri = Convert.ToBoolean(builder["UseUri"]);
                }

                if (builder.ContainsKey("Read Only"))
                {
                    ReadOnly = Convert.ToBoolean(builder["Read Only"]);
                }

                if (builder.ContainsKey("Foreign Keys"))
                {
                    ForeignKeys = Convert.ToBoolean(builder["Foreign Keys"]);
                }

                if (builder.ContainsKey("Recursive Triggers"))
                {
                    RecursiveTriggers = Convert.ToBoolean(builder["Recursive Triggers"]);
                }

                if (builder.ContainsKey("Cache Size"))
                {
                    CacheSize = Convert.ToInt32(builder["Cache Size"]);
                }

                if (builder.ContainsKey("Page Size"))
                {
                    PageSize = Convert.ToInt32(builder["Page Size"]);
                }

                if (builder.ContainsKey("Max Page Count"))
                {
                    MaxPageCount = Convert.ToInt32(builder["Max Page Count"]);
                }

                if (builder.ContainsKey("Journal Mode"))
                {
                    JournalMode = builder["Journal Mode"].ToString();
                }

                if (builder.ContainsKey("Synchronous"))
                {
                    Synchronous = builder["Synchronous"].ToString();
                }

                if (builder.ContainsKey("Temp Store"))
                {
                    TempStore = builder["Temp Store"].ToString();
                }

                if (builder.ContainsKey("Busy Timeout"))
                {
                    BusyTimeout = Convert.ToInt32(builder["Busy Timeout"]);
                }

                if (builder.ContainsKey("FailIfMissing"))
                {
                    FailIfMissing = Convert.ToBoolean(builder["FailIfMissing"]);
                }

                if (builder.ContainsKey("Pooling"))
                {
                    PoolConnection = Convert.ToBoolean(builder["Pooling"]);
                }

                if (builder.ContainsKey("Max Pool Size"))
                {
                    MaxPoolSize = Convert.ToInt32(builder["Max Pool Size"]);
                }

                if (builder.ContainsKey("Min Pool Size"))
                {
                    MinPoolSize = Convert.ToInt32(builder["Min Pool Size"]);
                }

                if (builder.ContainsKey("Pool Lifetime"))
                {
                    PoolLifetime = Convert.ToInt32(builder["Pool Lifetime"]);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"sqlite:///{DatabaseFilePath}";
        }

        private void GetInstallDataSources()
        {
            InstalledDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.SqlLite)
                {
                    InstalledDataSources.Add(item);
                }
            }
        }
    }
}
