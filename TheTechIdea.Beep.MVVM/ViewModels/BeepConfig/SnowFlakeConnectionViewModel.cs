using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class SnowFlakeConnectionViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string account = "";

        [ObservableProperty]
        private string warehouse = "";

        [ObservableProperty]
        private string database = "";

        [ObservableProperty]
        private string schema = "PUBLIC";

        [ObservableProperty]
        private string userName = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string role = "";

        [ObservableProperty]
        private string region = "us-west-2";

        [ObservableProperty]
        private int connectionTimeout = 30;

        [ObservableProperty]
        private bool validateDefaultParameters = true;

        [ObservableProperty]
        private string authenticator = "snowflake";

        [ObservableProperty]
        private string privateKeyFile = "";

        [ObservableProperty]
        private string privateKeyFilePwd = "";

        [ObservableProperty]
        UnitofWork<ConnectionProperties> dBWork;

        [ObservableProperty]
        private ConnectionProperties connectionProperties;

        public SnowFlakeConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            ConnectionProperties = new ConnectionProperties();
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
        }

        [RelayCommand]
        private void SaveConnection()
        {
            try
            {
                ConnectionProperties.ConnectionName = $"SnowFlake_{Account}_{Database}";
                ConnectionProperties.ConnectionString = BuildConnectionString();
                ConnectionProperties.DatabaseType = DataSourceType.SnowFlake;
                ConnectionProperties.DriverName = "Snowflake.Data";
                ConnectionProperties.DriverVersion = "2.0.0.0";

                // Save the connection
                DBWork.Commit();

                Editor.AddLogMessage("Info", "Connection saved successfully!", DateTime.Now, -1, "", Errors.Ok);
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
                var factory = DbProviderFactories.GetFactory("Snowflake.Data");

                using (var connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    connection.Close();
                }

                Editor.AddLogMessage("Info", "Connection test successful!", DateTime.Now, -1, "", Errors.Ok);
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Connection test failed: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        [RelayCommand]
        private void BrowsePrivateKeyFile()
        {
            // Implementation for browsing private key file
            // This would typically open a file dialog
        }

        private string BuildConnectionString()
        {
            var builder = new DbConnectionStringBuilder();

            builder["account"] = Account;
            builder["warehouse"] = Warehouse;
            builder["database"] = Database;
            builder["schema"] = Schema;
            builder["user"] = UserName;
            builder["password"] = Password;
            builder["role"] = Role;
            builder["region"] = Region;
            builder["connection_timeout"] = ConnectionTimeout;
            builder["validate_default_parameters"] = ValidateDefaultParameters;
            builder["authenticator"] = Authenticator;

            if (!string.IsNullOrEmpty(PrivateKeyFile))
            {
                builder["private_key_file"] = PrivateKeyFile;
                if (!string.IsNullOrEmpty(PrivateKeyFilePwd))
                    builder["private_key_file_pwd"] = PrivateKeyFilePwd;
            }

            return builder.ConnectionString;
        }

        public void ParseConnectionString(string connectionString)
        {
            try
            {
                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                if (builder.ContainsKey("account"))
                    Account = builder["account"].ToString();

                if (builder.ContainsKey("warehouse"))
                    Warehouse = builder["warehouse"].ToString();

                if (builder.ContainsKey("database"))
                    Database = builder["database"].ToString();

                if (builder.ContainsKey("schema"))
                    Schema = builder["schema"].ToString();

                if (builder.ContainsKey("user"))
                    UserName = builder["user"].ToString();

                if (builder.ContainsKey("password"))
                    Password = builder["password"].ToString();

                if (builder.ContainsKey("role"))
                    Role = builder["role"].ToString();

                if (builder.ContainsKey("region"))
                    Region = builder["region"].ToString();

                if (builder.ContainsKey("connection_timeout"))
                    ConnectionTimeout = Convert.ToInt32(builder["connection_timeout"]);

                if (builder.ContainsKey("validate_default_parameters"))
                    ValidateDefaultParameters = Convert.ToBoolean(builder["validate_default_parameters"]);

                if (builder.ContainsKey("authenticator"))
                    Authenticator = builder["authenticator"].ToString();

                if (builder.ContainsKey("private_key_file"))
                    PrivateKeyFile = builder["private_key_file"].ToString();

                if (builder.ContainsKey("private_key_file_pwd"))
                    PrivateKeyFilePwd = builder["private_key_file_pwd"].ToString();
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Error parsing connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }
    }
}
