using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DataSourceDefaultsViewModel : BaseViewModel
    {
        [ObservableProperty]
        public string dataSourceName;
        [ObservableProperty]
        public List<AssemblyClassDefinition> rules;
        [ObservableProperty]
        public string selectedRule;
    }
}
