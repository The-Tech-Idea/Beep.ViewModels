using BeepEnterprize.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataManagementModels.DriversConfigurations;
using DataManagementModels.Editor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;

using TheTechIdea.Util;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class EntityCreatorViewModel : BaseViewModel
    {
        [ObservableProperty]
        string entityName;
        [ObservableProperty]
        EntityStructure structure;
        [ObservableProperty]
        string datasourcename;
        [ObservableProperty]
        IEnumerable<DatatypeMapping> datatypeMappings;
        [ObservableProperty]
        List<EntityField> oldfields;
        [ObservableProperty]
        List<EntityField> newfields;
        UnitofWork<EntityField> dBWork;
        [ObservableProperty]
        bool isChanged=false;
        [ObservableProperty]
        bool isNew = false;
        IDataSource SourceConnection;
        DataTable tb;
        public EntityCreatorViewModel(IDMEEditor Editor, IVisManager visManager) : base(Editor, visManager)
        {
            // unitofwork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(fields), "GuidID");
            //dBWork.PreInsert += Unitofwork_PreInsert;

        }

        private void Unitofwork_PreInsert(object sender, UnitofWorkParams e)
        {
            EntityField field = (EntityField)e.Record;
            field.EntityName = Structure.EntityName;
            
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
                Oldfields = Structure.Fields;
                Newfields = Structure.Fields;
                dBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(Structure.Fields), "GuidID");
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
                dBWork.Commit(Logprogress,Token);
                Oldfields = Structure.Fields;
                Newfields = Structure.Fields;
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
                        IDataSource SourceConnection = Editor.GetDataSource(datasourcename);
                        Editor.OpenDataSource(datasourcename);
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

                dBWork = new UnitofWork<EntityField>(Editor, true, new ObservableBindingList<EntityField>(Structure.Fields), "GuidID");
                dBWork.PreInsert -= Unitofwork_PreInsert;
                dBWork.PreInsert += Unitofwork_PreInsert;
            }
        }
        
    }
}
