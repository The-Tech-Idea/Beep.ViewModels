﻿using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using DataManagementModels.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.MVVM;

using TheTechIdea.Beep.Report;
using DataManagementModels.ConfigUtil;
using TheTechIdea.Util;

using TheTechIdea.Beep.Editor;
using System.ComponentModel;
using System.Diagnostics;

using System.Data;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class CreateCrudViewViewModel: BaseViewModel 
    {
        [ObservableProperty]
        string entityname;
        [ObservableProperty]
        Type entityType;
        [ObservableProperty]
        string datasourceName;
        [ObservableProperty]
        EntityStructure structure;
        [ObservableProperty]
        ObservableBindingList<AppFilter> filters;
        [ObservableProperty]
        IDataSource dataSource;
        [ObservableProperty]
        IBindingListView ts;
        [ObservableProperty]
        string primaryKey;
        [ObservableProperty]
        bool isPrimarykeyMissing;

     

        // Using IList to hold a list of dynamically typed entities
        UnitOfWorkWrapper unitOfWork;
        public CreateCrudViewViewModel(IDMEEditor Editor, IVisManager visManager) : base(Editor, visManager)
        {
        }
        public bool GetData()
        {
            try
            {
                EntityType = DataSource.GetEntityType(Entityname);
                var uow = UnitOfWorkFactory.CreateUnitOfWork(EntityType, Editor, DatasourceName, Entityname, Structure, PrimaryKey);
                unitOfWork = new UnitOfWorkWrapper(uow);
                var result= unitOfWork.Get().Result;
                // Ensure Ts is ObservableBindingList<object>
                //if (result is DataTable dataTable)
                //{
                //    Ts = (IBindingListView)Editor.Utilfunction.GetBindingListByDataTable(result, EntityType,Structure);
                //}
                //else if (result is List<object> list)
                //{
                //    Ts = Editor.Utilfunction.GetBindingListFromIList(result, EntityType, Structure);
                //}
                //else 
                //{
                    Ts = result; // Directly use if already ObservableBindingList<object>
             //   }
               
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", ex.Message, DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
           return true;
        }
        public bool Init()
        {
            if (string.IsNullOrEmpty(DatasourceName))
            {
               Editor.AddLogMessage("Fail", $"Datasource name is empty", DateTime.Now, 0, DatasourceName, Errors.Failed);
                return false;
            }
            if (string.IsNullOrEmpty(Entityname))
            {
               Editor.AddLogMessage("Fail", $"Entity name is empty", DateTime.Now, 0, DatasourceName, Errors.Failed);
                return false;
            }
            DataSource =Editor.GetDataSource(DatasourceName);
            if (DataSource == null)
            {
               Editor.AddLogMessage("Fail", $"Could not find datasource {DatasourceName}", DateTime.Now, 0, DatasourceName, Errors.Failed);
                return false;
            }
            if (DataSource.Openconnection() == ConnectionState.Closed)
            {
               Editor.AddLogMessage("Fail", $"Could not open connection to datasource {DatasourceName}", DateTime.Now, 0, DatasourceName, Errors.Failed);
                return false;
            }
            Structure = DataSource.GetEntityStructure(Entityname, true);
            if (Structure == null)
            {
               Editor.AddLogMessage("Fail", $"Could not find entity {Entityname}", DateTime.Now, 0, DatasourceName, Errors.Failed);
                return false;
            }
         
            if (Structure.Fields.Count > 0)
            {
                if (Structure.PrimaryKeys.Count == 0)
                {
                   Editor.AddLogMessage("Fail", $"Could not find primary key for entity {Entityname}, setting first column", DateTime.Now, 0, DatasourceName, Errors.Failed);
                    PrimaryKey = Structure.Fields.FirstOrDefault().fieldname;
                    IsPrimarykeyMissing = true;


                }
                else
                {
                    if (structure.PrimaryKeys.FirstOrDefault().fieldname == null)
                    {
                       Editor.AddLogMessage("Fail", $"Could not find primary key for entity {Entityname}, setting first column", DateTime.Now, 0, DatasourceName, Errors.Failed);
                        PrimaryKey = Structure.Fields.FirstOrDefault().fieldname;
                        IsPrimarykeyMissing = true;
                        //   return;
                    }
                    else
                    {
                        PrimaryKey = Structure.PrimaryKeys.FirstOrDefault().fieldname;
                        IsPrimarykeyMissing = false;
                    }

                }
            }
            EntityType = DataSource.GetEntityType(Entityname);
            if(IsPrimarykeyMissing)
            {
                SetPrimarKey();
            }
            return true;
        }
        private void SetPrimarKey()
        {
            int idx = DataSource.Entities.FindIndex(x => x.EntityName == Entityname);
            if (idx > -1)
            {
                DataSource.Entities[idx].PrimaryKeys.Add(new EntityField { fieldname = PrimaryKey });
            }
        }
        public bool Save()
        {
            try
            {
                IErrorsInfo retval= unitOfWork.Commit().Result;
                if(retval.Flag != Errors.Ok)
                {
                    Editor.AddLogMessage("Beep", $"Error Could not Save Data ...{retval.Message}", DateTime.Now, 0, null, Errors.Failed);
                    return false;
                }
                Editor.AddLogMessage("Beep", $"Saved Data Successfully .", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Could not Save Data ...{ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
            return true;
        }
        
     
    }
}
