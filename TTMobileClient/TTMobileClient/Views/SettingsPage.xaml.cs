using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        TTMobileClient.PortableAppSettings _portableAppSettings = null;
        bool _areSettingsLoaded = false;

        ///********************************************************************
        /// <summary>
        /// Constructor
        /// </summary>
        ///********************************************************************

        public SettingsPage()
        {
            InitializeComponent();

            _portableAppSettings = PortableAppSettings.GetTestData();

            CreateSettingsTable();
        }

        ///********************************************************************
        /// <summary>
        /// Create table of settings values from _portableAppSettings
        /// </summary>
        ///********************************************************************

        private void CreateSettingsTable()
        {
            EntryCell ec;
            SwitchCell sc;
            this.Title = "Settings";
            var table = new TableView() { Intent = TableIntent.Settings };
            table.Root = new TableRoot();

            foreach (var settingsCollection in _portableAppSettings.AppSettingCollections)
            {
                var section = new TableSection() { 
                    Title = settingsCollection.name, TextColor = Color.Turquoise };
                table.Root.Add(section);

                foreach (var setting in settingsCollection.AppSettings)
                {
                    string automationId = settingsCollection.name + setting.name;
                    switch (setting.Type)
                    {
                        case Type tipe when tipe == typeof(int):
                            ec = new EntryCell { Label = setting.name, 
                                Text = setting.Value.ToString(), 
                                Keyboard = Keyboard.Numeric, AutomationId = automationId };
                            ec.PropertyChanged += ValueUpdated;
                            section.Add(ec);
                            break;
                        case Type tipe when tipe == typeof(Int64):
                            ec = new EntryCell { Label = setting.name, 
                                Text = setting.Value.ToString(), 
                                Keyboard = Keyboard.Numeric, AutomationId = automationId };
                            ec.PropertyChanged += ValueUpdated;
                            section.Add(ec);
                            break;
                        case Type tipe when tipe == typeof(bool):
                            sc = new SwitchCell { Text = setting.name, 
                                On = (bool)setting.Value, OnColor = Color.Green, 
                                AutomationId = automationId };
                            sc.OnChanged += ValueUpdated;
                            section.Add(sc);
                            break;
                        case Type tipe when tipe == typeof(float):
                            ec = new EntryCell { Label = setting.name, 
                                Text = setting.Value.ToString(), 
                                Keyboard = Keyboard.Numeric, AutomationId = automationId };
                            ec.PropertyChanged += ValueUpdated;
                            section.Add(ec);
                            break;
                        case Type tipe when tipe == typeof(double):
                            ec = new EntryCell { Label = setting.name, 
                                Text = setting.Value.ToString(), 
                                Keyboard = Keyboard.Numeric, AutomationId = automationId };
                            ec.PropertyChanged += ValueUpdated;
                            section.Add(ec);
                            break;
                        case Type tipe when tipe == typeof(string):
                            ec = new EntryCell { Label = setting.name, 
                                Text = setting.Value.ToString(), 
                                Keyboard = Keyboard.Text, AutomationId = automationId };
                            ec.PropertyChanged += ValueUpdated;
                            section.Add(ec);
                            break;
                        default:
                            break;
                    }
                }
            }

            Content = table;
            _areSettingsLoaded = true;
        }

        ///********************************************************************
        /// <summary>
        /// Called whenever a value in the settings table changes, updates
        /// the value of the associated setting in _portableAppSettings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///********************************************************************

        private void ValueUpdated(object sender, EventArgs e)
        {
            if (!_areSettingsLoaded)
                return;

            var ec = sender as EntryCell;

            if (null != ec)
            {
                _portableAppSettings.UpdateValue(ec.AutomationId, ec.Text);
            }
            else
            {
                var sc = sender as SwitchCell;

                if (null != sc)
                    _portableAppSettings.UpdateValue(sc.AutomationId, sc.On);
            }
        }
    }
}