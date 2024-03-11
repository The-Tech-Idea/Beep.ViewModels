using BeepEnterprize.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataManagementModels.Editor;
using TheTechIdea.Beep.Editor;

using TheTechIdea.Util;


namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DataConnectionViewModel : BaseViewModel
    {
        [ObservableProperty]
        UnitofWork<ConnectionProperties> dBWork;
    
        public DataConnectionViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
          //  DBWork = new UnitofWork<ConnectionDriversConfig>(DMEEditor, true, new ObservableBindingList<ConnectionDriversConfig>(DMEEditor.ConfigEditor.DataDriversClasses), "GuidID");
            dBWork = new UnitofWork<ConnectionProperties>(Editor,true, new ObservableBindingList<ConnectionProperties>(Editor.ConfigEditor.DataConnections), "GuidID");
        }
        [RelayCommand]
        void Save()
        {
            if (DBWork != null)
            {
                DBWork.Commit(Logprogress,Token);
            }
        }


    }
}
