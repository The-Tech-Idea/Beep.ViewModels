using TheTechIdea.Beep.Vis.Modules;

using TheTechIdea.Beep.Container.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using System.Linq;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor.UOW;

namespace TheTechIdea.Beep.MVVM.ViewModels.BeepConfig
{
    public partial class DataTypeMappingViewModel : BaseViewModel
    {
        [ObservableProperty]
        List<string> dataClasses;
        [ObservableProperty]
        string[] dataTypes;

        public UnitofWork<DatatypeMapping> DBWork { get; set; }
        public DataTypeMappingViewModel(IDMEEditor dMEEditor,IAppManager visManager) : base( dMEEditor, visManager)
        {
            
            DBWork = new UnitofWork<DatatypeMapping>(dMEEditor, true, new ObservableBindingList<DatatypeMapping>(Editor.ConfigEditor.DataTypesMap), "GuidID");
            dataTypes = Editor.typesHelper.GetNetDataTypes2();
            dataClasses=Editor.typesHelper.GetDataClasses();
        }
        public IErrorsInfo Save()
        {
            try
            {
                Editor.ConfigEditor.DataTypesMap = DBWork.Units.ToList();
                Editor.ConfigEditor.WriteDataTypeFile();
                Editor.AddLogMessage("Beep", $"Success saving DataTypes Changes ", System.DateTime.Now, 0, null, Errors.Ok);
            }
            catch (System.Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error in saving DataTypes Changes {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);


            }
            return Editor.ErrorObject;
          
        }
        public IErrorsInfo Read()
        {
            try
            {
                Editor.ConfigEditor.ReadDataTypeFile();
                Editor.AddLogMessage("Beep", $"Success Reading DataTypes  ", System.DateTime.Now, 0, null, Errors.Ok);
            }
            catch (System.Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error in Reading DataTypes  {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);
            }
            return Editor.ErrorObject;
           
        }
    }
}
