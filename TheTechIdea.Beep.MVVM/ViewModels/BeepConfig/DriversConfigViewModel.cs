using TheTechIdea.Beep.Vis.Modules;

using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Utilities;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System;
using TheTechIdea.Beep.Addin;
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

        public DriversConfigViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
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
            listofImages = visManager.visHelper.GetImageNames();
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
