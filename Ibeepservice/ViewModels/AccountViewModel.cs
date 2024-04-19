using Beep.Vis.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public AccountViewModel(IBeepService beepService) : base( beepService)
        {
        }
    }
}
