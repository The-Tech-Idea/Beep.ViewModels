using Beep.Vis.Module;

using TheTechIdea.Beep.Container.Services;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(IBeepService beepService) : base( beepService)
        {
        }
    }
}
