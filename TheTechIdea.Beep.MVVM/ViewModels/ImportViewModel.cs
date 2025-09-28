using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;
using System.Collections.Generic;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Report;
using System.IO;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Helpers.FileandFolderHelpers;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    
    [Addin(Caption = "Beep BaseViewModel", Name = "BaseViewModel", addinType = AddinType.Class)]
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
        [ObservableProperty]
        bool entityExistinDestination;

        DataImportManager dataImportManager;

        

        public ImportViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            dataImportManager=new DataImportManager(dMEEditor);

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
                        EntityExistinDestination=true;
                        IsStopped = false;
                        Editor.Logger.LogInfo($"Import: Found Imported Entity {CurrentEntity.EntityName}");
                        return true;
                    }
                    else
                    {
                        EntityExistinDestination = false;
                        IsStopped=false;
                        Editor.Logger.LogInfo($"Import:  Imported Entity {CurrentEntity.EntityName} not found going to create it");
                    }
                }
                else
                {
                    EntityExistinDestination = false;
                    IsStopped =true;
                    Editor.Logger.LogCritical($"Import:  DataSource {CurrentDataSourceName} could not be found or unable to connect to it");
                    return false;
                }
            }
            return false;
        }
        public bool ImportFile()
        {
            if (IsStopped)
            {
                return false;
            }
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
                        bool created=CurrDataSource.CreateEntityAs(ImportedEntity);
                        if (created)
                        {
                            CurrentEntity = ImportedEntity;
                        }
                        else {
                            CurrentEntity = CurrDataSource.GetEntityStructure(CurrentEntity, false);
                        }
                    }
                    if (ImportedEntity != null)
                    {
                        // Import the Entity to the File

                        //if (Data != null)
                        //{
                            
                        //    CurrDataSource.UpdateEntities(CurrentEntity.EntityName, Data, Waitprogress);
                        //}
                        dataImportManager.DestEntityStructure=CurrentEntity;
                        dataImportManager.SourceEntityStructure=ImportedEntity;
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
            ImportExtension = Path.GetExtension(Filename);
            ImportDriverConfig =FileHelper.ExtensionExists(Editor, ImportExtension);  
        }
        public void SetImportExtension(string ext)
        {
            ImportExtension = ext;
            ImportDriverConfig = FileHelper.ExtensionExists(Editor, ext);
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
