using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Editor;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using CommunityToolkit.Mvvm.Input;


namespace TheTechIdea.Beep.MVVM.ViewModels
{
    [Addin(Caption = "Beep BaseViewModel", Name = "BaseViewModel", addinType = AddinType.Class)]
    public partial class FunctionToFunctionMappingViewModel : BaseViewModel
    {
        [ObservableProperty]
        public List<string> fromEvents;
        [ObservableProperty]
        public List<string> toEvents;
        [ObservableProperty]    
        public List<string> fromFunctions;
        [ObservableProperty]
        public List<string> toFunctions;
        [ObservableProperty]
        public List<string> fromClasses;
        [ObservableProperty]
        public List<string> toClasses;
        [ObservableProperty]
        public UnitofWork<Function2FunctionAction> dBWork;
       
        public ObservableBindingList<Function2FunctionAction> Units { get { return DBWork.Units; } }
        [ObservableProperty]
        public List<string> actiontypes;
        [ObservableProperty]
        public AddinType fromClassType;
        [ObservableProperty]
        public AddinType toClassType;
     

        public FunctionToFunctionMappingViewModel(IDMEEditor pEditor, IAppManager visManager) : base(pEditor, visManager)
        {
             Editor=pEditor;
            VisManager = visManager;
             DBWork = new UnitofWork<Function2FunctionAction>(Editor, true, new ObservableBindingList<Function2FunctionAction>(Editor.ConfigEditor.Function2Functions), "GuidID");
             DBWork.PreInsert += Unitofwork_PreInsert;
            ToClasses=new   List<string>();
            FromClasses=new   List<string>();   
            Actiontypes=new   List<string>();
            foreach (var item in Editor.ConfigEditor.BranchesClasses)
            {
                ToClasses.Add(item.className);
                FromClasses.Add(item.className);
            }
          

            actiontypes.Add("Event");
            actiontypes.Add("Function");
        }
        [RelayCommand]
        public void LoadData()
        {
            DBWork.Units= new ObservableBindingList<Function2FunctionAction>(Editor.ConfigEditor.Function2Functions);

        }
        [RelayCommand]
        public void SaveData()
        {
            Editor.ConfigEditor.Function2Functions = DBWork.Units.ToList();
            Editor.ConfigEditor.SaveFucntion2Function();
        }
        private void Unitofwork_PreInsert(object sender, UnitofWorkParams e)
        {
            
        }
        [RelayCommand]
        public void GetFromFunctions(string fromclass)
        {
            FromFunctions.Clear();
            foreach (var item in Editor.ConfigEditor.BranchesClasses.Where(x => x.className == fromclass).FirstOrDefault().Methods)
            {
                FromFunctions.Add(item.Caption);
            }
        }
        [RelayCommand]
        public void GetToFunctions(string toclass)
        {
            ToFunctions.Clear();
            foreach (var item in Editor.ConfigEditor.BranchesClasses.Where(x => x.className == toclass).FirstOrDefault().Methods)
            {
                ToFunctions.Add(item.Caption);
            }
        }
    }
}
