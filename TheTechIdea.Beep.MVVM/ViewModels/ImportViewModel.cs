using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Vis.Modules;
using DataManagementModels.Editor;
using System.Collections.Generic;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Helpers;
using DataManagementModels.DriversConfigurations;
using TheTechIdea.Util;
using TheTechIdea.Beep.Report;
using System.IO;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class ImportViewModel:BaseViewModel
    {
        [ObservableProperty]
        EntityStructure importedEntity;
        [ObservableProperty]
        IDataSource importedDataSource;
        [ObservableProperty]
        string filename;
        [ObservableProperty]
        string importpath;
        [ObservableProperty]
        string importedEntityName;
        [ObservableProperty]
        string importedDataSourceName;
        [ObservableProperty]
        string importExtension;
        [ObservableProperty]
        ConnectionDriversConfig importDriverConfig;
        [ObservableProperty]
        List<ConnectionDriversConfig> driversConfigs;
        [ObservableProperty]
        ConnectionProperties cn;
        [ObservableProperty]
        List<AppFilter> appFilters;
        [ObservableProperty]
        List<object> data;
        public ImportViewModel(IDMEEditor dMEEditor, IVisManager visManager) : base(dMEEditor, visManager)
        {

        }
        public bool Init()
        {
            AppFilters = new List<AppFilter>();
            DriversConfigs = FileHelper.GetFileDataSources(Editor);
            if (string.IsNullOrEmpty(CurrentEntityName) || string.IsNullOrEmpty(CurrentDataSourceName))
            {
                return false;
            }
            if (Editor != null)
            {
                CurrDataSource = Editor.GetDataSource(CurrentDataSourceName);
                if (CurrDataSource != null)
                {
                    CurrentEntity = CurrDataSource.GetEntityStructure(CurrentEntityName, true);
                    if (CurrentEntity != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ImportFile()
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(Importpath) || (ImportDriverConfig == null))
            {
                Editor.AddLogMessage("Beep", $"Imported filename and path missing", System.DateTime.Now, -1, "", Errors.Failed);
                return false;
            }
            if (string.IsNullOrEmpty(ImportedEntityName) || string.IsNullOrEmpty(ImportedDataSourceName))
            {
                Editor.AddLogMessage("Beep", $"Imported Entityname and Datasourcename missing", System.DateTime.Now, -1, "", Errors.Failed);
                return false;
            }

            if (Editor != null)
            {
                string file = Path.Combine(Importpath, Filename);
                Cn = Editor.Utilfunction.CreateFileDataConnection(file);
                // Creating File DataSource
                ImportedDataSource = Editor.CreateNewDataSourceConnection(Cn, ImportedDataSourceName);
                CurrDataSource = Editor.GetDataSource(CurrentDataSourceName);
                // if the Current DataSource to be Imported is available
                if (CurrDataSource != null && ImportedDataSource != null)
                {
                    if (CurrDataSource.Openconnection() != System.Data.ConnectionState.Open)
                    {
                        Editor.AddLogMessage("Beep", $"Current DataSource {CurrentDataSourceName}", System.DateTime.Now, -1, "", Errors.Failed);
                        return false;
                    }

                    // Get the Entity to be Imported
                    ImportedEntity = ImportedDataSource.GetEntityStructure(ImportedEntityName, false);
                    if (ImportedEntity == null)
                    {
                        CurrDataSource.CreateEntityAs(ImportedEntity);
                    }
                    if (ImportedEntity != null)
                    {
                        // Import the Entity to the File

                        if (Data != null)
                        {
                            
                            CurrDataSource.UpdateEntities(CurrentEntity.EntityName, Data, Waitprogress);
                        }

                    }
                }
            }
            return false;
        }
        public void SetImportPath(string path)
        {
            Importpath = path;
        }
        public void SetImportFileName(string filename)
        {
            Filename = filename;
        }
        public void SetImportExtension(string ext)
        {
            ImportExtension = ext;
        }
        public void SetImportDriverConfig(ConnectionDriversConfig driverConfig)
        {
            ImportDriverConfig = driverConfig;
        }
        public void SetImportData(List<object> pdata)
        {
            Data = pdata;
        }
    }
}
