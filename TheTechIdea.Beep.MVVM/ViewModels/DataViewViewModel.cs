using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DataView;

using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;

using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class DataViewViewModel:BaseViewModel
    {
        [ObservableProperty]
        string entityname;
        [ObservableProperty]
        Type entityType;
        [ObservableProperty]
        string datasourceName;
        [ObservableProperty]
        EntityStructure currStructure;
        [ObservableProperty]
        ObservableBindingList<AppFilter> filters;
        [ObservableProperty]
        ObservableBindingList<EntityField> fields;
        [ObservableProperty]
        ObservableBindingList<EntityField> oldfields;
        [ObservableProperty]
        ObservableBindingList<EntityStructure> entities;
        [ObservableProperty]
        IDataSource dataSource;

        [ObservableProperty]
        IDataViewDataSource dataView;

        [ObservableProperty]
        UnitofWork<EntityStructure> entitiesunitofWork;

        [ObservableProperty]
        string fileName;
        [ObservableProperty]
        string filePath;
        [ObservableProperty]
        string fromFileName;
        [ObservableProperty]
        string fromFilePath;
        [ObservableProperty]
        string fullPathandFileName;
        public DataViewViewModel(IDMEEditor pEditor, IVisManager visManager) : base(pEditor, visManager)
        {
          

        }
        public bool CreateDataView()
        {
            try
            {
                DataSource =new DataViewDataSource(FullPathandFileName,Editor.Logger,Editor, DataSourceType.Json,Editor.ErrorObject);
                DataView= (IDataViewDataSource)DataSource;
                EntitiesunitofWork = new UnitofWork<EntityStructure>(Editor, true, new ObservableBindingList<EntityStructure>(DataSource.Entities), "GuidID");
                DataView.GenerateView(FileName, FileName);
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Creating DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool GetData()
        {
            try
            {
                if( string.IsNullOrEmpty(DatasourceName))
                {
                    return false;
                }
                DataSource=Editor.GetDataSource(DatasourceName);
                if (DataSource == null)
                {
                    return false;
                }
                DataSource.GetEntitesList();
                foreach (var item in DataSource.EntitiesNames)
                {
                    DataSource.GetEntityStructure(item,false);
                }
                FileName = DataSource.Dataconnection.ConnectionProp.FileName;
                FilePath = DataSource.Dataconnection.ConnectionProp.FilePath;
                FullPathandFileName=Path.Combine(FilePath, FileName);
                DataView = (IDataViewDataSource)DataSource;
                EntitiesunitofWork = new UnitofWork<EntityStructure>(Editor,true, new ObservableBindingList<EntityStructure>(DataSource.Entities), "GuidID");
                try
                {
                    Entities = Task.Run(() => EntitiesunitofWork.Get()).Result;
                    
                }
                catch (Exception ex)
                {
                    Editor.AddLogMessage("Error", $"Error in getting entity Data: {ex.Message}", DateTime.Now, 0, "", Errors.Failed);
                   return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Getting DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool SaveDataView()
        {
            try
            {
                DataView.WriteDataViewFile(FullPathandFileName);
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep",$"Error Saving DataView - {ex.Message}", DateTime.Now, 0, null,Errors.Failed   );
                return false;
            }
        }
        public bool LoadDataView()
        {
            try
            {
                DataView.ReadDataViewFile(FullPathandFileName);
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Loading DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool MoveDataViewFile(string newpath)
        {
            try
            {
                if (File.Exists(FullPathandFileName))
                {
                    if(newpath != FilePath)
                    {
                        File.Move(FullPathandFileName, Path.Combine(newpath, FileName));
                    }
                   
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Moving DataView File - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool UpdateEntity()
        {
            try
            {
                int idx = DataView.Entities.FindIndex(p => p.GuidID == CurrStructure.GuidID);
                if (idx > -1)
                {
                    DataView.Entities[idx] = CurrStructure;
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Updating Entity in DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool UpdateEntity(EntityStructure ent)
        {
            try
            { 
                int idx = DataView.Entities.IndexOf(DataView.Entities.FirstOrDefault(c => c.GuidID == ent.GuidID));
                if (idx > -1)
                {
                    DataView.Entities[idx] = ent;
                }

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Updating Entity in DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool AddEntities(List<EntityStructure> ents)
        {
            try
            {
                foreach (var item in ents)
                {
                    if(DataView.Entities.FirstOrDefault(c => c.EntityName == item.EntityName) == null)
                    {
                        DataView.AddEntitytoDataView(item);
                    }
                }
               
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error adding Entities to DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool AddEntities(EntityStructure ent)
        {
            try
            {
              
                    if (DataView.Entities.FirstOrDefault(c => c.EntityName == ent.EntityName) == null)
                    {
                        DataView.AddEntitytoDataView(ent);
                }
              

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error adding Entity to DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool RemoveEntity(string EntityName)
        {
            try
            {
                EntityStructure ent = DataView.Entities.FirstOrDefault(c => c.EntityName == EntityName);
                if (ent != null)
                {
                    DataView.RemoveEntity(ent.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Removing Entity from DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool RemoveEntity(EntityStructure ent)
        {
            try
            {
                EntityStructure ent1 = DataView.Entities.FirstOrDefault(c => c.EntityName == ent.EntityName);
                if (ent1 != null)
                {
                    DataView.RemoveEntity(ent1.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Removing Entity from DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool RemoveEntities(List<EntityStructure> ents)
        {
            try
            {
                foreach (var item in ents)
                {
                    EntityStructure ent1 = DataView.Entities.FirstOrDefault(c => c.EntityName == item.EntityName);
                    if (ent1 != null)
                    {
                        DataView.Entities.Remove(ent1);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Removing Entities from DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool AddFilter(EntityStructure ent, AppFilter filter)
        {
            try
            {
                EntityStructure ent1 = DataView.Entities.FirstOrDefault(c => c.EntityName == ent.EntityName);
                if (ent1 != null)
                {
                   
                        ent1.Filters.Add(filter);
                }

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error adding Filter to DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool AddChildEntity(EntityStructure parent,EntityField parentfield, EntityStructure child,EntityField childfield)
        {
            try
            {
                int parentidx = DataView.Entities.IndexOf(parent);
                int childidx = DataView.Entities.IndexOf(child);
               
                if (parentidx>-1 && childidx>-1)
                {
                    EntityStructure parententity = DataView.Entities[parentidx];
                    EntityStructure childentity = DataView.Entities[childidx];
                    if (parententity != null && childentity!=null)
                    {
                        RelationShipKeys relationShipKeys = new RelationShipKeys();
                        relationShipKeys.RelatedEntityID = childentity.EntityName;
                        relationShipKeys.RelatedEntityColumnID = childfield.fieldname;
                        relationShipKeys.EntityColumnID = parentfield.fieldname;
                        parententity.Relations.Add(relationShipKeys);

                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error adding Child Entity to DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool RemoveChildEntity(EntityStructure parent, EntityField parentfield, EntityStructure child, EntityField childfield)
        {
            try
            {
                int parentidx = DataView.Entities.IndexOf(parent);
                int childidx = DataView.Entities.IndexOf(child);

                if (parentidx > -1 && childidx > -1)
                {
                    EntityStructure parententity = DataView.Entities[parentidx];
                    EntityStructure childentity = DataView.Entities[childidx];
                    if (parententity != null && childentity != null)
                    {
                        RelationShipKeys relationShipKeys = parententity.Relations.FirstOrDefault(c => c.RelatedEntityID == childentity.EntityName && c.RelatedEntityColumnID == childfield.fieldname && c.EntityColumnID == parentfield.fieldname);
                        if (relationShipKeys != null)
                        {
                            parententity.Relations.Remove(relationShipKeys);
                        }
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Removing Child Entity from DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
        public bool MoveEntityFromParentToOtherParent(EntityStructure parent, EntityField parentfield, EntityStructure child, EntityField childfield, EntityStructure otherparent, EntityField otherparentfield)
        {
            try
            {
                int parentidx = DataView.Entities.IndexOf(parent);
                int childidx = DataView.Entities.IndexOf(child);
                int otherparentidx = DataView.Entities.IndexOf(otherparent);
                if (parentidx > -1 && childidx > -1 && otherparentidx > -1)
                {
                    EntityStructure parententity = DataView.Entities[parentidx];
                    EntityStructure childentity = DataView.Entities[childidx];
                    EntityStructure otherparententity = DataView.Entities[otherparentidx];
                    if (parententity != null && childentity != null && otherparententity != null)
                    {
                        RelationShipKeys relationShipKeys = parententity.Relations.FirstOrDefault(c => c.RelatedEntityID == childentity.EntityName && c.RelatedEntityColumnID == childfield.fieldname && c.EntityColumnID == parentfield.fieldname);
                        if (relationShipKeys != null)
                        {
                            parententity.Relations.Remove(relationShipKeys);
                            RelationShipKeys relationShipKeys1 = new RelationShipKeys();
                            relationShipKeys1.RelatedEntityID = childentity.EntityName;
                            relationShipKeys1.RelatedEntityColumnID = childfield.fieldname;
                            relationShipKeys1.EntityColumnID = otherparentfield.fieldname;
                            otherparententity.Relations.Add(relationShipKeys1);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Moving Child Entity from Parent to Other Parent in DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }

        }
        public bool AddEntityandChildsFromDataSourceToDataView(IDataSource sourceds,string sourceentityname)
        {
            try
            {
                EntityStructure ent = sourceds.Entities.FirstOrDefault(c => c.EntityName == sourceentityname);
                if (ent != null)
                {
                    if (DataView.Entities.FirstOrDefault(c => c.EntityName == ent.EntityName) == null)
                    {
                        DataView.AddEntitytoDataView(ent);
                    }
                    foreach (var item in ent.Relations)
                    {
                        EntityStructure child = sourceds.Entities.FirstOrDefault(c => c.EntityName == item.RelatedEntityID);
                        if (child != null)
                        {
                            if (DataView.Entities.FirstOrDefault(c => c.EntityName == child.EntityName) == null)
                            {
                                DataView.AddEntitytoDataView(child);
                                AddChildEntity(ent, ent.Fields.FirstOrDefault(c => c.fieldname == item.EntityColumnID), child, child.Fields.FirstOrDefault(c => c.fieldname == item.RelatedEntityColumnID));
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Adding Entity and Childs from DataSource to DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }

        }
        public bool RemoveEntityandChilds(string EntityName)
        {
            try
            {
                EntityStructure ent = DataView.Entities.FirstOrDefault(c => c.EntityName == EntityName);
                if (ent != null)
                {
                  
                    foreach (var item in ent.Relations)
                    {
                        EntityStructure child = DataView.Entities.FirstOrDefault(c => c.GuidID == item.RelatedEntityID);
                        if (child != null)
                        {
                            DataView.RemoveEntity(child.Id);
                        }
                    }
                    DataView.RemoveEntity(ent.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                Editor.AddLogMessage("Beep", $"Error Removing Entity and Childs from DataView - {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }
    }
}
