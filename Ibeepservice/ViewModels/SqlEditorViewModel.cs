using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep.Vis.Module;

using TheTechIdea.Beep.Container.Services;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class SqlEditorViewModel:BaseViewModel
    {
        public SqlEditorViewModel(IBeepService beepService) : base(beepService)
        {
        }
        

    }
}
