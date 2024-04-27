using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using Beep.Vis.Module;
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
    public partial class ExportViewModel: BaseViewModel
    {
        [ObservableProperty]
        EntityStructure exportedEntity;
        [ObservableProperty]
        IDataSource exportedDataSource;
        [ObservableProperty]
        string filename;
        [ObservableProperty]
        string exportpath;
        [ObservableProperty]
        string exportedEntityName;
        [ObservableProperty]
        string exportedDataSourceName;
        [ObservableProperty]
        string exportExtension;
        [ObservableProperty]
        ConnectionDriversConfig exportDriverConfig;
        [ObservableProperty]
        List<ConnectionDriversConfig> driversConfigs;
        [ObservableProperty]
        ConnectionProperties cn;
        [ObservableProperty ]
        List<AppFilter> appFilters;
        [ObservableProperty]
        List<object> data;
        public ExportViewModel(IDMEEditor dMEEditor, IVisManager visManager) : base(dMEEditor, visManager)
        {

        }
        public bool Init()
        {
            AppFilters = new List<AppFilter>();
            DriversConfigs =FileHelper.GetFileDataSources(Editor);
            if (string.IsNullOrEmpty(CurrentEntityName) || string.IsNullOrEmpty(CurrentDataSourceName))
            {
                return false;
            }
            if (Editor != null)
            {
                CurrDataSource = Editor.GetDataSource(CurrentDataSourceName);
                if(CurrDataSource != null)
                {
                    CurrentEntity = CurrDataSource.GetEntityStructure(CurrentEntityName,true);
                    if(CurrentEntity != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ExportToFile()
        {
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(Exportpath)||(ExportDriverConfig==null))
            {
                Editor.AddLogMessage("Beep", $"Exported filename and path missing", System.DateTime.Now, -1, "", Errors.Failed);
                return false;
            }
            if (string.IsNullOrEmpty(ExportedEntityName) || string.IsNullOrEmpty(ExportedDataSourceName))
            {
                Editor.AddLogMessage("Beep", $"Exported Entityname and Datasourcename missing", System.DateTime.Now, -1, "", Errors.Failed);
                return false;
            }
          
            if (Editor != null)
            {
                string file=Path.Combine(Exportpath,Filename    );
                Cn = Editor.Utilfunction.CreateFileDataConnection(file);
                // Creating File DataSource
                ExportedDataSource = Editor.CreateNewDataSourceConnection(Cn,ExportedDataSourceName);
                CurrDataSource=Editor.GetDataSource(CurrentDataSourceName);
                // if the Current DataSource to be exported is available
                if (CurrDataSource != null && ExportedDataSource!=null)
                {
                    if (CurrDataSource.Openconnection()!= System.Data.ConnectionState.Open)
                    {
                        Editor.AddLogMessage("Beep",$"Current DataSource {CurrentDataSourceName}", System.DateTime.Now, -1, "", Errors.Failed);
                        return false;
                    }
                    
                    // Get the Entity to be exported
                    CurrentEntity = CurrDataSource.GetEntityStructure(ExportedEntityName, true);
                    if (CurrentEntity != null)
                    {
                        // Export the Entity to the File
                        
                        if (Data != null) {
                            ExportedDataSource.UpdateEntities(CurrentEntity.EntityName, Data, Waitprogress);
                        }
                        
                    }
                }
            }
            return false;
        }
        public void SetExportPath(string path)
        {
            Exportpath = path;
        }
        public void SetExportFileName(string filename)
        {
            Filename = filename;
        }
        public void SetExportExtension(string ext)
        {
            ExportExtension = ext;
        }
        public void SetExportDriverConfig(ConnectionDriversConfig driverConfig)
        {
            ExportDriverConfig = driverConfig;
        }
        public void SetExportData(List<object> pdata)
        {
            Data=pdata;
        }
    }
}
