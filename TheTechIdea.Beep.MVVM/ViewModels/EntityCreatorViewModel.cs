using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Addin;

using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Utilities;
using System.Drawing;
using TheTechIdea.Beep.Editor.Migration;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    [Addin(Caption = "Beep BaseViewModel", Name = "BaseViewModel", addinType = AddinType.Class)]
    public partial class EntityManagerViewModel : BaseViewModel
    {
        [ObservableProperty]
        public string entityName;
        [ObservableProperty]
        public EntityStructure structure;
        [ObservableProperty]
        public string datasourcename;
        [ObservableProperty]
        public IEnumerable<DatatypeMapping> datatypeMappings;
        [ObservableProperty]
        public List<EntityField> oldFields;
        [ObservableProperty]
        public ObservableBindingList<EntityField> fields;
        [ObservableProperty]
        public UnitofWork<EntityField> dBWork;
        [ObservableProperty]
        public bool isChanged=false;
        [ObservableProperty]
        public bool isNew = false;
        [ObservableProperty]
        public IDataSource sourceConnection;
        DataTable tb;
        public EntityManagerViewModel(IDMEEditor pEditor, IAppManager visManager) : base(pEditor, visManager)
        {
            Editor = pEditor;
            VisManager = visManager;
            // dBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(fields), "GuidID");
            //dBWork.PreInsert += Unitofwork_PreInsert;

        }

        private IDataSource EnsureConnectionOpen()
        {
            if (SourceConnection == null && !string.IsNullOrEmpty(Datasourcename))
            {
                SourceConnection = Editor.GetDataSource(Datasourcename);
            }

            if (SourceConnection == null)
            {
                Editor.AddLogMessage("Beep", "Datasource not Found", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            if (SourceConnection.ConnectionStatus != ConnectionState.Open)
            {
                Editor.OpenDataSource(Datasourcename);
            }

            if (SourceConnection.ConnectionStatus != ConnectionState.Open)
            {
                Editor.AddLogMessage("Beep", "Datasource not Open", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }

            return SourceConnection;
        }

        private void RefreshFieldWork(EntityStructure entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.Fields ??= new List<EntityField>();
            OldFields = entity.Fields;
            DBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(entity.Fields), "GuidID");
            DBWork.PreInsert -= Unitofwork_PreInsert;
            DBWork.PreInsert += Unitofwork_PreInsert;
        }

        private void Unitofwork_PreInsert(object sender, UnitofWorkParams e)
        {
            EntityField field = (EntityField)e.Record;
            field.EntityName = Structure.EntityName;
            
        }
        [RelayCommand]
        public void UpdateEntityName()
        {
            if (Structure != null && !string.IsNullOrEmpty(EntityName))
            {
                Structure.EntityName = EntityName;
                IsChanged = true;
            }
        }
        [RelayCommand]
        public void UpdateDatasourceName()
        {
            if (Structure != null && !string.IsNullOrEmpty(Datasourcename))
            {
                Structure.DatasourceEntityName = Datasourcename;
                IsChanged = true;
            }
        }
        [RelayCommand]
        public void UpdateFieldName(EntityField field)
        {
            if (Structure != null && field != null && !string.IsNullOrEmpty(field.FieldName))
            {
                field.EntityName = Structure.EntityName;
                IsChanged = true;
            }
        }
        [RelayCommand]
        public void GetEntityStructure()
        {
            if (!string.IsNullOrEmpty(EntityName))
            {
                var connection = EnsureConnectionOpen();
                if (connection == null)
                {
                    return;
                }

                Structure = connection.GetEntityStructure(EntityName, true);
                if (Structure != null)
                {
                    IsChanged = false;
                    IsNew = false;
                    RefreshFieldWork(Structure);
                }
            }
        }

        [RelayCommand]
        public void LoadOrCreateEntityStructure(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                return;
            }

            EntityName = entityName;
            var connection = EnsureConnectionOpen();
            if (connection == null)
            {
                return;
            }

            if (connection.CheckEntityExist(entityName))
            {
                Structure = connection.GetEntityStructure(entityName, true);
                IsNew = false;
            }
            else
            {
                Structure = new EntityStructure
                {
                    EntityName = entityName,
                    DatasourceEntityName = entityName,
                    Fields = new List<EntityField>()
                };
                IsNew = true;
            }

            IsChanged = false;
            RefreshFieldWork(Structure);
        }
        [RelayCommand]
        public void UpdateFieldTypes()
        {
            if (!string.IsNullOrEmpty(Datasourcename) && string.IsNullOrEmpty(EntityName))
            {
                ConnectionProperties connection = Editor.ConfigEditor.DataConnections.Where(o => o.ConnectionName.Equals(Datasourcename, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                ConnectionDriversConfig conf = Editor.Utilfunction.LinkConnection2Drivers(connection);
                if (conf != null)
                {
                    DatatypeMappings= Editor.ConfigEditor.DataTypesMap.Where(p => p.DataSourceName.Equals(conf.classHandler, StringComparison.InvariantCultureIgnoreCase)).Distinct();
                }
            }
        }
        [RelayCommand]
        public void InitEntity(EntityStructure entity)
        {
            if (entity == null)
            {
                return;
            }

            Structure = entity;
            IsChanged = false;
            RefreshFieldWork(entity);
        }
        [RelayCommand]
        public void SaveEntity()
        {
            if (Structure != null && IsChanged)
            {
                if(Structure.Fields.Count==0)
                {
                    Editor.AddLogMessage("Beep", "No Fields Exist", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }
                ApplyChanges();
                DBWork.Commit(Logprogress,Token);
                OldFields = Structure.Fields;
              
            }
        }
        [RelayCommand]
        public void ApplyChanges()
        {
            if (Structure != null && IsChanged)
            {
                try
                {
                    var connection = EnsureConnectionOpen();
                    if (connection == null)
                    {
                        return;
                    }

                    Structure.DatasourceEntityName = Structure.EntityName;
                    Structure.Fields = DBWork?.Units?.ToList() ?? Structure.Fields;

                    var migration = new MigrationManager(Editor, connection);
                    IErrorsInfo result;

                    if (connection.CheckEntityExist(Structure.EntityName))
                    {
                        result = migration.EnsureEntity(Structure, createIfMissing: false, addMissingColumns: true);
                    }
                    else
                    {
                        var created = connection.CreateEntityAs(Structure);
                        result = created
                            ? new ErrorsInfo { Flag = Errors.Ok, Message = "Entity created" }
                            : connection.ErrorObject ?? new ErrorsInfo { Flag = Errors.Failed, Message = "Entity creation failed" };
                    }

                    if (result.Flag == Errors.Ok)
                    {
                        Editor.AddLogMessage("Success", result.Message ?? "Entity updated", DateTime.Now, -1, "", Errors.Ok);
                        IsChanged = false;
                        IsNew = false;
                    }
                    else
                    {
                        Editor.AddLogMessage("Fail", result.Message ?? "Entity update failed", DateTime.Now, -1, "", Errors.Failed);
                    }
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Exception", ex.Message, DateTime.Now, -1, ex.ToString(), Errors.Failed);
                }

            }
        }

        [RelayCommand]
        public void DeleteEntity()
        {
            if (string.IsNullOrEmpty(EntityName))
            {
                return;
            }

            var connection = EnsureConnectionOpen();
            if (connection == null)
            {
                return;
            }

            var migration = new MigrationManager(Editor, connection);
            var result = migration.DropEntity(EntityName);
            if (result.Flag == Errors.Ok)
            {
                Editor.AddLogMessage("Success", result.Message ?? "Entity deleted", DateTime.Now, -1, "", Errors.Ok);
                Structure = null;
                IsNew = true;
            }
            else
            {
                Editor.AddLogMessage("Fail", result.Message ?? "Entity delete failed", DateTime.Now, -1, "", Errors.Failed);
            }
        }
        [RelayCommand]
        public void New()
        { 
            if (Structure != null  && IsChanged )
            {
                Editor.AddLogMessage("Beep", "Changes not Saved", DateTime.Now, 0, null, Errors.Failed);
                return;
            }
            Structure = new EntityStructure
            {
                EntityName = EntityName,
                DatasourceEntityName = EntityName,
                Fields = new List<EntityField>()
            };

            IsChanged = true;
            IsNew = true;
            RefreshFieldWork(Structure);
        }
        
    }
}
