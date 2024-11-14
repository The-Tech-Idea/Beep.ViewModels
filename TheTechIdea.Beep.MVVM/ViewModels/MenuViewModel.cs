using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public MenuViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
