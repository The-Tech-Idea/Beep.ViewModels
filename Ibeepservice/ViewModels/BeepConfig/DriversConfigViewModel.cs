using Beep.Vis.Module;

using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Util;
using DataManagementModels.DriversConfigurations;
using DataManagementModels.Editor;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System;

using TheTechIdea.Beep.Container.Services;

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

        public DriversConfigViewModel(IBeepService beepService) : base( beepService)
        {
            DBWork = new UnitofWork<ConnectionDriversConfig>(Editor, true, new ObservableBindingList<ConnectionDriversConfig>(Editor.ConfigEditor.DataDriversClasses), "GuidID");
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
            listofImages = VisManager.visHelper.GetImageNames();
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
