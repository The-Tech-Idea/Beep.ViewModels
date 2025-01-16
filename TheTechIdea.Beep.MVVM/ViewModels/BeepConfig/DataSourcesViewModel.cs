using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public class DataSourcesViewModel : BaseViewModel
    {
        public DataSourcesViewModel(IDMEEditor dMEEditor,IAppManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
