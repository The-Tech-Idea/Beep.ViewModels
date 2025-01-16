using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Editor;
namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public class DictionaryQueryForRDBMSViewModel : BaseViewModel
    {
        public DictionaryQueryForRDBMSViewModel(IDMEEditor dMEEditor,IAppManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
