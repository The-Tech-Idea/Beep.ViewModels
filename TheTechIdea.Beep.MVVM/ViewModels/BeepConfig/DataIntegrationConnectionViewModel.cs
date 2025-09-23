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
    public partial class DataIntegrationConnectionViewModel : BaseViewModel
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

        // SSL Configuration Properties
        [ObservableProperty]
        bool enableSSL;

        [ObservableProperty]
        string sslVersion;

        [ObservableProperty]
        string clientCertificate;

        [ObservableProperty]
        string certificatePassword;

        [ObservableProperty]
        bool verifySSL;

        [ObservableProperty]
        string caCertificate;

        [ObservableProperty]
        int sslTimeout;

        // Common Data Integration Properties
        [ObservableProperty]
        string projectId;

        [ObservableProperty]
        string region;

        [ObservableProperty]
        string serviceAccountKey;

        [ObservableProperty]
        string serviceAccountEmail;

        [ObservableProperty]
        string apiEndpoint;

        [ObservableProperty]
        string apiVersion;

        [ObservableProperty]
        bool useApplicationDefaultCredentials;

        [ObservableProperty]
        string accessToken;

        [ObservableProperty]
        string refreshToken;

        [ObservableProperty]
        string clientId;

        [ObservableProperty]
        string clientSecret;

        [ObservableProperty]
        string tenantId;

        [ObservableProperty]
        string subscriptionId;

        [ObservableProperty]
        string resourceGroup;

        [ObservableProperty]
        string workspace;

        [ObservableProperty]
        string pipelineName;

        [ObservableProperty]
        string datasetName;

        [ObservableProperty]
        string linkedServiceName;

        [ObservableProperty]
        string integrationRuntime;

        [ObservableProperty]
        int requestTimeout;

        [ObservableProperty]
        int sessionTimeout;

        [ObservableProperty]
        int batchSize;

        [ObservableProperty]
        int bufferMemory;

        [ObservableProperty]
        string compressionType;

        [ObservableProperty]
        int retries;

        [ObservableProperty]
        int retryBackoff;

        [ObservableProperty]
        bool enableIdempotence;

        // DataFlow-specific Properties
        [ObservableProperty]
        string dataflowEndpoint;

        [ObservableProperty]
        string dataflowRegion;

        [ObservableProperty]
        string dataflowNetwork;

        [ObservableProperty]
        string dataflowSubnetwork;

        [ObservableProperty]
        string workerMachineType;

        [ObservableProperty]
        int numWorkers;

        [ObservableProperty]
        int maxWorkers;

        [ObservableProperty]
        string workerZone;

        [ObservableProperty]
        bool usePublicIps;

        [ObservableProperty]
        string tempLocation;

        [ObservableProperty]
        string stagingLocation;

        [ObservableProperty]
        string jobName;

        [ObservableProperty]
        string templateLocation;

        [ObservableProperty]
        string parameters;

        [ObservableProperty]
        string labels;

        [ObservableProperty]
        bool enableStreamingEngine;

        [ObservableProperty]
        string sdkContainerImage;

        [ObservableProperty]
        bool useFlexTemplate;

        // StreamSets-specific Properties
        [ObservableProperty]
        string streamSetsUrl;

        [ObservableProperty]
        string username;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string authType;

        [ObservableProperty]
        string ldapUrl;

        [ObservableProperty]
        string ldapBaseDn;

        [ObservableProperty]
        string ldapBindDn;

        [ObservableProperty]
        string ldapBindPassword;

        [ObservableProperty]
        bool enableSaml;

        [ObservableProperty]
        string samlMetadataUrl;

        [ObservableProperty]
        string samlEntityId;

        [ObservableProperty]
        string defaultPipelineEngine;

        [ObservableProperty]
        string runtimeParameters;

        [ObservableProperty]
        string pipelineLibrary;

        [ObservableProperty]
        bool enableMetrics;

        [ObservableProperty]
        string metricsEndpoint;

        [ObservableProperty]
        int metricsInterval;

        [ObservableProperty]
        bool enableAlerts;

        [ObservableProperty]
        string alertsWebhookUrl;

        [ObservableProperty]
        string executionMode;

        [ObservableProperty]
        int workerCount;

        [ObservableProperty]
        string workerType;

        [ObservableProperty]
        bool enableClusterMode;

        [ObservableProperty]
        string clusterToken;

        [ObservableProperty]
        string clusterLabels;

        // Monitoring Properties
        [ObservableProperty]
        bool enableLogging;

        [ObservableProperty]
        string logLevel;

        [ObservableProperty]
        bool enableTracing;

        [ObservableProperty]
        string tracingEndpoint;

        public DataIntegrationConnectionViewModel()
        {
            
        }

      

        public void SetParameterValue(string parameterName, object value)
        {
            var property = this.GetType().GetProperty(parameterName);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(this, convertedValue);
                }
                catch (Exception ex)
                {
                    // Log error if needed
                    Console.WriteLine($"Error setting parameter {parameterName}: {ex.Message}");
                }
            }
        }

        public T GetParameterValue<T>(string parameterName)
        {
            var property = this.GetType().GetProperty(parameterName);
            if (property != null && property.CanRead)
            {
                try
                {
                    var value = property.GetValue(this);
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    // Log error if needed
                    Console.WriteLine($"Error getting parameter {parameterName}: {ex.Message}");
                    return default(T);
                }
            }
            return default(T);
        }
    }
}