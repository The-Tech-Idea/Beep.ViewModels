﻿using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Utilities;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    [Addin(Caption = "Beep BaseViewModel", Name = "BaseViewModel", addinType = AddinType.Class)]
    public class LoginViewModel : BaseViewModel
    {
       
        public LoginViewModel(IDMEEditor dMEEditor,IAppManager visManager) : base( dMEEditor, visManager)
        {
        }
    }
}
