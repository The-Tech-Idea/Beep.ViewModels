using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
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
    public partial class CouchDBConnectionViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string serverUrl = "http://localhost:5984";

        [ObservableProperty]
        private string databaseName = "";

        [ObservableProperty]
        private string userName = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private int connectionTimeout = 30;

        [ObservableProperty]
        private int maxConnections = 20;

        [ObservableProperty]
        private bool enableSsl = false;

        [ObservableProperty]
        private string sslCertificatePath = "";

        [ObservableProperty]
        private bool ignoreSslErrors = false;

        [ObservableProperty]
        private string proxyHost = "";

        [ObservableProperty]
        private int proxyPort = 8080;

        [ObservableProperty]
        private string proxyUserName = "";

        [ObservableProperty]
        private string proxyPassword = "";

        [ObservableProperty]
        UnitofWork<ConnectionProperties> dBWork;

        [ObservableProperty]
        private ConnectionProperties connectionProperties;

        public CouchDBConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
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
                ConnectionProperties.ConnectionName = $"CouchDB_{ServerUrl}_{DatabaseName}";
                ConnectionProperties.ConnectionString = BuildConnectionString();
                ConnectionProperties.DatabaseType = DataSourceType.CouchDB;
                ConnectionProperties.DriverName = "MyCouch";
                ConnectionProperties.DriverVersion = "7.0.0.0";

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
        private async Task TestConnection()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(ConnectionTimeout);

                    // Set up authentication if provided
                    if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                    {
                        var byteArray = Encoding.ASCII.GetBytes($"{UserName}:{Password}");
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    // Test connection by getting server info
                    var response = await client.GetAsync($"{ServerUrl}/");
                    response.EnsureSuccessStatusCode();

                    Editor.AddLogMessage("Info", "Connection test successful!", DateTime.Now, -1, "", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Connection test failed: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        [RelayCommand]
        private void BrowseSslCertificate()
        {
            // Implementation for browsing SSL certificate file
            // This would typically open a file dialog
        }

        private string BuildConnectionString()
        {
            var builder = new DbConnectionStringBuilder();

            builder["Server"] = ServerUrl;
            builder["Database"] = DatabaseName;
            builder["UserName"] = UserName;
            builder["Password"] = Password;
            builder["ConnectionTimeout"] = ConnectionTimeout;
            builder["MaxConnections"] = MaxConnections;
            builder["EnableSsl"] = EnableSsl;
            builder["IgnoreSslErrors"] = IgnoreSslErrors;

            if (!string.IsNullOrEmpty(SslCertificatePath))
                builder["SslCertificatePath"] = SslCertificatePath;

            if (!string.IsNullOrEmpty(ProxyHost))
            {
                builder["ProxyHost"] = ProxyHost;
                builder["ProxyPort"] = ProxyPort;
                builder["ProxyUserName"] = ProxyUserName;
                builder["ProxyPassword"] = ProxyPassword;
            }

            return builder.ConnectionString;
        }

        public void ParseConnectionString(string connectionString)
        {
            try
            {
                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                if (builder.ContainsKey("Server"))
                    ServerUrl = builder["Server"].ToString();

                if (builder.ContainsKey("Database"))
                    DatabaseName = builder["Database"].ToString();

                if (builder.ContainsKey("UserName"))
                    UserName = builder["UserName"].ToString();

                if (builder.ContainsKey("Password"))
                    Password = builder["Password"].ToString();

                if (builder.ContainsKey("ConnectionTimeout"))
                    ConnectionTimeout = Convert.ToInt32(builder["ConnectionTimeout"]);

                if (builder.ContainsKey("MaxConnections"))
                    MaxConnections = Convert.ToInt32(builder["MaxConnections"]);

                if (builder.ContainsKey("EnableSsl"))
                    EnableSsl = Convert.ToBoolean(builder["EnableSsl"]);

                if (builder.ContainsKey("IgnoreSslErrors"))
                    IgnoreSslErrors = Convert.ToBoolean(builder["IgnoreSslErrors"]);

                if (builder.ContainsKey("SslCertificatePath"))
                    SslCertificatePath = builder["SslCertificatePath"].ToString();

                if (builder.ContainsKey("ProxyHost"))
                    ProxyHost = builder["ProxyHost"].ToString();

                if (builder.ContainsKey("ProxyPort"))
                    ProxyPort = Convert.ToInt32(builder["ProxyPort"]);

                if (builder.ContainsKey("ProxyUserName"))
                    ProxyUserName = builder["ProxyUserName"].ToString();

                if (builder.ContainsKey("ProxyPassword"))
                    ProxyPassword = builder["ProxyPassword"].ToString();
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Error parsing connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }
    }
}
