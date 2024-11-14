using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;


namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public class DataSourcesViewModel : BaseViewModel
    {
        public DataSourcesViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
