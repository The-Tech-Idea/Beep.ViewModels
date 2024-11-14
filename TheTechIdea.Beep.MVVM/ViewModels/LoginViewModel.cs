using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
