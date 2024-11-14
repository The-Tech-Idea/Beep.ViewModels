using TheTechIdea.Beep.Vis.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class ResetPasswordViewModel : BaseViewModel
    {
        public ResetPasswordViewModel(IDMEEditor dMEEditor,IVisManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
