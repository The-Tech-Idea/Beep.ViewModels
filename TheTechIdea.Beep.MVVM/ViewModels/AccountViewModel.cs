using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;



namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public AccountViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
