using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataManagementModels.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Util;


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
        public ObservableBindingList<ConnectionProperties> DataConnections =>DBWork.Units;
    
        public DataConnectionViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
          //  DBWork = new UnitofWork<ConnectionDriversConfig>(DMEEditor, true, new ObservableBindingList<ConnectionDriversConfig>(Editor.ConfigEditor.DataDriversClasses), "GuidID");
            dBWork = new UnitofWork<ConnectionProperties>(Editor,true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
            ConnectionProperties connection=new ConnectionProperties();
         
            Filters =new List<AppFilter>();
            DatasourcesCategorys= Enum.GetValues(typeof(DatasourceCategory));
            packageNames = new List<string>();
            packageVersions = new List<string>();
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
           
        }
        [RelayCommand]
        public void Save()
        {
            if (DBWork != null)
            {

                Editor.ConfigEditor.DataConnections = DBWork.Units.ToList();
                Editor.ConfigEditor.SaveDataconnectionsValues();
            }
        }
        [RelayCommand]
        public void Add()
        {
            Connection = new ConnectionProperties();
            if (DBWork != null)
            {
                if(SelectedCategoryItem != null)
                {
                    Connection.Category = SelectedCategoryItem;
                }
                if(SelectedCategoryItem!=null)
                {
                    Connection.Category = SelectedCategoryItem;
                }
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
            if(SelectedconnectionGuid != null)
            {
                if (!string.IsNullOrEmpty(SelectedconnectionGuid))
                {
                    DBWork.Get(new List<AppFilter>() { new AppFilter() { FieldName = "GuidID", FilterValue = $"{SelectedconnectionGuid}", Operator = "=" } });
                }
            }
            if(DBWork.Units.Count > 0)
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
                    Filters = new List<AppFilter>() { new AppFilter() { FieldName = "Category", FilterValue = selectedCategoryTextValue.ToUpper().ToString(), Operator = "=" }};
                    DBWork.Get(Filters);
                   

                }
              
            }
        }
        [RelayCommand]
        public void GetConnection()
        {
            if (DBWork != null)
            {
                if(Connection!=null)
                {
                    Selectedconnectionidx= DBWork.Getindex(Connection.GuidID);
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
                Fields= DBWork.EntityStructure.Fields;
            }
            else
            {
                Fields= null;
            }
        }
    }
}
