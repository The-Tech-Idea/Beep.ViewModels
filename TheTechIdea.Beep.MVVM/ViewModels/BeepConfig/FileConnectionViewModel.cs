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
    public partial class FileConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> fileDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedFileDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string filePath;

        [ObservableProperty]
        string fileName;

        [ObservableProperty]
        string fileExtension;

        [ObservableProperty]
        string delimiter;

        [ObservableProperty]
        bool hasHeaders;

        [ObservableProperty]
        string encoding;

        [ObservableProperty]
        string schemaFilePath;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public FileConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            dBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            fileDatabaseTypes = new List<ConnectionDriversConfig>();

            GetInstallDataSources();

            // Filter for FILE data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.FILE))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        fileDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.FILE))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            // Set default filter for FILE category using AppFilter
            Filters.Add(new AppFilter {FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue = Enum.GetName(DatasourceCategory.FILE), Operator = "=" });
            DBWork.Get(Filters);
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
                    Editor.AddLogMessage("Beep", $"Error Saving File Connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
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
            Connection.Category = DatasourceCategory.FILE;
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
                    // Check if file exists and is accessible
                    string fullPath = Path.Combine(Connection.FilePath ?? "", Connection.FileName ?? "");
                    if (File.Exists(fullPath))
                    {
                        Editor.AddLogMessage("Beep", "File connection test successful - file exists and is accessible", DateTime.Now, -1, null, Errors.Ok);
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "File connection test failed - file not found or inaccessible", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"File connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void BrowseFile()
        {
            // File browsing should be handled by the View through a service or dialog
            // This method can be used to trigger the file browse action from the View
            Editor.AddLogMessage("Beep", "File browse requested", DateTime.Now, -1, null, Errors.Ok);
        }

        [RelayCommand]
        public void RefreshConnections()
        {
            if (DBWork != null)
            {
                DBWork.Get();
                Filters.Clear();
                Filters.Add(new AppFilter {FieldName = "Category", FieldType = typeof(DatasourceCategory), FilterValue = Enum.GetName(DatasourceCategory.FILE), Operator = "=" });
                DBWork.Get(Filters);
            }
        }
    }
}
