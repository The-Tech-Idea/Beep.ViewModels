using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;

namespace TheTechIdea.Beep.MVVM.ViewModels
{
    [Addin(Caption = "Create Local DB", Name = "CreateLocalDBViewModel", addinType = AddinType.Class)]
    public partial class CreateLocalDBViewModel : BaseViewModel
    {
        public CreateLocalDBViewModel(IDMEEditor pEditor, IAppManager visManager) : base(pEditor, visManager)
        {
            Editor = pEditor;
            VisManager = visManager;
        }
        [ObservableProperty]
        public string dbName;
        [ObservableProperty]
        public string dbPath;
        [ObservableProperty]
        public string dbType;
        [ObservableProperty]
        public string dbDriver;
        [ObservableProperty]
        public string dbConnectionString;
        [ObservableProperty]
        public bool isChanged = false;
        [ObservableProperty]
        public bool isNew = false;
        [ObservableProperty]
        public List<ConnectionDriversConfig> drivers = new List<ConnectionDriversConfig>();
        [ObservableProperty]
        public ConnectionDriversConfig selectedDriver;

        [RelayCommand]
        public void CreateDB()
        {
            if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(dbPath) || string.IsNullOrEmpty(dbType))
            {
                // Show error message
                return;
            }
            // Create the database using the selected driver and connection string
            // Implement the logic to create the database here
            // Set IsChanged to false after creating the database
            IsChanged = false;
        }
        [RelayCommand]
        public void LoadDrivers()
        {
            // Load the drivers from the configuration or any other source
            // For example, you can load them from a JSON file or a database
            // Populate the Drivers property with the loaded drivers
            Drivers = Editor.ConfigEditor.DataDriversClasses
                .Where(d => d.CreateLocal)
                .ToList();
        }
        [RelayCommand]
        public void SelectDriver()
        {
            if (SelectedDriver != null)
            {
                DbType = SelectedDriver.PackageName;
                DbConnectionString = SelectedDriver.ConnectionString;
            }
        }
        [RelayCommand]
        public void BrowseDBPath()
        {
            // Implement the logic to open a file dialog and select the database path
            // Set DbPath to the selected path
            // For example:
            // using (var dialog = new SaveFileDialog())
            // {
            //     dialog.Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*";
            //     if (dialog.ShowDialog() == DialogResult.OK)
            //     {
            //         DbPath = dialog.FileName;
            //     }
            // }
        }
    }
}
