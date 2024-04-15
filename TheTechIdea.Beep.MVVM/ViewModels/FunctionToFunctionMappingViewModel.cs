using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataManagementModels.DriversConfigurations;
using DataManagementModels.Editor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using DataManagementModels.ConfigUtil;
using TheTechIdea.Util;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class FunctionToFunctionMappingViewModel : BaseViewModel
    {
        [ObservableProperty]
        List<string> fromEvents;
        [ObservableProperty]
        List<string> toEvents;
        [ObservableProperty]    
        List<string> fromFunctions;
        [ObservableProperty]
        List<string> toFunctions;
        [ObservableProperty]
        List<string> fromClasses;
        [ObservableProperty]
        List<string> toClasses;
       
        UnitofWork<Function2FunctionAction> DBWork;
        [ObservableProperty]
        List<string> actiontypes;
        [ObservableProperty]
        AddinType fromClassType;
        [ObservableProperty]
        AddinType toClassType;
        public ObservableBindingList<Function2FunctionAction> Function2FunctionActions => DBWork.Units;

        public FunctionToFunctionMappingViewModel(IDMEEditor Editor, IVisManager visManager) : base(Editor, visManager)
        {
             DMEEditor=Editor;
            VisManager = visManager;
             DBWork = new UnitofWork<Function2FunctionAction>(Editor, true, new ObservableBindingList<Function2FunctionAction>(DMEEditor.ConfigEditor.Function2Functions), "GuidID");
             DBWork.PreInsert += Unitofwork_PreInsert;
            ToClasses=new   List<string>();
            FromClasses=new   List<string>();   
            Actiontypes=new   List<string>();
            foreach (var item in DMEEditor.ConfigEditor.BranchesClasses)
            {
                ToClasses.Add(item.className);
                FromClasses.Add(item.className);
            }
          

            actiontypes.Add("Event");
            actiontypes.Add("Function");
        }
        public void LoadData()
        {
            DBWork.Units= new ObservableBindingList<Function2FunctionAction>(DMEEditor.ConfigEditor.Function2Functions);

        }
        public void SaveData()
        {
            DMEEditor.ConfigEditor.Function2Functions = DBWork.Units.ToList();
            DMEEditor.ConfigEditor.SaveFucntion2Function();
        }
        private void Unitofwork_PreInsert(object sender, UnitofWorkParams e)
        {
            throw new NotImplementedException();
        }
        public void GetFromFunctions(string fromclass)
        {
            FromFunctions.Clear();
            foreach (var item in DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.className == fromclass).FirstOrDefault().Methods)
            {
                FromFunctions.Add(item.Caption);
            }
        }
        public void GetToFunctions(string toclass)
        {
            ToFunctions.Clear();
            foreach (var item in DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.className == toclass).FirstOrDefault().Methods)
            {
                ToFunctions.Add(item.Caption);
            }
        }
    }
}
