using CommunityToolkit.Mvvm.ComponentModel;

using TheTechIdea.Beep.Container.Services;
using BeepEnterprize.Vis.Module;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        string logoname="SimpleODM.ico";
        [ObservableProperty]
        string title = "Beep - The Data Plaform";
        public MainViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {

            VisManager.Title = title;
            VisManager.IconUrl = logoname;

        }
     
    }
}
