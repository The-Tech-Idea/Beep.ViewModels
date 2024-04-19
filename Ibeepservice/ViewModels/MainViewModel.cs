using CommunityToolkit.Mvvm.ComponentModel;

using TheTechIdea.Beep.Container.Services;
using Beep.Vis.Module;
using DataManagementModels.Editor;
using System.Collections.Generic;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
    
        

        public MainViewModel(IBeepService beepService) : base( beepService)
        {

        

        }
     
    }
}
