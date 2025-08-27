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

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class WebAPIConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> webapiDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedWebapiDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string url;

        [ObservableProperty]
        string apiKey;

        [ObservableProperty]
        string apiSecret;

        [ObservableProperty]
        string bearerToken;

        [ObservableProperty]
        string authenticationType;

        [ObservableProperty]
        string contentType;

        [ObservableProperty]
        int timeout;

        [ObservableProperty]
        bool useProxy;

        [ObservableProperty]
        string proxyUrl;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public WebAPIConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            dBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            webapiDatabaseTypes = new List<ConnectionDriversConfig>();

            GetInstallDataSources();

            // Filter for WEBAPI data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.WEBAPI))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        webapiDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.WEBAPI))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            // Set default filter for WEBAPI category
            DBWork.Units.Filter = "Category = " + DatasourceCategory.WEBAPI;
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
        public void Save()
        {
            if (DBWork != null)
            {
                try
                {
                    Editor.ConfigEditor.DataConnections = DBWork.Units.ToList();
                    DBWork.Commit();
                    IsNew = false;
                    Editor.ConfigEditor.SaveDataconnectionsValues();
                    IsSaved = true;
                }
                catch (Exception ex)
                {
                    IsSaved = false;
                    Editor.AddLogMessage("Beep", $"Error Saving WebAPI Connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void UpdateConnection()
        {
            if (DBWork != null)
            {
                if (Connection != null)
                {
                    DBWork.Update(Connection);
                }
                Editor.ConfigEditor.SaveDataconnectionsValues();
                IsNew = false;
            }
        }

        [RelayCommand]
        public void AddNewConnection()
        {
            Connection = new ConnectionProperties();
            Connection.Category = DatasourceCategory.WEBAPI;
            Connection.GuidID = Guid.NewGuid().ToString();
            DBWork.Add(Connection);
            IsNew = true;
        }

        [RelayCommand]
        public void DeleteConnection()
        {
            if (Connection != null && DBWork != null)
            {
                DBWork.Delete(Connection);
                Save();
            }
        }

        [RelayCommand]
        public void TestConnection()
        {
            if (Connection != null)
            {
                try
                {
                    // Basic URL validation for WebAPI connections
                    if (!string.IsNullOrEmpty(Connection.Url))
                    {
                        Uri uriResult;
                        bool isValidUrl = Uri.TryCreate(Connection.Url, UriKind.Absolute, out uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                        if (isValidUrl)
                        {
                            Editor.AddLogMessage("Beep", "WebAPI connection URL validation successful", DateTime.Now, -1, null, Errors.Ok);
                        }
                        else
                        {
                            Editor.AddLogMessage("Beep", "WebAPI connection test failed - invalid URL format", DateTime.Now, -1, null, Errors.Failed);
                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "WebAPI connection test failed - URL is required", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"WebAPI connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void RefreshConnections()
        {
            if (DBWork != null)
            {
                DBWork.Get();
                DBWork.Units.Filter = "Category = " + DatasourceCategory.WEBAPI;
            }
        }
    }
}
