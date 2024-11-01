using CommunityToolkit.Mvvm.ComponentModel;

using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Vis.Modules;
using DataManagementModels.Editor;
using System.Collections.Generic;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
    
        

        public MainViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {

        

        }
     
    }
}
