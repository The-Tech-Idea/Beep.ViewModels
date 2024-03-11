using BeepEnterprize.Vis.Module;

using TheTechIdea.Beep.MVVM.Modules;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public MenuViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
