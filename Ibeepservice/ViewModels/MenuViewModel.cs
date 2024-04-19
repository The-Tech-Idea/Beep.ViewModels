using Beep.Vis.Module;

using TheTechIdea.Beep.Container.Services;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public MenuViewModel(IBeepService beepService) : base( beepService)
        {
        }
    }
}
