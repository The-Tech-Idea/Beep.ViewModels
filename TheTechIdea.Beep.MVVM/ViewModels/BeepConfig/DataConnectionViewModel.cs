using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataManagementModels.DriversConfigurations;
using DataManagementModels.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Util;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.InMemory;
using DataManagementModels.DataBase;


namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DataConnectionViewModel : BaseViewModel
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
        List<ConnectionDriversConfig> embeddedDatabaseTypes;
        [ObservableProperty]
        ConnectionDriversConfig selectedEmbeddedDatabaseType;
        [ObservableProperty]
        List<ConnectionDriversConfig> inMemoryDatabaseTypes;
        [ObservableProperty]
        ConnectionDriversConfig selectedinMemoryDatabaseType;
        [ObservableProperty]
        string embeddedDatabaseType;
        [ObservableProperty]
        string embeddedDatabaseTypeGuid;
        [ObservableProperty]
        StorageFolders selectedFolder;
        [ObservableProperty]
        string installFolderName;
        [ObservableProperty]
        string installFolderGuid;
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
        string installFolderPath;
        
      
        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;
        public DataConnectionViewModel(IDMEEditor dMEEditor, IVisManager visManager) : base(dMEEditor, visManager)
        {
            //  DBWork = new UnitofWork<ConnectionDriversConfig>(DMEEditor, true, new ObservableBindingList<ConnectionDriversConfig>(Editor.ConfigEditor.DataDriversClasses), "GuidID");
            dBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            ConnectionProperties connection = new ConnectionProperties();

            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            embeddedDatabaseTypes = new List<ConnectionDriversConfig>();
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {

                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageNames.Add(item.PackageName);
                }
            }
            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (!string.IsNullOrEmpty(item.PackageName))
                {
                    packageVersions.Add(item.version);
                }
            }
            foreach (ConnectionDriversConfig cls in Editor.ConfigEditor.DataDriversClasses.Where(x => x.CreateLocal == true))
            {
                embeddedDatabaseTypes.Add(cls);
            }
            GetInMemoryDriverConfigs();
            
        }
        [RelayCommand]
        public void Save()
        {
            if (DBWork != null)
            {
                //if (Connection != null)
                //{
                //    if (IsNew)
                //    {
                //        DBWork.Create(Connection);
                //    }
                //    else
                //    {
                //        DBWork.Update(Connection);
                //    }

                //}
                try
                {
                    DBWork.Commit();
                    IsNew = false;
                    Editor.ConfigEditor.DataConnections = DBWork.Units.ToList();
                    Editor.ConfigEditor.SaveDataconnectionsValues();
                    IsSaved = true;
                }
                catch (Exception ex)
                {
                    IsSaved = false;
                    Editor.AddLogMessage("Beep", $"Error Saving Connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
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
        public void Add()
        {
            Connection = new ConnectionProperties();
            if (DBWork != null)
            {
                DBWork.Create(Connection);
            }
        }
        [RelayCommand]
        public void Delete()
        {
            if (DBWork != null)
            {
                if (Connection != null)
                {
                    DBWork.Delete(Connection);
                }

            }
        }
        [RelayCommand]
        public void Get()
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
                DBWork.Get();
            }
        }
        [RelayCommand]
        public void GetByGuid()
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");

            }
            if (SelectedconnectionGuid != null)
            {
                if (!string.IsNullOrEmpty(SelectedconnectionGuid))
                {
                    DBWork.Get(new List<AppFilter>() { new AppFilter() { FieldName = "GuidID", FilterValue = $"{SelectedconnectionGuid}", Operator = "=" } });
                }
            }
            if (DBWork.Units.Count > 0)
            {
                Connection = DBWork.Units[0];
            }

        }
        [RelayCommand]
        public void Filter()
        {
            if (DBWork != null)
            {
                if (string.IsNullOrEmpty(SelectedCategoryTextValue))
                {
                    DBWork.Get();
                }
                else
                {
                    Filters = new List<AppFilter>() { new AppFilter() { FieldName = "Category", FilterValue = selectedCategoryTextValue.ToUpper().ToString(), Operator = "=" } };
                    DBWork.Get(Filters);


                }

            }
        }
        [RelayCommand]
        public void GetConnection()
        {
            if (DBWork != null)
            {
                if (Connection != null)
                {
                    Selectedconnectionidx = DBWork.Getindex(Connection.GuidID);
                }
                if (Selectedconnectionidx > -1)
                {
                    Connection = DBWork.Units[Selectedconnectionidx];
                }
            }
        }
        [RelayCommand]
        public void GetFields()
        {
            if (DBWork.EntityStructure != null)
            {
                Fields = DBWork.EntityStructure.Fields;
            }
            else
            {
                Fields = null;
            }
        }
        [RelayCommand]
        public void CreateLocalConnection()
        {
            if (DBWork != null)
            {
                try
                {
                    IsSaved = false;
                    IsNew = true;
                    IsCreated = false;
                    Connection = new ConnectionProperties();
                    if (SelectedEmbeddedDatabaseType != null)
                    {
                        Connection.Category = SelectedEmbeddedDatabaseType.DatasourceCategory;//(DatasourceCategory)(int) Enum.Parse(typeof( DatasourceCategory),CategorycomboBox.Text);
                        Connection.DatabaseType = SelectedEmbeddedDatabaseType.DatasourceType; //(DataSourceType)(int)Enum.Parse(typeof(DataSourceType), DatabaseTypecomboBox.Text);
                        Connection.ConnectionName = DatabaseName;
                        Connection.DriverName = SelectedEmbeddedDatabaseType.PackageName;
                        Connection.DriverVersion = SelectedEmbeddedDatabaseType.version;
                        if (Editor.ConfigEditor.DataConnections.Count == 0)
                        {
                            Connection.ID = 1;
                        }
                        else
                        {
                            Connection.ID = Editor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
                        }

                        Connection.FilePath = ".\\" + SelectedFolder.FolderPath;
                        Connection.FileName = DatabaseName;
                        Connection.IsLocal = true;

                        Connection.ConnectionString = SelectedEmbeddedDatabaseType.ConnectionString; //Path.Combine(Connection.FilePath, Connection.FileName);
                        if (Connection.FilePath.Contains(Editor.ConfigEditor.ExePath))
                        {
                            Connection.FilePath.Replace(Editor.ConfigEditor.ExePath, ".");
                        }
                        Connection.UserID = "";
                        Connection.Password = Password;
                      //  DBWork.Create(Connection);
                        Save();
                        //-----------------------------------------------------
                        IDataSource ds = Editor.CreateLocalDataSourceConnection(Connection, DatabaseName, SelectedEmbeddedDatabaseType.classHandler);
                        if (IsSaved)
                        {
                            if (ds != null)
                            {
                                ILocalDB dB = (ILocalDB)ds;
                                bool ok = dB.CreateDB();
                                if (ok)
                                {
                                    //ds.ConnectionStatus = ds.Dataconnection.OpenConnection();
                                    Editor.OpenDataSource(Connection.ConnectionName);
                                }
                                IsCreated = true;
                            }
                            else
                            {
                                Editor.AddLogMessage("Beep", $"Could not Create Local/Embedded Database", DateTime.Now, -1, null, Errors.Failed);
                            }
                        }
                        
                        //-----------------------------------------------------
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", $"Error could not find databse drivers for  Local DB  ", DateTime.Now, -1, null, Errors.Failed);

                    }
                }
                catch (Exception ex)
                {

                    Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);

                }

            }
        }
        [RelayCommand]
        public void CreateLocalConnectionUsingPath()
        {
            if (!string.IsNullOrEmpty(InstallFolderPath))
            {
                if (InstallFolderPath.StartsWith(".") || InstallFolderPath.Equals("/") || InstallFolderPath.Equals("\\"))
                {
                    string fullPath = Path.Combine(Editor.ConfigEditor.ExePath, InstallFolderPath.TrimStart('.', '/', '\\'));
                    InstallFolderPath = fullPath;
                    //  = ConnectionProp.FilePath.Replace(".", DMEEditor.ConfigEditor.ExePath);
                }
                if (!Directory.Exists(installFolderPath))
                {
                    ErrorObject.Flag = Errors.Failed;
                    ErrorObject.Message = $"Folder {installFolderPath} does not exist";
                    return;
                }

            }
            if (DBWork != null && !string.IsNullOrEmpty(InstallFolderPath))
            {
                try
                {
                    IsSaved = false;
                    IsNew = true;
                    IsCreated = false;
                     Add();
                    if (SelectedEmbeddedDatabaseType != null)
                    {
                        Connection.Category = SelectedEmbeddedDatabaseType.DatasourceCategory;//(DatasourceCategory)(int) Enum.Parse(typeof( DatasourceCategory),CategorycomboBox.Text);
                        Connection.DatabaseType = SelectedEmbeddedDatabaseType.DatasourceType; //(DataSourceType)(int)Enum.Parse(typeof(DataSourceType), DatabaseTypecomboBox.Text);
                        Connection.ConnectionName = DatabaseName;
                        Connection.DriverName = SelectedEmbeddedDatabaseType.PackageName;
                        Connection.DriverVersion = SelectedEmbeddedDatabaseType.version;
                        if (Editor.ConfigEditor.DataConnections.Count == 0)
                        {
                            Connection.ID = 1;
                        }
                        else
                        {
                            Connection.ID = Editor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
                        }

                        Connection.FilePath = $@"{InstallFolderPath}";
                        Connection.FileName = DatabaseName;
                        Connection.IsLocal = true;

                        Connection.ConnectionString = SelectedEmbeddedDatabaseType.ConnectionString; //Path.Combine(Connection.FilePath, Connection.FileName);
                        if (Connection.FilePath.Contains(Editor.ConfigEditor.ExePath))
                        {
                            Connection.FilePath.Replace(Editor.ConfigEditor.ExePath, ".");
                        }
                        Connection.UserID = "";
                        Connection.Password = Password;
              //          DBWork.Create(Connection);
                        Save();
                        //-----------------------------------------------------
                        Save();
                        //-----------------------------------------------------
                        IDataSource ds = Editor.CreateLocalDataSourceConnection(Connection, DatabaseName, SelectedEmbeddedDatabaseType.classHandler);
                        if (IsSaved)
                        {
                            if (ds != null)
                            {
                                ILocalDB dB = (ILocalDB)ds;
                                bool ok = dB.CreateDB();
                                if (ok)
                                {
                                    //ds.ConnectionStatus = ds.Dataconnection.OpenConnection();
                                    Editor.OpenDataSource(Connection.ConnectionName);
                                }
                                IsCreated = true;
                            }
                            else
                            {
                                Editor.AddLogMessage("Beep", $"Could not Create Local/Embedded Database", DateTime.Now, -1, null, Errors.Failed);
                            }
                        }
                   
                        else
                        {
                            Editor.AddLogMessage("Beep", $"Could not Create Local/Embedded Database", DateTime.Now, -1, null, Errors.Failed);
                        }
                        //-----------------------------------------------------
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", $"Error could not find databse drivers for  Local DB  ", DateTime.Now, -1, null, Errors.Failed);

                    }
                }
                catch (Exception ex)
                {

                    Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);

                }

            }
        }
        [RelayCommand]
        public void CreateInMemoryConnection()
        {
            if (DBWork != null)
            {
                try
                {
                    IsSaved = false;
                    IsNew = true;
                    IsCreated = false;
                    Add();
                    if (SelectedinMemoryDatabaseType != null)
                    {
                        ConnectionDriversConfig package = SelectedinMemoryDatabaseType;
                        if (package != null)
                        {
                            Connection.Category = DatasourceCategory.INMEMORY;//(DatasourceCategory)(int) Enum.Parse(typeof( DatasourceCategory),CategorycomboBox.Text);
                            Connection.DatabaseType = package.DatasourceType; //(DataSourceType)(int)Enum.Parse(typeof(DataSourceType), DatabaseTypecomboBox.Text);
                            Connection.ConnectionName = DatabaseName;
                            Connection.DriverName = package.PackageName;
                            Connection.DriverVersion = package.version;
                            Connection.ID = Editor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
                            Connection.Database = DatabaseName;
                            Connection.IsInMemory = true;
                            Connection.IsLocal = true;
                            Connection.DriverName = package.PackageName;
                            Connection.DriverVersion = package.version;
                            Connection.ConnectionString = package.ConnectionString; //Path.Combine(dataConnection.FilePath, dataConnection.FileName);
                            Save();
                            //-----------------------------------------------------


                            if (IsSaved)
                            {
                                IDataSource ds = Editor.GetDataSource(DatabaseName);
                                if (ds != null)
                                {
                                    IInMemoryDB dB = (IInMemoryDB)ds;
                                    IErrorsInfo ok = dB.OpenDatabaseInMemory(DatabaseName);
                                    if (ok.Flag == Errors.Failed)
                                    {
                                        //ds.ConnectionStatus = ds.Dataconnection.OpenConnection();
                                        Editor.OpenDataSource(Connection.ConnectionName);
                                    }
                                    IsCreated = true;
                                }
                                else
                                {
                                    Editor.AddLogMessage("Beep", $"Could not Create Local/Embedded Database", DateTime.Now, -1, null, Errors.Failed);
                                }
                            }

                        }
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", $"Error could not find databse drivers for  Local DB  ", DateTime.Now, -1, null, Errors.Failed);

                    }
                }
                catch (Exception ex)
                {

                    Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);

                }

            }
        }
      
        public  List<AssemblyClassDefinition> GetInMemoryDBs()
        {
            return Editor.ConfigEditor.DataSourcesClasses.Where(p => p.classProperties != null && p.InMemory == true).ToList();
        }
        public  void GetDriverConfiguration(string pclassname)
        {
            try
            {
               // AssemblyClassDefinition assembly = GetInMemoryDBs().Where(p => p.className == pclassname).FirstOrDefault();
                ConnectionDriversConfig package = Editor.ConfigEditor.DataDriversClasses.Where(x => x.classHandler == pclassname).OrderByDescending(o => o.version).FirstOrDefault();
                SelectedinMemoryDatabaseType = package;
                
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Could not Create Drivers Config {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
                SelectedinMemoryDatabaseType = null;
            }
        }
        public void GetInMemoryDriverConfigs()
        {
            try
            {
                List<string> ls = GetInMemoryDBs().Select(p => p.className).ToList();
                InMemoryDatabaseTypes = Editor.ConfigEditor.DataDriversClasses.Where(x => ls.Contains(x.classHandler) ).OrderByDescending(o => o.version).ToList();
           }
            catch (Exception ex)
            {
                InMemoryDatabaseTypes = null;
                Editor.AddLogMessage("Beep", $"Could not Create Drivers Config {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
                
            }
        }
       
    }
}
