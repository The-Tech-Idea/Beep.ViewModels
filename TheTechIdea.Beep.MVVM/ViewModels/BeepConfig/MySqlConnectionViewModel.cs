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
    public partial class MySqlConnectionViewModel : DataConnectionViewModel
    {
        [ObservableProperty]
        List<ConnectionDriversConfig> mySqlDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedMySqlDatabaseType;

        [ObservableProperty]
        string serverName;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string socketPath;

        [ObservableProperty]
        bool useCompression;

        [ObservableProperty]
        bool allowBatch;

        [ObservableProperty]
        bool useAffectedRows;

        [ObservableProperty]
        bool useSSL;

        [ObservableProperty]
        string sslMode;

        [ObservableProperty]
        string certificateFile;

        [ObservableProperty]
        string certificatePassword;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        int defaultCommandTimeout;

        [ObservableProperty]
        string characterSet;

        [ObservableProperty]
        bool convertZeroDateTime;

        [ObservableProperty]
        bool allowZeroDateTime;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public MySqlConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            mySqlDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Mysql)
                {
                    mySqlDatabaseTypes.Add(item);
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceType == DataSourceType.Mysql))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    PackageNames.Add(item.PackageName);
                    PackageVersions.Add(item.version);
                }
            }

            if (mySqlDatabaseTypes.Count > 0)
            {
                SelectedMySqlDatabaseType = mySqlDatabaseTypes[0];
            }

            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();

            Filters.Add(new AppFilter {FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Mysql), Operator = "=" });
            DBWork.Get(Filters);

            Port = 3306;
            ConnectionTimeout = 15;
            DefaultCommandTimeout = 30;
            UseCompression = false;
            AllowBatch = true;
            UseAffectedRows = false;
            UseSSL = false;
            SslMode = "Preferred";
            ConvertZeroDateTime = false;
            AllowZeroDateTime = false;
            CharacterSet = "utf8mb4";
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
            Connection.DatabaseType = DataSourceType.Mysql;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedMySqlDatabaseType != null)
            {
                Connection.DriverName = SelectedMySqlDatabaseType.PackageName;
                Connection.DriverVersion = SelectedMySqlDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "MySQL");
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
            ServerName = "localhost";
            Port = 3306;
            SocketPath = "";
            UseCompression = false;
            AllowBatch = true;
            UseAffectedRows = false;
            UseSSL = false;
            SslMode = "Preferred";
            CertificateFile = "";
            CertificatePassword = "";
            ConnectionTimeout = 15;
            DefaultCommandTimeout = 30;
            CharacterSet = "utf8mb4";
            ConvertZeroDateTime = false;
            AllowZeroDateTime = false;
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

            if (!string.IsNullOrEmpty(ServerName))
            {
                builder["Server"] = ServerName;
            }

            builder["Port"] = Port;

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                builder["Database"] = DatabaseName;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["User Id"] = UserId;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            if (!string.IsNullOrEmpty(SocketPath))
            {
                builder["Socket"] = SocketPath;
            }

            builder["Compress"] = UseCompression;
            builder["Allow Batch"] = AllowBatch;
            builder["Use Affected Rows"] = UseAffectedRows;
            builder["SSL Mode"] = SslMode;

            if (!string.IsNullOrEmpty(CertificateFile))
            {
                builder["Certificate File"] = CertificateFile;
            }

            if (!string.IsNullOrEmpty(CertificatePassword))
            {
                builder["Certificate Password"] = CertificatePassword;
            }

            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Default Command Timeout"] = DefaultCommandTimeout;
            builder["Character Set"] = CharacterSet;
            builder["Convert Zero DateTime"] = ConvertZeroDateTime;
            builder["Allow Zero DateTime"] = AllowZeroDateTime;

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
                    ServerName = builder["Server"].ToString();
                }

                if (builder.ContainsKey("Port"))
                {
                    Port = Convert.ToInt32(builder["Port"]);
                }

                if (builder.ContainsKey("Database"))
                {
                    DatabaseName = builder["Database"].ToString();
                }

                if (builder.ContainsKey("User Id"))
                {
                    UserId = builder["User Id"].ToString();
                }

                if (builder.ContainsKey("Password"))
                {
                    Password = builder["Password"].ToString();
                }

                if (builder.ContainsKey("Socket"))
                {
                    SocketPath = builder["Socket"].ToString();
                }

                if (builder.ContainsKey("Compress"))
                {
                    UseCompression = Convert.ToBoolean(builder["Compress"]);
                }

                if (builder.ContainsKey("Allow Batch"))
                {
                    AllowBatch = Convert.ToBoolean(builder["Allow Batch"]);
                }

                if (builder.ContainsKey("Use Affected Rows"))
                {
                    UseAffectedRows = Convert.ToBoolean(builder["Use Affected Rows"]);
                }

                if (builder.ContainsKey("SSL Mode"))
                {
                    SslMode = builder["SSL Mode"].ToString();
                }

                if (builder.ContainsKey("Certificate File"))
                {
                    CertificateFile = builder["Certificate File"].ToString();
                }

                if (builder.ContainsKey("Certificate Password"))
                {
                    CertificatePassword = builder["Certificate Password"].ToString();
                }

                if (builder.ContainsKey("Connection Timeout"))
                {
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);
                }

                if (builder.ContainsKey("Default Command Timeout"))
                {
                    DefaultCommandTimeout = Convert.ToInt32(builder["Default Command Timeout"]);
                }

                if (builder.ContainsKey("Character Set"))
                {
                    CharacterSet = builder["Character Set"].ToString();
                }

                if (builder.ContainsKey("Convert Zero DateTime"))
                {
                    ConvertZeroDateTime = Convert.ToBoolean(builder["Convert Zero DateTime"]);
                }

                if (builder.ContainsKey("Allow Zero DateTime"))
                {
                    AllowZeroDateTime = Convert.ToBoolean(builder["Allow Zero DateTime"]);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            return $"mysql://{ServerName}:{Port}/{DatabaseName}";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Mysql)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
