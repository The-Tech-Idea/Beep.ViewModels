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
    public partial class NOSQLConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> nosqlDatabaseTypes;

        [ObservableProperty]
        ConnectionDriversConfig selectedNosqlDatabaseType;

        [ObservableProperty]
        string currentDataSourceName;

        [ObservableProperty]
        string databaseName;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string connectionString;

        [ObservableProperty]
        string userId;

        [ObservableProperty]
        string host;

        [ObservableProperty]
        int port;

        [ObservableProperty]
        string authenticationMethod;

        [ObservableProperty]
        bool useSSL;

        [ObservableProperty]
        string certificatePath;

        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;

        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;

        public NOSQLConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            dBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            DBWork.Get();
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            nosqlDatabaseTypes = new List<ConnectionDriversConfig>();

            GetInstallDataSources();

            // Filter for NOSQL data sources
            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.NOSQL))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    var ds = InstalledDataSources.Where(x => x.className == item.classHandler).FirstOrDefault();
                    if (ds != null)
                    {
                        packageNames.Add(item.PackageName);
                        nosqlDatabaseTypes.Add(item);
                    }
                }
            }

            foreach (var item in Editor.ConfigEditor.DataDriversClasses.Where(x => x.DatasourceCategory == DatasourceCategory.NOSQL))
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }

            // Set default filter for NOSQL category
            DBWork.Units.Filter = "Category = " + DatasourceCategory.NOSQL;
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
                    Editor.AddLogMessage("Beep", $"Error Saving NOSQL Connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
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
            Connection.Category = DatasourceCategory.NOSQL;
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
                    // Use ConnectionHelper to validate the connection
                    bool isValid = ConnectionHelper.IsConnectionStringValid(Connection.ConnectionString, Connection.DatabaseType);
                    if (isValid)
                    {
                        Editor.AddLogMessage("Beep", "NOSQL Connection test successful", DateTime.Now, -1, null, Errors.Ok);
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "NOSQL Connection test failed - invalid connection string", DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Beep", $"NOSQL Connection test failed - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                }
            }
        }

        [RelayCommand]
        public void RefreshConnections()
        {
            if (DBWork != null)
            {
                DBWork.Get();
                DBWork.Units.Filter = "Category = " + DatasourceCategory.NOSQL;
            }
        }
    }
}
