using Beep.Vis.Module;
using TheTechIdea.Beep.Container.Services;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class TreeViewModel : BaseViewModel
    {
        public TreeViewModel(IBeepService beepService) : base( beepService)
        {
        }
    }
}
