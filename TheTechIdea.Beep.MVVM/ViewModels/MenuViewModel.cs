using TheTechIdea.Beep.Vis.Modules;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public MenuViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
