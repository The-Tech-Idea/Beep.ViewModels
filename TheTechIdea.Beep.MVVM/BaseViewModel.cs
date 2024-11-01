using TheTechIdea.Beep.Vis.Modules;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Util;

namespace TheTechIdea.Beep.MVVM
{
    public partial class BaseViewModel : ObservableObject,IDisposable
    {
        [ObservableProperty]
        bool isNew;
        [ObservableProperty]
        bool isDirty;
        [ObservableProperty]
        bool isChanged;
        [ObservableProperty]
        bool isCreated;
        [ObservableProperty]
        bool isUpdated;
        [ObservableProperty]
        bool isDeleted;
        [ObservableProperty]
        bool isAdded;
        [ObservableProperty]
        bool isBusy;
        [ObservableProperty]
        bool isReady;
        [ObservableProperty]
        bool isRunning;
        [ObservableProperty]
        bool isStopped;
        [ObservableProperty]
        bool isPaused;
        [ObservableProperty]
        bool isLoaded;
        [ObservableProperty]
        bool isSaved;
        [ObservableProperty]
        bool isModified;
        [ObservableProperty]
        bool isEditing;
        [ObservableProperty]
        bool isAdding;
        [ObservableProperty]
        bool isDeleting;
        [ObservableProperty]
        bool isSearching;
        [ObservableProperty]
        bool isPrinting;
        [ObservableProperty]
        bool isOpen;
        [ObservableProperty]
        bool isClose;
        [ObservableProperty]
        BaseViewModel currentViewModel;
        [ObservableProperty]
        IDMEEditor editor;
        [ObservableProperty]
        Progress<PassedArgs> waitprogress;
        [ObservableProperty]
        Progress<PassedArgs> logprogress;
        [ObservableProperty]
        CancellationToken token;
        private bool disposedValue;
        [ObservableProperty]
        string logoname = "SimpleODM.ico";
        [ObservableProperty]
        string title = "Beep The Data Plaform";
        [ObservableProperty]
        string version = "";
        [ObservableProperty]
        string build = "";
        [ObservableProperty]
        string buildDate = "";
        [ObservableProperty]
        string buildTime = "";
        [ObservableProperty]
        List<MenuItem> menuItems;
        [ObservableProperty]
        List<MenuItem> toolBarItems;
        [ObservableProperty]
        List<MenuItem> contextMenuItems;
        [ObservableProperty]
        List<MenuItem> statusItems;
        [ObservableProperty]
        List<MenuItem> quickAccessItems;
        [ObservableProperty]
        List<MenuItem> helpItems;
        [ObservableProperty]
        List<MenuItem> settingsItems;
        [ObservableProperty]
        List<MenuItem> userItems;
        [ObservableProperty]
        List<MenuItem> adminItems;
        [ObservableProperty]
        List<MenuItem> developerItems;
        [ObservableProperty]
        List<IBranch> branchs;
        [ObservableProperty]
        IBeepService beepService;
        [ObservableProperty]
        IErrorsInfo errorObject;
        [ObservableProperty]
        EntityStructure currentEntity;
        [ObservableProperty]
        IDataSource currDataSource;
        [ObservableProperty]
        string currentEntityName;
        [ObservableProperty]
        string currentDataSourceName;
        public IVisManager VisManager { get; set; }

        public BaseViewModel()
        {
                
        }
        public BaseViewModel(IDMEEditor peditor, IVisManager visManager)
        {
          
            Editor = peditor;

            VisManager = visManager;
            if(!string.IsNullOrEmpty(visManager.Title ))
            {
                title = visManager.Title;
            }
            if(!string.IsNullOrEmpty(visManager.IconUrl))
            {
                logoname = visManager.IconUrl;
            }
            
            waitprogress =new Progress<PassedArgs>(AddWait);
            logprogress = new Progress<PassedArgs>(AddLog);
            token =new CancellationToken();
            if (CurrentViewModel != null)
            {
                CurrentViewModel.PropertyChanged += CurrentViewModel_PropertyChanged;
            }

           
           
        }

        private void CurrentViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
        public void  AddLogMessege(string message)
        {
            PassedArgs args = new PassedArgs();
            args.Messege = message;
            AddLog(args);
        }
        public void AddWaitMessege(string message)
        {
            PassedArgs args = new PassedArgs();
            args.Messege = message;
            AddWait(args);
        }
        [RelayCommand]
        void AddLog(PassedArgs message)
        {
            if (message!=null)
            {
               
                if (string.IsNullOrEmpty(message.Messege))
                {
                    if (Editor != null)
                    {
                        Editor.AddLogMessage(message.Messege);
                    }
                }
            }
        }
        [RelayCommand]
        void AddWait(PassedArgs message)
        {
            if (message != null)
            {
               
                if (string.IsNullOrEmpty(message.Messege))
                {
                    
                    if (VisManager != null)
                    {
                        if (!VisManager.IsShowingWaitForm)
                        {
                            VisManager.ShowWaitForm(message);
                         
                        }
                        VisManager.PasstoWaitForm(message);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {  // Dispose when app is closed
                    VisManager.Dispose();
                    Editor!.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
