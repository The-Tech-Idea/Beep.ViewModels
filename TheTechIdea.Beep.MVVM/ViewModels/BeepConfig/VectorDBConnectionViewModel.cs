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
using System.Text;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class VectorDBConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> vectorDBDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedVectorDBDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        // VectorDB specific properties
        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string database;

        [ObservableProperty]
        string userName;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string apiKey;

        [ObservableProperty]
        string apiSecret;

        [ObservableProperty]
        string region;

        [ObservableProperty]
        string projectId;

        [ObservableProperty]
        string collectionName;

        [ObservableProperty]
        string indexName;

        [ObservableProperty]
        int dimension;

        [ObservableProperty]
        string metric;

        [ObservableProperty]
        string embeddingModel;

        [ObservableProperty]
        int maxConnections;

        [ObservableProperty]
        int connectionTimeout;

        [ObservableProperty]
        int readTimeout;

        [ObservableProperty]
        bool useSSL;

        [ObservableProperty]
        string sslCertPath;

        [ObservableProperty]
        string sslKeyPath;

        [ObservableProperty]
        string sslCaPath;

        [ObservableProperty]
        bool verifySSL;

        [ObservableProperty]
        string persistencePath;

        [ObservableProperty]
        string cloudProvider;

        [ObservableProperty]
        string environment;

        [ObservableProperty]
        string namespaceName;

        [ObservableProperty]
        string tenantId;

        [ObservableProperty]
        string organizationId;

        [ObservableProperty]
        string clusterName;

        [ObservableProperty]
        string endpointUrl;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public VectorDBConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            PackageNames = new List<string>();
            packageVersions = new List<string>();
            vectorDBDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for VectorDB data sources
            var vectorDBCategories = new[] { DatasourceCategory.VectorDB };
            foreach (var category in vectorDBCategories)
            {
                foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == category))
                {
                    if (!string.IsNullOrEmpty(item.PackageName))
                    {
                        var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                        if (ds != null)
                        {
                            PackageNames.Add(item.PackageName);
                            vectorDBDatabaseTypes.Add(item);
                        }
                    }
                }
            }

            foreach (var category in vectorDBCategories)
            {
                foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == category))
                {
                    if (!string.IsNullOrEmpty(item.PackageName))
                    {
                        packageVersions.Add(item.version);
                    }
                }
            }

            if (vectorDBDatabaseTypes.Count > 0)
            {
                SelectedVectorDBDatabaseType = vectorDBDatabaseTypes[0];
                SelectedDataSourceType = vectorDBDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.VectorDB;
            SelectedCategoryValue = (int)DatasourceCategory.VectorDB;
            SelectedCategoryTextValue = DatasourceCategory.VectorDB.ToString();

            // Filter for VectorDB category
            var categoryFilter = new AppFilter
            {
               FieldName = "Category",
                FieldType = typeof(DatasourceCategory),
                Operator = "IN",
                FilterValue = $"{(int)DatasourceCategory.VectorDB}"
            };
            Filters.Add(categoryFilter);

            // Initialize default values
            Dimension = 1536; // Common embedding dimension
            Metric = "cosine";
            MaxConnections = 10;
            ConnectionTimeout = 30000;
            ReadTimeout = 30000;
            UseSSL = true;
            VerifySSL = true;
        }

        private void GetInstallDataSources()
        {
            InstalledDataSources = new List<AssemblyClassDefinition>();
            if (Editor.ConfigEditor.DataSourcesClasses != null)
            {
                foreach (var item in Editor.ConfigEditor.DataSourcesClasses)
                {
                    if (!string.IsNullOrEmpty(item.className))
                    {
                        InstalledDataSources.Add(item);
                    }
                }
            }
        }

        [RelayCommand]
        public void TestConnection()
        {
            try
            {
                IsBusy = true;
                // Test connection logic would go here
                // This would validate the connection parameters
            }
            catch (Exception ex)
            {
                // Handle connection test errors
                Editor.AddLogMessage("Connection Test", $"Error testing connection: {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void SaveConnection()
        {
            try
            {
                if (Connection != null)
                {
                    // Update connection properties with current values
                    Connection.ConnectionString = BuildConnectionString();
                    DBWork.Update(Connection);
                    DBWork.Commit();
                    IsSaved = true;
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Save Connection", $"Error saving connection: {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }

        private string BuildConnectionString()
        {
            // Build connection string based on the selected VectorDB type
            var builder = new StringBuilder();

            switch (SelectedDataSourceType)
            {
                case DataSourceType.ChromaDB:
                    builder.Append($"Host={Host};Port={Port};Database={Database}");
                    if (!string.IsNullOrEmpty(ApiKey)) builder.Append($";ApiKey={ApiKey}");
                    break;

                case DataSourceType.PineCone:
                    builder.Append($"ApiKey={ApiKey};Environment={Environment};ProjectId={ProjectId}");
                    if (!string.IsNullOrEmpty(Region)) builder.Append($";Region={Region}");
                    break;

                case DataSourceType.Qdrant:
                    builder.Append($"Host={Host};Port={Port}");
                    if (!string.IsNullOrEmpty(ApiKey)) builder.Append($";ApiKey={ApiKey}");
                    break;

                case DataSourceType.Weaviate:
                    builder.Append($"Host={Host};Port={Port}");
                    if (!string.IsNullOrEmpty(ApiKey)) builder.Append($";ApiKey={ApiKey}");
                    break;

                case DataSourceType.Milvus:
                    builder.Append($"Host={Host};Port={Port};Database={Database}");
                    if (!string.IsNullOrEmpty(UserName)) builder.Append($";User={UserName}");
                    if (!string.IsNullOrEmpty(Password)) builder.Append($";Password={Password}");
                    break;

                case DataSourceType.RedisVector:
                    builder.Append($"Host={Host};Port={Port}");
                    if (!string.IsNullOrEmpty(Password)) builder.Append($";Password={Password}");
                    break;

                case DataSourceType.Zilliz:
                    builder.Append($"CloudProvider={CloudProvider};Region={Region};ProjectId={ProjectId};ApiKey={ApiKey}");
                    break;

                case DataSourceType.Vespa:
                    builder.Append($"Host={Host};Port={Port};Namespace={NamespaceName}");
                    break;

                default:
                    builder.Append($"Host={Host};Port={Port};Database={Database}");
                    break;
            }

            if (UseSSL) builder.Append(";UseSSL=true");
            if (!VerifySSL) builder.Append(";VerifySSL=false");

            return builder.ToString();
        }

        [RelayCommand]
        public void LoadConnection(ConnectionProperties conn)
        {
            if (conn != null)
            {
                Connection = conn;
                SelectedconnectionGuid = conn.GuidID;
                CurrentDataSourceName = conn.ConnectionName;

                // Parse connection string to populate properties
                ParseConnectionString(conn.ConnectionString);
            }
        }

        private void ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return;

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Contains('='))
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();

                        switch (key.ToLower())
                        {
                            case "host": Host = value; break;
                            case "port": if (int.TryParse(value, out int p)) Port = p; break;
                            case "database": Database = value; break;
                            case "user": UserName = value; break;
                            case "username": UserName = value; break;
                            case "password": Password = value; break;
                            case "apikey": ApiKey = value; break;
                            case "apisecret": ApiSecret = value; break;
                            case "region": Region = value; break;
                            case "projectid": ProjectId = value; break;
                            case "collectionname": CollectionName = value; break;
                            case "indexname": IndexName = value; break;
                            case "dimension": if (int.TryParse(value, out int d)) Dimension = d; break;
                            case "metric": Metric = value; break;
                            case "embeddingmodel": EmbeddingModel = value; break;
                            case "maxconnections": if (int.TryParse(value, out int mc)) MaxConnections = mc; break;
                            case "connectiontimeout": if (int.TryParse(value, out int ct)) ConnectionTimeout = ct; break;
                            case "readtimeout": if (int.TryParse(value, out int rt)) ReadTimeout = rt; break;
                            case "usessl": if (bool.TryParse(value, out bool ssl)) UseSSL = ssl; break;
                            case "verifyssl": if (bool.TryParse(value, out bool vssl)) VerifySSL = vssl; break;
                            case "cloudprovider": CloudProvider = value; break;
                            case "environment": Environment = value; break;
                            case "namespace": NamespaceName = value; break;
                            case "tenantid": TenantId = value; break;
                            case "organizationid": OrganizationId = value; break;
                            case "clustername": ClusterName = value; break;
                            case "endpointurl": EndpointUrl = value; break;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties
            {
                GuidID = Guid.NewGuid().ToString(),
                ConnectionName = "New VectorDB Connection",
                Category = DatasourceCategory.VectorDB,
                DatabaseType = SelectedDataSourceType,
                ConnectionString = ""
            };

            // Set default values
            Host = "localhost";
            Port = 8000;
            Database = "default";
            Dimension = 1536;
            Metric = "cosine";
            UseSSL = true;
            VerifySSL = true;
        }
    }
}