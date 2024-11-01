using TheTechIdea.Beep.Vis.Modules;

using TheTechIdea.Beep.Container.Services;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
