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
    public partial class BigDataConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> bigDataDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedBigDataDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string clusterName;

        [ObservableProperty]
        string masterNode;

        [ObservableProperty]
        string zookeeperQuorum;

        [ObservableProperty]
        string hdfsUri;

        [ObservableProperty]
        string yarnResourceManager;

        [ObservableProperty]
        string sparkMaster;

        [ObservableProperty]
        string kafkaBootstrapServers;

        [ObservableProperty]
        string schemaRegistryUrl;

        [ObservableProperty]
        string metastoreUri;

        [ObservableProperty]
        string warehousePath;

        [ObservableProperty]
        string namenodeHost;

        [ObservableProperty]
        int namenodePort;

        [ObservableProperty]
        int resourceManagerPort;

        [ObservableProperty]
        int metastorePort;

        [ObservableProperty]
        string authenticationType;

        [ObservableProperty]
        string principal;

        [ObservableProperty]
        string keytabPath;

        [ObservableProperty]
        bool useKerberos;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public BigDataConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            bigDataDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for BIGDATA data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.BigData))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        bigDataDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.BigData))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            if (bigDataDatabaseTypes.Count > 0)
            {
                SelectedBigDataDatabaseType = bigDataDatabaseTypes[0];
                SelectedDataSourceType = bigDataDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.BigData;
            SelectedCategoryValue = (int)DatasourceCategory.BigData;
            SelectedCategoryTextValue = DatasourceCategory.BigData.ToString();

            // Filter for BIGDATA category
            Filters.Add(new AppFilter { FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue =Enum.GetName(DatasourceCategory.BigData), Operator = "=" });
            DBWork.Get(Filters);

            NamenodePort = 9000;
            ResourceManagerPort = 8032;
            MetastorePort = 9083;
            UseKerberos = false;
            AuthenticationType = "Simple";
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
            Connection.DatabaseType = SelectedDataSourceType;
            Connection.Category = DatasourceCategory.BigData;
            if (SelectedBigDataDatabaseType != null)
            {
                Connection.DriverName = SelectedBigDataDatabaseType.PackageName;
                Connection.DriverVersion = SelectedBigDataDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "Big Data connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"Big Data connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "Big Data connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"Big Data connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            ClusterName = "";
            MasterNode = "";
            ZookeeperQuorum = "";
            HdfsUri = "";
            YarnResourceManager = "";
            SparkMaster = "";
            KafkaBootstrapServers = "";
            SchemaRegistryUrl = "";
            MetastoreUri = "";
            WarehousePath = "";
            NamenodeHost = "";
            NamenodePort = 9000;
            ResourceManagerPort = 8032;
            MetastorePort = 9083;
            AuthenticationType = "Simple";
            Principal = "";
            KeytabPath = "";
            UseKerberos = false;
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

            if (!string.IsNullOrEmpty(ClusterName))
            {
                builder["ClusterName"] = ClusterName;
            }

            if (!string.IsNullOrEmpty(MasterNode))
            {
                builder["MasterNode"] = MasterNode;
            }

            if (!string.IsNullOrEmpty(ZookeeperQuorum))
            {
                builder["ZookeeperQuorum"] = ZookeeperQuorum;
            }

            if (!string.IsNullOrEmpty(HdfsUri))
            {
                builder["HdfsUri"] = HdfsUri;
            }

            if (!string.IsNullOrEmpty(YarnResourceManager))
            {
                builder["YarnResourceManager"] = YarnResourceManager;
            }

            if (!string.IsNullOrEmpty(SparkMaster))
            {
                builder["SparkMaster"] = SparkMaster;
            }

            if (!string.IsNullOrEmpty(KafkaBootstrapServers))
            {
                builder["KafkaBootstrapServers"] = KafkaBootstrapServers;
            }

            if (!string.IsNullOrEmpty(SchemaRegistryUrl))
            {
                builder["SchemaRegistryUrl"] = SchemaRegistryUrl;
            }

            if (!string.IsNullOrEmpty(MetastoreUri))
            {
                builder["MetastoreUri"] = MetastoreUri;
            }

            if (!string.IsNullOrEmpty(WarehousePath))
            {
                builder["WarehousePath"] = WarehousePath;
            }

            if (!string.IsNullOrEmpty(NamenodeHost))
            {
                builder["NamenodeHost"] = NamenodeHost;
            }

            builder["NamenodePort"] = NamenodePort;
            builder["ResourceManagerPort"] = ResourceManagerPort;
            builder["MetastorePort"] = MetastorePort;
            builder["AuthenticationType"] = AuthenticationType;
            builder["UseKerberos"] = UseKerberos;

            if (!string.IsNullOrEmpty(Principal))
            {
                builder["Principal"] = Principal;
            }

            if (!string.IsNullOrEmpty(KeytabPath))
            {
                builder["KeytabPath"] = KeytabPath;
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

                if (builder.ContainsKey("ClusterName"))
                {
                    ClusterName = builder["ClusterName"].ToString();
                }

                if (builder.ContainsKey("MasterNode"))
                {
                    MasterNode = builder["MasterNode"].ToString();
                }

                if (builder.ContainsKey("ZookeeperQuorum"))
                {
                    ZookeeperQuorum = builder["ZookeeperQuorum"].ToString();
                }

                if (builder.ContainsKey("HdfsUri"))
                {
                    HdfsUri = builder["HdfsUri"].ToString();
                }

                if (builder.ContainsKey("YarnResourceManager"))
                {
                    YarnResourceManager = builder["YarnResourceManager"].ToString();
                }

                if (builder.ContainsKey("SparkMaster"))
                {
                    SparkMaster = builder["SparkMaster"].ToString();
                }

                if (builder.ContainsKey("KafkaBootstrapServers"))
                {
                    KafkaBootstrapServers = builder["KafkaBootstrapServers"].ToString();
                }

                if (builder.ContainsKey("SchemaRegistryUrl"))
                {
                    SchemaRegistryUrl = builder["SchemaRegistryUrl"].ToString();
                }

                if (builder.ContainsKey("MetastoreUri"))
                {
                    MetastoreUri = builder["MetastoreUri"].ToString();
                }

                if (builder.ContainsKey("WarehousePath"))
                {
                    WarehousePath = builder["WarehousePath"].ToString();
                }

                if (builder.ContainsKey("NamenodeHost"))
                {
                    NamenodeHost = builder["NamenodeHost"].ToString();
                }

                if (builder.ContainsKey("NamenodePort"))
                {
                    NamenodePort = Convert.ToInt32(builder["NamenodePort"]);
                }

                if (builder.ContainsKey("ResourceManagerPort"))
                {
                    ResourceManagerPort = Convert.ToInt32(builder["ResourceManagerPort"]);
                }

                if (builder.ContainsKey("MetastorePort"))
                {
                    MetastorePort = Convert.ToInt32(builder["MetastorePort"]);
                }

                if (builder.ContainsKey("AuthenticationType"))
                {
                    AuthenticationType = builder["AuthenticationType"].ToString();
                }

                if (builder.ContainsKey("UseKerberos"))
                {
                    UseKerberos = Convert.ToBoolean(builder["UseKerberos"]);
                }

                if (builder.ContainsKey("Principal"))
                {
                    Principal = builder["Principal"].ToString();
                }

                if (builder.ContainsKey("KeytabPath"))
                {
                    KeytabPath = builder["KeytabPath"].ToString();
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Error", $"Failed to parse connection string: {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
        }

        private string BuildUrl()
        {
            switch (SelectedDataSourceType)
            {
                case DataSourceType.Hadoop:
                    return $"hdfs://{NamenodeHost}:{NamenodePort}";
                case DataSourceType.Kafka:
                    return $"kafka://{KafkaBootstrapServers}";
                default:
                    return $"{SelectedDataSourceType.ToString().ToLower()}://{MasterNode}";
            }
        }

        private void GetInstallDataSources()
        {
            InstalledDataSources = new List<AssemblyClassDefinition>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.BigData))
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
