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
            if (Structure != null && field != null && !string.IsNullOrEmpty(field.fieldname))
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
                if(SourceConnection==null)
                {
                    SourceConnection = Editor.GetDataSource(Datasourcename);
                }
                if(SourceConnection == null)
                {
                    Editor.AddLogMessage("Beep", "Datasource not Found", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }
                if(SourceConnection.ConnectionStatus != ConnectionState.Open)
                {
                    Editor.AddLogMessage("Beep", "Datasource not Open", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }
                Structure = SourceConnection.GetEntityStructure(EntityName,true);
                if (Structure != null)
                {
                    IsChanged = false;
                    IsNew = false;
               //     Fields= Structure.Fields;
                    OldFields = Structure.Fields;
                    DBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(Structure.Fields), "GuidID");
                    DBWork.PreInsert -= Unitofwork_PreInsert;
                    DBWork.PreInsert += Unitofwork_PreInsert;
                }
            }
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
                Structure = entity;
                IsChanged = false;
                Structure = entity;
                OldFields = Structure.Fields;
                DBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(Structure.Fields), "GuidID");
            }
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
                IsChanged = false;
                IsNew = false;

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
                if(!IsNew)
                {

                }
                else
                {
                    try
                    {
                       // Apply for New Entity
                        IDataSource SourceConnection = Editor.GetDataSource(Datasourcename);
                        Editor.OpenDataSource(Datasourcename);
                        //SourceConnection.Dataconnection.OpenConnection();
                        SourceConnection.ConnectionStatus = SourceConnection.Dataconnection.ConnectionStatus;
                        if (SourceConnection.ConnectionStatus == ConnectionState.Open)
                        {
                            Structure.DatasourceEntityName = Structure.EntityName;

                            SourceConnection.CreateEntityAs(Structure);
                            if (Editor.ErrorObject.Flag == Errors.Ok)
                            {
                               
                                Editor.AddLogMessage("Success", "Table Creation Success", DateTime.Now, -1, "", Errors.Failed);
                            }
                            else
                            {
                                string mes = "Entity Creation Failed";
                               
                                Editor.AddLogMessage("Create Table", mes, DateTime.Now, -1, mes, Errors.Failed);
                            }

                        }
                        else
                        {
                           
                            Editor.AddLogMessage("Fail", "Table Creation Not Success Could not open Database", DateTime.Now, -1, "", Errors.Failed);
                        }

                    }
                    catch (Exception ex)
                    {
                        string mes = "Entity Creation Failed";
                      
                        Editor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
                    };

                }

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
            Structure = new EntityStructure();
            if (Structure != null && IsChanged && !string.IsNullOrEmpty(Datasourcename))
            {
                IsChanged = false;
                IsNew = true;

                DBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(Structure.Fields), "GuidID");
                DBWork.PreInsert -= Unitofwork_PreInsert;
                DBWork.PreInsert += Unitofwork_PreInsert;
            }
        }
        
    }
}
