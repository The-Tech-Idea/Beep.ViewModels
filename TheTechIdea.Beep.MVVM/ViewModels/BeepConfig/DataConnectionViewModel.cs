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
    public partial class DataConnectionViewModel : BaseViewModel
    {
        #region Observable Properties
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
        [ObservableProperty]
        string extension;
        [ObservableProperty]
        List<AssemblyClassDefinition> installedDataSources;
        #endregion

        #region Public Properties
        public ObservableBindingList<ConnectionProperties> DataConnections => DBWork.Units;
        #endregion

        #region Constructor
        public DataConnectionViewModel(IDMEEditor dMEEditor, IAppManager visManager) : base(dMEEditor, visManager)
        {
            InitializeViewModel();
        }
        #endregion

        #region Initialization
        private void InitializeViewModel()
        {
            try
            {
                // Initialize DBWork
                dBWork = new UnitofWork<ConnectionProperties>(Editor, true, 
                    new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
                DBWork.Get();

                // Initialize collections
                InitializeCollections();
                GetInstallDataSources();
                PopulatePackageInformation();
                PopulateEmbeddedDatabaseTypes();
                GetInMemoryDriverConfigs();
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error initializing DataConnectionViewModel - {ex.Message}", 
                    DateTime.Now, -1, null, Errors.Failed);
            }
        }

        private void InitializeCollections()
        {
            Filters = new List<AppFilter>();
            DatasourcesCategorys = Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
            embeddedDatabaseTypes = new List<ConnectionDriversConfig>();
            installedDataSources = new List<AssemblyClassDefinition>();
        }

        private void PopulatePackageInformation()
        {
            var installedDataSourceClasses = installedDataSources?.Select(x => x.className).ToHashSet() ?? new HashSet<string>();

            foreach (var item in Editor.ConfigEditor.DataDriversClasses)
            {
                if (!string.IsNullOrEmpty(item.PackageName) && installedDataSourceClasses.Contains(item.classHandler))
                {
                    if (!packageNames.Contains(item.PackageName))
                        packageNames.Add(item.PackageName);
                    
                    if (!packageVersions.Contains(item.version))
                        packageVersions.Add(item.version);
                }
            }
        }

        private void PopulateEmbeddedDatabaseTypes()
        {
            var installedDataSourceClasses = installedDataSources?.Select(x => x.className).ToHashSet() ?? new HashSet<string>();

            foreach (var cls in Editor.ConfigEditor.DataDriversClasses.Where(x => x.CreateLocal == true))
            {
                if (installedDataSourceClasses.Contains(cls.classHandler))
                {
                    embeddedDatabaseTypes.Add(cls);
                }
            }
        }
        #endregion

        #region Data Access Commands
        [RelayCommand]
        public void Save()
        {
            if (DBWork == null) return;

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
                Editor.AddLogMessage("Beep", $"Error Saving Connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }

        [RelayCommand]
        public void UpdateConnection()
        {
            if (DBWork != null && Connection != null)
            {
                DBWork.Update(Connection);
                Editor.ConfigEditor.SaveDataconnectionsValues();
                IsNew = false;
            }
        }

        [RelayCommand]
        public void Add()
        {
            Connection = new ConnectionProperties();
            DBWork?.Add(Connection);
        }

        [RelayCommand]
        public void Delete()
        {
            if (DBWork != null && Connection != null)
            {
                DBWork.Delete(Connection);
            }
        }

        [RelayCommand]
        public void Get()
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, 
                    new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            }
            DBWork.Get();
        }

        [RelayCommand]
        public void GetByGuid()
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, 
                    new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            }

            if (!string.IsNullOrEmpty(SelectedconnectionGuid))
            {
                DBWork.Get(new List<AppFilter>() 
                { 
                    new AppFilter() 
                    { 
                        FieldName = "GuidID", 
                        FilterValue = SelectedconnectionGuid, 
                        Operator = "=" 
                    } 
                });

                if (DBWork.Units.Count > 0)
                {
                    Connection = DBWork.Units[0];
                }
            }
        }

        [RelayCommand]
        public void Filter()
        {
            if (DBWork == null) return;

            if (string.IsNullOrEmpty(SelectedCategoryTextValue))
            {
                DBWork.Get();
            }
            else
            {
                Filters = new List<AppFilter>() 
                { 
                    new AppFilter() 
                    { 
                        FieldName = "Category", 
                        FilterValue = selectedCategoryTextValue.ToUpper(), 
                        Operator = "=" 
                    } 
                };
                DBWork.Get(Filters);
            }
        }

        [RelayCommand]
        public void GetConnection()
        {
            if (DBWork != null && Connection != null)
            {
                Selectedconnectionidx = DBWork.Getindex(Connection.GuidID);
                if (Selectedconnectionidx > -1)
                {
                    Connection = DBWork.Units[Selectedconnectionidx];
                }
            }
        }

        [RelayCommand]
        public void GetFields()
        {
            Fields = DBWork.EntityStructure?.Fields ?? null;
        }
        #endregion

        #region Database Creation Commands
        [RelayCommand]
        public void CreateLocalConnection()
        {
            try
            {
                var validationResult = ValidateLocalConnectionInput();
                if (!validationResult.IsValid)
                {
                    Editor.AddLogMessage("Beep", validationResult.ErrorMessage, DateTime.Now, -1, null, Errors.Failed);
                    return;
                }

                var connectionInfo = PrepareLocalConnectionInfo();
                if (connectionInfo == null) return;

                var result = CreateAndSaveConnection(connectionInfo);
                if (result.Success)
                {
                    CreatePhysicalDatabase(connectionInfo);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }

        [RelayCommand]
        public void CreateLocalConnectionUsingPath()
        {
            try
            {
                var pathValidation = ValidateInstallPath();
                if (!pathValidation.IsValid)
                {
                    ErrorObject.Flag = Errors.Failed;
                    ErrorObject.Message = pathValidation.ErrorMessage;
                    return;
                }

                var validationResult = ValidateLocalConnectionInput();
                if (!validationResult.IsValid)
                {
                    Editor.AddLogMessage("Beep", validationResult.ErrorMessage, DateTime.Now, -1, null, Errors.Failed);
                    return;
                }

                var connectionInfo = PrepareLocalConnectionInfoWithPath();
                if (connectionInfo == null) return;

                var result = CreateAndSaveConnection(connectionInfo);
                if (result.Success)
                {
                    CreatePhysicalDatabase(connectionInfo);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }

        [RelayCommand]
        public void CreateInMemoryConnection()
        {
            try
            {
                var validationResult = ValidateInMemoryConnectionInput();
                if (!validationResult.IsValid)
                {
                    Editor.AddLogMessage("Beep", validationResult.ErrorMessage, DateTime.Now, -1, null, Errors.Failed);
                    return;
                }

                var connectionInfo = PrepareInMemoryConnectionInfo();
                if (connectionInfo == null) return;

                var result = CreateAndSaveConnection(connectionInfo);
                if (result.Success)
                {
                    CreateInMemoryDatabase(connectionInfo);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error creating In-Memory DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }
        #endregion

        #region Validation Methods
        private ValidationResult ValidateLocalConnectionInput()
        {
            if (SelectedEmbeddedDatabaseType == null)
                return new ValidationResult(false, "Please select an embedded database type");

            if (string.IsNullOrWhiteSpace(DatabaseName))
                return new ValidationResult(false, "Database name is required");

            if (DatabaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return new ValidationResult(false, "Database name contains invalid characters");

            return new ValidationResult(true);
        }

        private ValidationResult ValidateInMemoryConnectionInput()
        {
            if (SelectedinMemoryDatabaseType == null)
                return new ValidationResult(false, "Please select an in-memory database type");

            if (string.IsNullOrWhiteSpace(DatabaseName))
                return new ValidationResult(false, "Database name is required");

            return new ValidationResult(true);
        }

        private ValidationResult ValidateInstallPath()
        {
            if (string.IsNullOrWhiteSpace(InstallFolderPath))
                return new ValidationResult(false, "Install folder path is required");

            var normalizedPath = NormalizePath(InstallFolderPath);
            if (!Directory.Exists(normalizedPath))
                return new ValidationResult(false, $"Folder {normalizedPath} does not exist");

            return new ValidationResult(true);
        }

        private string NormalizePath(string path)
        {
            if (path.StartsWith(".") || path.Equals("/") || path.Equals("\\"))
            {
                return Path.Combine(Editor.ConfigEditor.ExePath, path.TrimStart('.', '/', '\\'));
            }
            return path;
        }
        #endregion

        #region Connection Preparation Methods
        private ConnectionInfo PrepareLocalConnectionInfo()
        {
            return new ConnectionInfo
            {
                DatabaseType = SelectedEmbeddedDatabaseType,
                Name = DatabaseName,
                Password = Password,
                Extension = string.IsNullOrEmpty(Extension) ? "db" : Extension,
                FolderPath = string.IsNullOrEmpty(InstallFolderPath) ? Editor.ConfigEditor.ExePath : InstallFolderPath,
                IsLocal = true,
                IsInMemory = false
            };
        }

        private ConnectionInfo PrepareLocalConnectionInfoWithPath()
        {
            return new ConnectionInfo
            {
                DatabaseType = SelectedEmbeddedDatabaseType,
                Name = DatabaseName,
                Password = Password,
                Extension = string.IsNullOrEmpty(Extension) ? "db" : Extension,
                FolderPath = NormalizePath(InstallFolderPath),
                IsLocal = true,
                IsInMemory = false
            };
        }

        private ConnectionInfo PrepareInMemoryConnectionInfo()
        {
            return new ConnectionInfo
            {
                DatabaseType = SelectedinMemoryDatabaseType,
                Name = DatabaseName,
                Password = Password,
                FolderPath = null,
                IsLocal = true,
                IsInMemory = true
            };
        }
        #endregion

        #region Database Creation Helper Methods
        private OperationResult CreateAndSaveConnection(ConnectionInfo connectionInfo)
        {
            try
            {
                IsSaved = false;
                IsNew = true;
                IsCreated = false;

                Add();

                PopulateConnectionProperties(connectionInfo);
                Save();

                return new OperationResult(IsSaved, IsSaved ? "Connection saved successfully" : "Failed to save connection");
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error saving connection - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
                return new OperationResult(false, ex.Message);
            }
        }

        private void PopulateConnectionProperties(ConnectionInfo connectionInfo)
        {
            Connection.Category = connectionInfo.DatabaseType.DatasourceCategory;
            Connection.DatabaseType = connectionInfo.DatabaseType.DatasourceType;
            Connection.ConnectionName = connectionInfo.Name;
            Connection.DriverName = connectionInfo.DatabaseType.PackageName;
            Connection.DriverVersion = connectionInfo.DatabaseType.version;
            Connection.ID = GetNextConnectionId();
            Connection.UserID = UserId ?? "";
            Connection.Password = connectionInfo.Password;

            if (connectionInfo.IsInMemory)
            {
                Connection.Database = connectionInfo.Name;
                Connection.IsInMemory = true;
                Connection.IsLocal = true;
                Connection.ConnectionString = connectionInfo.DatabaseType.ConnectionString;
            }
            else
            {
                Connection.FilePath = connectionInfo.FolderPath;
                Connection.FileName = $"{connectionInfo.Name}.{connectionInfo.Extension}";
                Connection.IsLocal = true;
                Connection.ConnectionString = connectionInfo.DatabaseType.ConnectionString;
                
                // Normalize file path for display
                if (Connection.FilePath.Contains(Editor.ConfigEditor.ExePath))
                {
                    Connection.FilePath = Connection.FilePath.Replace(Editor.ConfigEditor.ExePath, ".");
                }
            }
        }

        private int GetNextConnectionId()
        {
            return Editor.ConfigEditor.DataConnections.Count == 0 ? 1 : 
                   Editor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
        }

        private void CreatePhysicalDatabase(ConnectionInfo connectionInfo)
        {
            try
            {
                if (!IsSaved) return;

                var ds = Editor.CreateLocalDataSourceConnection(Connection, connectionInfo.Name, 
                    connectionInfo.DatabaseType.classHandler);
                
                if (ds is ILocalDB localDB)
                {
                    var success = localDB.CreateDB();
                    if (success)
                    {
                        Editor.OpenDataSource(Connection.ConnectionName);
                        IsCreated = true;
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", "Could not create physical database", 
                            DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                else
                {
                    Editor.AddLogMessage("Beep", "Could not create local database data source", 
                        DateTime.Now, -1, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error creating physical database - {ex.Message}", 
                    DateTime.Now, -1, null, Errors.Failed);
            }
        }

        private void CreateInMemoryDatabase(ConnectionInfo connectionInfo)
        {
            try
            {
                if (!IsSaved) return;

                var ds = Editor.GetDataSource(connectionInfo.Name);
                if (ds is IInMemoryDB inMemoryDB)
                {
                    var result = inMemoryDB.OpenDatabaseInMemory(connectionInfo.Name);
                    if (result.Flag == Errors.Ok)
                    {
                        Editor.OpenDataSource(Connection.ConnectionName);
                        IsCreated = true;
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", $"Could not create in-memory database: {result.Message}", 
                            DateTime.Now, -1, null, Errors.Failed);
                    }
                }
                else
                {
                    Editor.AddLogMessage("Beep", "Could not create in-memory database data source", 
                        DateTime.Now, -1, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error creating in-memory database - {ex.Message}", 
                    DateTime.Now, -1, null, Errors.Failed);
            }
        }
        #endregion

        #region Helper Methods and Data Access
        private void GetInstallDataSources()
        {
            installedDataSources = new List<AssemblyClassDefinition>();
            if (Editor.ConfigEditor.DataSourcesClasses != null)
            {
                foreach (var item in Editor.ConfigEditor.DataSourcesClasses)
                {
                    if (!string.IsNullOrEmpty(item.className))
                    {
                        installedDataSources.Add(item);
                    }
                }
            }
        }

        public List<AssemblyClassDefinition> GetInMemoryDBs()
        {
            return Editor.ConfigEditor.DataSourcesClasses
                .Where(p => p.classProperties != null && p.InMemory == true)
                .ToList();
        }

        public void GetDriverConfiguration(string pclassname)
        {
            try
            {
                SelectedinMemoryDatabaseType = Editor.ConfigEditor.DataDriversClasses
                    .Where(x => x.classHandler == pclassname)
                    .OrderByDescending(o => o.version)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Could not create drivers config {ex.Message}", 
                    DateTime.Now, -1, "", Errors.Failed);
                SelectedinMemoryDatabaseType = null;
            }
        }

        public void GetInMemoryDriverConfigs()
        {
            try
            {
                var inMemoryClassNames = GetInMemoryDBs().Select(p => p.className).ToList();
                InMemoryDatabaseTypes = Editor.ConfigEditor.DataDriversClasses
                    .Where(x => inMemoryClassNames.Contains(x.classHandler))
                    .OrderByDescending(o => o.version)
                    .ToList();
            }
            catch (Exception ex)
            {
                InMemoryDatabaseTypes = null;
                Editor.AddLogMessage("Beep", $"Could not create drivers config {ex.Message}", 
                    DateTime.Now, -1, "", Errors.Failed);
            }
        }

        public void GetConnection(string connid)
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, 
                    new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            }

            if (!string.IsNullOrEmpty(connid))
            {
                DBWork.Get(new List<AppFilter>() 
                { 
                    new AppFilter() 
                    { 
                        FieldName = "GuidID", 
                        FilterValue = connid, 
                        Operator = "=" 
                    } 
                });

                if (DBWork.Units.Count > 0)
                {
                    Connection = DBWork.Units[0];
                }
            }
        }

        public void GetConnectionByName(string dsname, DataSourceType sourceType, DatasourceCategory category)
        {
            if (DBWork == null)
            {
                DBWork = new UnitofWork<ConnectionProperties>(Editor, true, 
                    new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            }

            if (!string.IsNullOrEmpty(dsname))
            {
                DBWork.Get(new List<AppFilter>() 
                {
                    new AppFilter() 
                    { 
                        FieldName = "ConnectionName", 
                        FilterValue = dsname, 
                        Operator = "=" 
                    },
                    new AppFilter() 
                    { 
                        FieldName = "DatabaseType", 
                        FilterValue = ((int)sourceType).ToString(), 
                        Operator = "=" 
                    },
                    new AppFilter() 
                    { 
                        FieldName = "Category", 
                        FilterValue = category.ToString().ToUpper(), 
                        Operator = "=" 
                    }
                });

                if (DBWork.Units.Count > 0)
                {
                    Connection = DBWork.Units[0];
                }
            }
        }
        #endregion

        #region Helper Classes
        private class ValidationResult
        {
            public bool IsValid { get; }
            public string ErrorMessage { get; }

            public ValidationResult(bool isValid, string errorMessage = null)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }

        private class OperationResult
        {
            public bool Success { get; }
            public string Message { get; }

            public OperationResult(bool success, string message = null)
            {
                Success = success;
                Message = message;
            }
        }

        private class ConnectionInfo
        {
            public ConnectionDriversConfig DatabaseType { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public string Extension { get; set; }
            public string FolderPath { get; set; }
            public bool IsLocal { get; set; }
            public bool IsInMemory { get; set; }
        }
        #endregion
    }
}
