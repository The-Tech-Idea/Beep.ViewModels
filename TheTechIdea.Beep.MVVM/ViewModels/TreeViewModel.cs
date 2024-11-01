using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Container.Services;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class TreeViewModel : BaseViewModel
    {
        public TreeViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
