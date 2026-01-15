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
    public partial class OracleConnectionViewModel : DataConnectionViewModel
    {
        [ObservableProperty]
        List<ConnectionDriversConfig> oracleDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedOracleDatabaseType;

        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string tnsName;

        [ObservableProperty]
        string serviceName;

        [ObservableProperty]
        string sid;

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
        string oracleHome;

        [ObservableProperty]
        List<ConnectionDriversConfig> installedDataSources;

        public OracleConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            oracleDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Oracle)
                {
                    oracleDatabaseTypes.Add(item);
                }
            }
            if (oracleDatabaseTypes.Count > 0)
            {
                SelectedOracleDatabaseType = oracleDatabaseTypes[0];
            }
            SelectedCategoryItem = DatasourceCategory.RDBMS;
            SelectedCategoryValue = (int)DatasourceCategory.RDBMS;
            SelectedCategoryTextValue = DatasourceCategory.RDBMS.ToString();
            Filters.Add(new AppFilter {FieldName = "DatasourceType", FieldType = typeof(DataSourceType), FilterValue = Enum.GetName(DataSourceType.Oracle), Operator = "=" });
            DBWork.Get(Filters);
            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ValidateConnection = true;
            UseConnectionPooling = true;
            Port = 1521;
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
            Connection.DatabaseType= DataSourceType.Oracle;
            Connection.Category = DatasourceCategory.RDBMS;
            if (SelectedOracleDatabaseType != null)
            {
                Connection.DriverName = SelectedOracleDatabaseType.PackageName;
                Connection.DriverVersion = SelectedOracleDatabaseType.version;
            }
            Connection.Url = BuildUrl();
            DBWork.Commit();
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                ConnectionTestUtility.TestConnectionWithLogging(Editor, Connection, "Oracle");
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
            TnsName = "";
            ServiceName = "";
            Sid = "";
            Host = "";
            Port = 1521;
            MinPoolSize = 1;
            MaxPoolSize = 100;
            ConnectionLifetime = 0;
            ValidateConnection = true;
            UseConnectionPooling = true;
            OracleHome = "";
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

            if (!string.IsNullOrEmpty(TnsName))
            {
                builder["Data Source"] = TnsName;
            }
            else if (!string.IsNullOrEmpty(ServiceName))
            {
                builder["Data Source"] = $"//{Host}:{Port}/{ServiceName}";
            }
            else if (!string.IsNullOrEmpty(Sid))
            {
                builder["Data Source"] = $"//{Host}:{Port}/{Sid}";
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                builder["User Id"] = UserId;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                builder["Password"] = Password;
            }

            if (UseConnectionPooling)
            {
                builder["Pooling"] = true;
                builder["Min Pool Size"] = MinPoolSize;
                builder["Max Pool Size"] = MaxPoolSize;
                builder["Connection Lifetime"] = ConnectionLifetime;
                builder["Validate Connection"] = ValidateConnection;
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
                    var dataSource = builder["Data Source"].ToString();
                    if (dataSource.Contains("//"))
                    {
                        // Service name or SID format: //host:port/service
                        var parts = dataSource.Split(new[] { "//", ":", "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            Host = parts[0];
                            Port = int.TryParse(parts[1], out var p) ? p : 1521;
                            var serviceOrSid = parts[2];
                            if (serviceOrSid.Contains("."))
                            {
                                ServiceName = serviceOrSid;
                            }
                            else
                            {
                                Sid = serviceOrSid;
                            }
                        }
                    }
                    else
                    {
                        // TNS name format
                        TnsName = dataSource;
                    }
                }

                if (builder.ContainsKey("User Id"))
                {
                    UserId = builder["User Id"].ToString();
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
                // Handle parsing errors
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            if (!string.IsNullOrEmpty(TnsName))
            {
                return $"oracle://{TnsName}";
            }
            else if (!string.IsNullOrEmpty(ServiceName))
            {
                return $"oracle://{Host}:{Port}/{ServiceName}";
            }
            else if (!string.IsNullOrEmpty(Sid))
            {
                return $"oracle://{Host}:{Port}/{Sid}";
            }
            return "";
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (item.DatasourceType == DataSourceType.Oracle)
                {
                    installedDataSources.Add(item);
                }
            }
        }
    }
}
