using CommunityToolkit.Mvvm.ComponentModel;


using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
    
        

        public MainViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {

        

        }
     
    }
}
