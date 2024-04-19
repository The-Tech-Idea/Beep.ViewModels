using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Helpers;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    public partial class SqlEditorViewModel:BaseViewModel
    {
        [ObservableProperty]
        string datasourcename;
        [ObservableProperty]
        bool isconnected;
        [ObservableProperty]
        string sqltext;
        [ObservableProperty]
        string sqlresult;
        [ObservableProperty]
        string sqlerror;
        [ObservableProperty]
        string sqlmessage;
        [ObservableProperty]
        string sqlstatus;
        [ObservableProperty]
        string sqltype;
        [ObservableProperty]
        string sqlversion;
        [ObservableProperty]
        string errors;
        [ObservableProperty]
        object sqlresultset;
        [ObservableProperty]
        List<string> entities;
        public IDataSource dataSource { get; set; }
        public SqlEditorViewModel(IDMEEditor dMEEditor, IVisManager visManager) : base(dMEEditor, visManager)
        {
            Editor = dMEEditor;
            VisManager = visManager;


        }
        public bool RunSql()
        {
            try
            {
                if(Isconnected)
                {
                    if (RDBMSHelper.IsSqlStatementValid(Sqltext))
                    {
                        Sqlresultset = dataSource.RunQuery(Sqltext);
                    }else
                    {
                        Errors = $"Its not a Valid  Sql ";
                        Editor.AddLogMessage("Beep", Errors, DateTime.Now, 0, null, TheTechIdea.Util.Errors.Failed);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Errors = $"Error Runing Sql {ex.Message}";
                Editor.AddLogMessage("Beep", $"Error Runing Sql {ex.Message}", DateTime.Now, 0, null, TheTechIdea.Util.Errors.Failed);
            }
            return true;
        }
        public System.Data.ConnectionState Connect()
        {
            Entities = new List<string>();
            if (!string.IsNullOrEmpty(Datasourcename))
            {
                dataSource = Editor.GetDataSource(Datasourcename);
                if(dataSource!=null)
                {
                  if(dataSource.Openconnection()== System.Data.ConnectionState.Open)
                    {
                        Isconnected = true;
                        dataSource.GetEntitesList();
                        Entities = dataSource.EntitiesNames;
                        return System.Data.ConnectionState.Open;
                    }
                    else
                    {
                        Isconnected = false;
                        Errors = $@"Error in connecting to datasource /n , datasourcename cannot connected";
                        return System.Data.ConnectionState.Closed;
                    }   
                }
                else
                {
                    Isconnected = false;
                    Errors = $@"Error in connecting to datasource /n , datasourcename is not found";
                    return System.Data.ConnectionState.Broken;
                }
            }
            else
            {
                Isconnected = false;
                Errors = $@"Error in connecting to datasource /n , datasourcename string is null";
                return System.Data.ConnectionState.Broken;
            }
           
        }
        public bool Disconnect()
        {
            dataSource.Closeconnection();
            if(dataSource.ConnectionStatus== System.Data.ConnectionState.Closed)
            {
                Isconnected = false;
                return true;
            }
            else
            {
                Isconnected = true;
                return false;
            }
            
        }
        public bool SaveSql()
        {
            return true;
        }
        public bool LoadSql()
        {
            return true;
        }
        public bool NewSql()
        {
            return true;
        }
        public bool OpenSql()
        {
            return true;
        }
        public bool CloseSql()
        {
            return true;
        }
        

    }
}
