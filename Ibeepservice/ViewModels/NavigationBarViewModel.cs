﻿using Beep.Vis.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;
namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public class NavigationBarViewModel : BaseViewModel
    {
        public NavigationBarViewModel(IBeepService beepService) : base( beepService)
        {
        }
    }
}
