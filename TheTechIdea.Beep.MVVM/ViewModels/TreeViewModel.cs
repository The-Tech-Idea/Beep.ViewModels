using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class TreeViewModel : BaseViewModel
    {
        public TreeViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
