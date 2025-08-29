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
    public partial class CloudConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> cloudDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedCloudDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string accountName;

        [ObservableProperty]
        string accountKey;

        [ObservableProperty]
        string accessKey;

        [ObservableProperty]
        string secretKey;

        [ObservableProperty]
        string region;

        [ObservableProperty]
        string bucketName;

        [ObservableProperty]
        string containerName;

        [ObservableProperty]
        string projectId;

        [ObservableProperty]
        string datasetId;

        [ObservableProperty]
        string subscriptionId;

        [ObservableProperty]
        string resourceGroup;

        [ObservableProperty]
        string tenantId;

        [ObservableProperty]
        string clientId;

        [ObservableProperty]
        string clientSecret;

        [ObservableProperty]
        string authenticationType;

        [ObservableProperty]
        string endpointUrl;

        [ObservableProperty]
        bool useSSL;

        [ObservableProperty]
        int timeout;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public CloudConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            cloudDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for CLOUD data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.CLOUD))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        cloudDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.CLOUD))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            if (cloudDatabaseTypes.Count > 0)
            {
                SelectedCloudDatabaseType = cloudDatabaseTypes[0];
                SelectedDataSourceType = cloudDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.CLOUD;
            SelectedCategoryValue = (int)DatasourceCategory.CLOUD;
            SelectedCategoryTextValue = DatasourceCategory.CLOUD.ToString();

            // Filter for CLOUD category
            Filters.Add(new AppFilter { FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue = Enum.GetName(DatasourceCategory.CLOUD), Operator = "=" });
            DBWork.Get(Filters);

            UseSSL = true;
            Timeout = 30000;
            AuthenticationType = "AccessKey";
        }

        [RelayCommand]
        public void SaveConnection()
        {
            if (Connection == null)
            {
                Connection = new ConnectionProperties();
            }
            Connection.ConnectionName = CurrentDataSourceName;
            Connection.ConnectionString = BuildConnectionString();
            Connection.DatabaseType= SelectedDataSourceType;
            Connection.Category = DatasourceCategory.CLOUD;
            if (SelectedCloudDatabaseType != null)
            {
                Connection.DriverName = SelectedCloudDatabaseType.PackageName;
                Connection.DriverVersion = SelectedCloudDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "Cloud connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"Cloud connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "Cloud connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"Cloud connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            AccountName = "";
            AccountKey = "";
            AccessKey = "";
            SecretKey = "";
            Region = "";
            BucketName = "";
            ContainerName = "";
            ProjectId = "";
            DatasetId = "";
            SubscriptionId = "";
            ResourceGroup = "";
            TenantId = "";
            ClientId = "";
            ClientSecret = "";
            EndpointUrl = "";
            UseSSL = true;
            Timeout = 30000;
            AuthenticationType = "AccessKey";
        }

        [RelayCommand]
        public void LoadConnection()
        {
            if (Selectedconnectionidx >= 0 && Selectedconnectionidx < DataConnections.Count)
            {
                Connection = DataConnections[Selectedconnectionidx];
                CurrentDataSourceName = Connection.ConnectionName;
                ParseConnectionString(Connection.ConnectionString);
                SelectedconnectionGuid = Connection.GuidID;
                SelectedDataSourceType = Connection.DatabaseType;
            }
        }

        private string BuildConnectionString()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            // Common cloud properties
            if (!string.IsNullOrEmpty(AccountName))
            {
                builder["AccountName"] = AccountName;
            }

            if (!string.IsNullOrEmpty(AccountKey))
            {
                builder["AccountKey"] = AccountKey;
            }

            if (!string.IsNullOrEmpty(AccessKey))
            {
                builder["AccessKey"] = AccessKey;
            }

            if (!string.IsNullOrEmpty(SecretKey))
            {
                builder["SecretKey"] = SecretKey;
            }

            if (!string.IsNullOrEmpty(Region))
            {
                builder["Region"] = Region;
            }

            if (!string.IsNullOrEmpty(BucketName))
            {
                builder["BucketName"] = BucketName;
            }

            if (!string.IsNullOrEmpty(ContainerName))
            {
                builder["ContainerName"] = ContainerName;
            }

            if (!string.IsNullOrEmpty(ProjectId))
            {
                builder["ProjectId"] = ProjectId;
            }

            if (!string.IsNullOrEmpty(DatasetId))
            {
                builder["DatasetId"] = DatasetId;
            }

            if (!string.IsNullOrEmpty(SubscriptionId))
            {
                builder["SubscriptionId"] = SubscriptionId;
            }

            if (!string.IsNullOrEmpty(ResourceGroup))
            {
                builder["ResourceGroup"] = ResourceGroup;
            }

            if (!string.IsNullOrEmpty(TenantId))
            {
                builder["TenantId"] = TenantId;
            }

            if (!string.IsNullOrEmpty(ClientId))
            {
                builder["ClientId"] = ClientId;
            }

            if (!string.IsNullOrEmpty(ClientSecret))
            {
                builder["ClientSecret"] = ClientSecret;
            }

            if (!string.IsNullOrEmpty(EndpointUrl))
            {
                builder["EndpointUrl"] = EndpointUrl;
            }

            builder["UseSSL"] = UseSSL;
            builder["Timeout"] = Timeout;
            builder["AuthenticationType"] = AuthenticationType;

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

                if (builder.ContainsKey("AccountName"))
                {
                    AccountName = builder["AccountName"].ToString();
                }

                if (builder.ContainsKey("AccountKey"))
                {
                    AccountKey = builder["AccountKey"].ToString();
                }

                if (builder.ContainsKey("AccessKey"))
                {
                    AccessKey = builder["AccessKey"].ToString();
                }

                if (builder.ContainsKey("SecretKey"))
                {
                    SecretKey = builder["SecretKey"].ToString();
                }

                if (builder.ContainsKey("Region"))
                {
                    Region = builder["Region"].ToString();
                }

                if (builder.ContainsKey("BucketName"))
                {
                    BucketName = builder["BucketName"].ToString();
                }

                if (builder.ContainsKey("ContainerName"))
                {
                    ContainerName = builder["ContainerName"].ToString();
                }

                if (builder.ContainsKey("ProjectId"))
                {
                    ProjectId = builder["ProjectId"].ToString();
                }

                if (builder.ContainsKey("DatasetId"))
                {
                    DatasetId = builder["DatasetId"].ToString();
                }

                if (builder.ContainsKey("SubscriptionId"))
                {
                    SubscriptionId = builder["SubscriptionId"].ToString();
                }

                if (builder.ContainsKey("ResourceGroup"))
                {
                    ResourceGroup = builder["ResourceGroup"].ToString();
                }

                if (builder.ContainsKey("TenantId"))
                {
                    TenantId = builder["TenantId"].ToString();
                }

                if (builder.ContainsKey("ClientId"))
                {
                    ClientId = builder["ClientId"].ToString();
                }

                if (builder.ContainsKey("ClientSecret"))
                {
                    ClientSecret = builder["ClientSecret"].ToString();
                }

                if (builder.ContainsKey("EndpointUrl"))
                {
                    EndpointUrl = builder["EndpointUrl"].ToString();
                }

                if (builder.ContainsKey("UseSSL"))
                {
                    UseSSL = Convert.ToBoolean(builder["UseSSL"]);
                }

                if (builder.ContainsKey("Timeout"))
                {
                    Timeout = Convert.ToInt32(builder["Timeout"]);
                }

                if (builder.ContainsKey("AuthenticationType"))
                {
                    AuthenticationType = builder["AuthenticationType"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            string baseUrl = "";
            switch (SelectedDataSourceType)
            {
                case DataSourceType.AzureCloud:
                    baseUrl = $"https://{AccountName}.blob.core.windows.net";
                    break;
                case DataSourceType.AWSRedshift:
                case DataSourceType.AWSRDS:
                case DataSourceType.AWSAthena:
                case DataSourceType.AWSGlue:
                    baseUrl = $"https://s3.{Region}.amazonaws.com/{BucketName}";
                    break;
                case DataSourceType.GoogleBigQuery:
                    baseUrl = $"https://bigquery.googleapis.com/projects/{ProjectId}";
                    break;
                default:
                    baseUrl = EndpointUrl;
                    break;
            }
            return baseUrl;
        }

        private void GetInstallDataSources()
        {
            InstalledDataSources = new List<AssemblyClassDefinition>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.CLOUD))
            {
                InstalledDataSources.Add(new AssemblyClassDefinition
                {
                    className = item.classHandler,
                    PackageName = item.PackageName,
                    Version = item.version
                });
            }
        }
    }
}
