using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.DriversConfigurations;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DriversConfigViewModel : BaseViewModel
    {
        [ObservableProperty]
        UnitofWork<ConnectionDriversConfig> dBWork;
        [ObservableProperty]
        List<AssemblyClassDefinition> dBAssemblyClasses;
        [ObservableProperty]
        List<object> dataSourceCategories;
        [ObservableProperty]
        List<object> dataSourceTypes;
        [ObservableProperty]
        List<string> listofImages;
   
        public ObservableBindingList<ConnectionDriversConfig> ConnectionDriversConfigs { get => DBWork.Units; }
        public DriversConfigViewModel(IDMEEditor dMEEditor,IAppManager visManager) : base( dMEEditor, visManager)
        {
            var x=Editor.ConfigEditor.LoadConnectionDriversConfigValues();
            if(x != null && x.Count>0)
            {
                Editor.ConfigEditor.DataDriversClasses = x;
            }
            DBWork = new UnitofWork<ConnectionDriversConfig>(dMEEditor, true, new ObservableBindingList<ConnectionDriversConfig>(Editor.ConfigEditor.DataDriversClasses), "GuidID");
            DBAssemblyClasses = Editor.ConfigEditor.DataSourcesClasses;
            DataSourceCategories = new List<object>();
            dataSourceTypes = new List<object>();
            foreach (var item in Enum.GetValues(typeof(DatasourceCategory)))
            {
                DataSourceCategories.Add(item);
            }
            foreach (var item in Enum.GetValues(typeof(DataSourceType)))
            {
                DataSourceTypes.Add(item);
            }
       
        }

        [RelayCommand]
        public void Save()
        {
            Editor.ConfigEditor.SaveConnectionDriversConfigValues();
            if (DBWork != null)
            {
               
                DBWork.Commit(Logprogress, Token);
            }
        }
    }
}
