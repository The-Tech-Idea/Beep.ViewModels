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
using System.Data.Common;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DB2ConnectionViewModel : BaseViewModel
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
        private string serverName = "localhost";

        [ObservableProperty]
        private string databaseName = "SAMPLE";

        [ObservableProperty]
        private string userName = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private int port = 50000;

        [ObservableProperty]
        private string protocol = "TCPIP";

        [ObservableProperty]
        private int connectionTimeout = 30;

        [ObservableProperty]
        private int commandTimeout = 30;

        [ObservableProperty]
        private bool pooling = true;

        [ObservableProperty]
        private int minPoolSize = 0;

        [ObservableProperty]
        private int maxPoolSize = 100;

        [ObservableProperty]
        private string currentSchema = "";

        public DB2ConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
        }

        [RelayCommand]
        private void SaveConnection()
        {
            try
            {
                ConnectionProperties conn = new ConnectionProperties();
                conn.ConnectionName = $"DB2_{ServerName}_{DatabaseName}";
                conn.ConnectionString = BuildConnectionString();
                conn.DatabaseType = DataSourceType.DB2;
                conn.DriverName = "IBM.Data.DB2";
                conn.DriverVersion = "11.5.0.0";

                // Save the connection
                Editor.ConfigEditor.DataConnections.Add(conn);
                Editor.ConfigEditor.SaveDataconnectionsValues();

                Editor.AddLogMessage("Success", "Connection saved successfully!", DateTime.Now, -1, "", Errors.Ok);
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Error saving connection: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        [RelayCommand]
        private void TestConnection()
        {
            try
            {
                var connectionString = BuildConnectionString();
                var factory = DbProviderFactories.GetFactory("IBM.Data.DB2");

                using (var connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    connection.Close();
                }

                Editor.AddLogMessage("Success", "Connection test successful!", DateTime.Now, -1, "", Errors.Ok);
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Connection test failed: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildConnectionString()
        {
            var builder = new DbConnectionStringBuilder();

            builder["Server"] = ServerName;
            builder["Database"] = DatabaseName;
            builder["UserID"] = UserName;
            builder["Password"] = Password;
            builder["Port"] = Port;
            builder["Protocol"] = Protocol;
            builder["Connection Timeout"] = ConnectionTimeout;
            builder["Command Timeout"] = CommandTimeout;
            builder["Pooling"] = Pooling;
            builder["Min Pool Size"] = MinPoolSize;
            builder["Max Pool Size"] = MaxPoolSize;

            if (!string.IsNullOrEmpty(CurrentSchema))
                builder["CurrentSchema"] = CurrentSchema;

            return builder.ConnectionString;
        }

        public void ParseConnectionString(string connectionString)
        {
            try
            {
                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                if (builder.ContainsKey("Server"))
                    ServerName = builder["Server"].ToString();

                if (builder.ContainsKey("Database"))
                    DatabaseName = builder["Database"].ToString();

                if (builder.ContainsKey("UserID"))
                    UserName = builder["UserID"].ToString();

                if (builder.ContainsKey("Password"))
                    Password = builder["Password"].ToString();

                if (builder.ContainsKey("Port"))
                    Port = Convert.ToInt32(builder["Port"]);

                if (builder.ContainsKey("Protocol"))
                    Protocol = builder["Protocol"].ToString();

                if (builder.ContainsKey("Connection Timeout"))
                    ConnectionTimeout = Convert.ToInt32(builder["Connection Timeout"]);

                if (builder.ContainsKey("Command Timeout"))
                    CommandTimeout = Convert.ToInt32(builder["Command Timeout"]);

                if (builder.ContainsKey("Pooling"))
                    Pooling = Convert.ToBoolean(builder["Pooling"]);

                if (builder.ContainsKey("Min Pool Size"))
                    MinPoolSize = Convert.ToInt32(builder["Min Pool Size"]);

                if (builder.ContainsKey("Max Pool Size"))
                    MaxPoolSize = Convert.ToInt32(builder["Max Pool Size"]);

                if (builder.ContainsKey("CurrentSchema"))
                    CurrentSchema = builder["CurrentSchema"].ToString();
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Error parsing connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }
    }
}
