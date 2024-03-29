﻿using Beep.Vis.Module;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;


namespace TheTechIdea.Beep.MVVM
{
    public partial class BaseViewModel : ObservableObject,IDisposable
    {
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

     

        public IVisManager VisManager { get; set; }

        public BaseViewModel()
        {
                
        }
        public BaseViewModel(IDMEEditor dMEEditor, IVisManager visManager)
        {
          
            this.editor = dMEEditor;
            VisManager = visManager;
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
