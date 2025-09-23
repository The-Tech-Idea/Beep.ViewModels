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
    public partial class StreamProcessingConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> streamProcessingDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedStreamProcessingDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string bootstrapServers;

        [ObservableProperty]
        string zookeeperConnect;

        [ObservableProperty]
        string groupId;

        [ObservableProperty]
        string topicName;

        [ObservableProperty]
        string schemaRegistryUrl;

        [ObservableProperty]
        string consumerGroup;

        [ObservableProperty]
        string producerConfig;

        [ObservableProperty]
        string consumerConfig;

        [ObservableProperty]
        int batchSize;

        [ObservableProperty]
        int lingerMs;

        [ObservableProperty]
        int bufferMemory;

        [ObservableProperty]
        string autoOffsetReset;

        [ObservableProperty]
        bool enableAutoCommit;

        [ObservableProperty]
        int sessionTimeoutMs;

        [ObservableProperty]
        int requestTimeoutMs;

        [ObservableProperty]
        string securityProtocol;

        [ObservableProperty]
        string saslMechanism;

        [ObservableProperty]
        string saslUsername;

        [ObservableProperty]
        string saslPassword;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        [ObservableProperty]
        DataSourceType selectedDataSourceType;

        [ObservableProperty]
        Array dataSourceTypes;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public StreamProcessingConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            DataSourceTypes = Enum.GetValues(typeof(DataSourceType));
            PackageNames = new List<string>();
            packageVersions = new List<string>();
            streamProcessingDatabaseTypes = new List<ConnectionDriversConfig>();
            GetInstallDataSources();

            // Filter for STREAM and QUEUE data sources
            var streamCategories = new[] { DatasourceCategory.STREAM, DatasourceCategory.QUEUE, DatasourceCategory.StreamProcessing };
            foreach (var category in streamCategories)
            {
                foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == category))
                {
                    if (!string.IsNullOrEmpty(item.PackageName))
                    {
                        var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                        if (ds != null)
                        {
                            PackageNames.Add(item.PackageName);
                            streamProcessingDatabaseTypes.Add(item);
                        }
                    }
                }
            }

            foreach (var category in streamCategories)
            {
                foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == category))
                {
                    if (!string.IsNullOrEmpty(item.PackageName))
                    {
                        packageVersions.Add(item.version);
                    }
                }
            }

            if (streamProcessingDatabaseTypes.Count > 0)
            {
                SelectedStreamProcessingDatabaseType = streamProcessingDatabaseTypes[0];
                SelectedDataSourceType = streamProcessingDatabaseTypes[0].DatasourceType;
            }

            SelectedCategoryItem = DatasourceCategory.StreamProcessing;
            SelectedCategoryValue = (int)DatasourceCategory.StreamProcessing;
            SelectedCategoryTextValue = DatasourceCategory.StreamProcessing.ToString();

            // Filter for STREAM, QUEUE and StreamProcessing categories
            var categoryFilter = new AppFilter
            {
                FieldName = "Category",
                FieldType = typeof(DatasourceCategory),
                Operator = "IN",
                FilterValue = $"{(int)DatasourceCategory.STREAM},{(int)DatasourceCategory.QUEUE},{(int)DatasourceCategory.StreamProcessing}"
            };
            Filters.Add(categoryFilter);
            DBWork.Get(Filters);

            BatchSize = 16384;
            LingerMs = 5;
            BufferMemory = 33554432; // 32MB
            AutoOffsetReset = "earliest";
            EnableAutoCommit = true;
            SessionTimeoutMs = 30000;
            RequestTimeoutMs = 40000;
            SecurityProtocol = "PLAINTEXT";
            SaslMechanism = "PLAIN";
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
            Connection.Category = SelectedCategoryItem;
            if (SelectedStreamProcessingDatabaseType != null)
            {
                Connection.DriverName = SelectedStreamProcessingDatabaseType.PackageName;
                Connection.DriverVersion = SelectedStreamProcessingDatabaseType.version;
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
                            Editor.AddLogMessage("Beep", "Stream Processing connection test successful", DateTime.Now, -1, null, Errors.Ok);
                            dataSource.Closeconnection();
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", $"Stream Processing connection test failed - {connectionState}", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "Stream Processing connection test failed - could not create data source", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"Stream Processing connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void CreateNewConnection()
        {
            Connection = new ConnectionProperties();
            CurrentDataSourceName = "";
            BootstrapServers = "";
            ZookeeperConnect = "";
            GroupId = "";
            TopicName = "";
            SchemaRegistryUrl = "";
            ConsumerGroup = "";
            ProducerConfig = "";
            ConsumerConfig = "";
            BatchSize = 16384;
            LingerMs = 5;
            BufferMemory = 33554432;
            AutoOffsetReset = "earliest";
            EnableAutoCommit = true;
            SessionTimeoutMs = 30000;
            RequestTimeoutMs = 40000;
            SecurityProtocol = "PLAINTEXT";
            SaslMechanism = "PLAIN";
            SaslUsername = "";
            SaslPassword = "";
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

            if (!string.IsNullOrEmpty(BootstrapServers))
            {
                builder["BootstrapServers"] = BootstrapServers;
            }

            if (!string.IsNullOrEmpty(ZookeeperConnect))
            {
                builder["ZookeeperConnect"] = ZookeeperConnect;
            }

            if (!string.IsNullOrEmpty(GroupId))
            {
                builder["GroupId"] = GroupId;
            }

            if (!string.IsNullOrEmpty(TopicName))
            {
                builder["TopicName"] = TopicName;
            }

            if (!string.IsNullOrEmpty(SchemaRegistryUrl))
            {
                builder["SchemaRegistryUrl"] = SchemaRegistryUrl;
            }

            if (!string.IsNullOrEmpty(ConsumerGroup))
            {
                builder["ConsumerGroup"] = ConsumerGroup;
            }

            if (!string.IsNullOrEmpty(ProducerConfig))
            {
                builder["ProducerConfig"] = ProducerConfig;
            }

            if (!string.IsNullOrEmpty(ConsumerConfig))
            {
                builder["ConsumerConfig"] = ConsumerConfig;
            }

            builder["BatchSize"] = BatchSize;
            builder["LingerMs"] = LingerMs;
            builder["BufferMemory"] = BufferMemory;
            builder["AutoOffsetReset"] = AutoOffsetReset;
            builder["EnableAutoCommit"] = EnableAutoCommit;
            builder["SessionTimeoutMs"] = SessionTimeoutMs;
            builder["RequestTimeoutMs"] = RequestTimeoutMs;
            builder["SecurityProtocol"] = SecurityProtocol;
            builder["SaslMechanism"] = SaslMechanism;

            if (!string.IsNullOrEmpty(SaslUsername))
            {
                builder["SaslUsername"] = SaslUsername;
            }

            if (!string.IsNullOrEmpty(SaslPassword))
            {
                builder["SaslPassword"] = SaslPassword;
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

                if (builder.ContainsKey("BootstrapServers"))
                {
                    BootstrapServers = builder["BootstrapServers"].ToString();
                }

                if (builder.ContainsKey("ZookeeperConnect"))
                {
                    ZookeeperConnect = builder["ZookeeperConnect"].ToString();
                }

                if (builder.ContainsKey("GroupId"))
                {
                    GroupId = builder["GroupId"].ToString();
                }

                if (builder.ContainsKey("TopicName"))
                {
                    TopicName = builder["TopicName"].ToString();
                }

                if (builder.ContainsKey("SchemaRegistryUrl"))
                {
                    SchemaRegistryUrl = builder["SchemaRegistryUrl"].ToString();
                }

                if (builder.ContainsKey("ConsumerGroup"))
                {
                    ConsumerGroup = builder["ConsumerGroup"].ToString();
                }

                if (builder.ContainsKey("ProducerConfig"))
                {
                    ProducerConfig = builder["ProducerConfig"].ToString();
                }

                if (builder.ContainsKey("ConsumerConfig"))
                {
                    ConsumerConfig = builder["ConsumerConfig"].ToString();
                }

                if (builder.ContainsKey("BatchSize"))
                {
                    BatchSize = Convert.ToInt32(builder["BatchSize"]);
                }

                if (builder.ContainsKey("LingerMs"))
                {
                    LingerMs = Convert.ToInt32(builder["LingerMs"]);
                }

                if (builder.ContainsKey("BufferMemory"))
                {
                    BufferMemory = Convert.ToInt32(builder["BufferMemory"]);
                }

                if (builder.ContainsKey("AutoOffsetReset"))
                {
                    AutoOffsetReset = builder["AutoOffsetReset"].ToString();
                }

                if (builder.ContainsKey("EnableAutoCommit"))
                {
                    EnableAutoCommit = Convert.ToBoolean(builder["EnableAutoCommit"]);
                }

                if (builder.ContainsKey("SessionTimeoutMs"))
                {
                    SessionTimeoutMs = Convert.ToInt32(builder["SessionTimeoutMs"]);
                }

                if (builder.ContainsKey("RequestTimeoutMs"))
                {
                    RequestTimeoutMs = Convert.ToInt32(builder["RequestTimeoutMs"]);
                }

                if (builder.ContainsKey("SecurityProtocol"))
                {
                    SecurityProtocol = builder["SecurityProtocol"].ToString();
                }

                if (builder.ContainsKey("SaslMechanism"))
                {
                    SaslMechanism = builder["SaslMechanism"].ToString();
                }

                if (builder.ContainsKey("SaslUsername"))
                {
                    SaslUsername = builder["SaslUsername"].ToString();
                }

                if (builder.ContainsKey("SaslPassword"))
                {
                    SaslPassword = builder["SaslPassword"].ToString();
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
                case DataSourceType.Kafka:
                    return $"kafka://{BootstrapServers}";
                case DataSourceType.RabbitMQ:
                    return $"amqp://{BootstrapServers}";
                case DataSourceType.ActiveMQ:
                    return $"tcp://{BootstrapServers}";
                case DataSourceType.Nats:
                    return $"nats://{BootstrapServers}";
                default:
                    return $"{SelectedDataSourceType.ToString().ToLower()}://{BootstrapServers}";
            }
        }

        private void GetInstallDataSources()
        {
            installedDataSources = new List<AssemblyClassDefinition>();
            var streamCategories = new[] { DatasourceCategory.STREAM, DatasourceCategory.QUEUE, DatasourceCategory.StreamProcessing };
            foreach (var category in streamCategories)
            {
                foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == category))
                {
                    installedDataSources.Add(new AssemblyClassDefinition
                    {
                        className = item.classHandler,
                        PackageName = item.PackageName,
                        Version = item.version
                    });
                }
            }
        }
    }
}
