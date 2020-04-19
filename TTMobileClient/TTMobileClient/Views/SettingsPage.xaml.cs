using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        TTMobileClient.PortableAppSettings _portableAppSettings = null;
        bool _areSettingsLoaded = false;
        private bool _useFakeSettingsData = false;

        ///********************************************************************
        /// <summary>
        /// Constructor
        /// </summary>
        ///********************************************************************

        public SettingsPage()
        {
            InitializeComponent();
        }
   
        ///********************************************************************
        /// <summary>
        /// 
        /// </summary>
        ///********************************************************************
        
        protected override void OnAppearing()
        {
            if (_useFakeSettingsData)
            {
                _portableAppSettings = PortableAppSettings.GetTestData();
                CreateSettingsTable();
            }
            else
            {
                if (0 < AppSettings.App.RemoteAppSettings.Count)
                {
                    //for now just take the first one, it's probably all we will ever need
                    _portableAppSettings = AppSettings.App.RemoteAppSettings[0];
                    CreateSettingsTable();
                }
            }

            base.OnAppearing();
        }

        ///********************************************************************
        /// <summary>
        /// Create table of settings values from _portableAppSettings
        /// </summary>
        ///********************************************************************

        private void CreateSettingsTable()
        {
            //prevent PropertyChanged events from being handled while building
            //is underway
            _areSettingsLoaded = false;

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

            //create UI buttons
            var saveButton = new Button { Text = "Save", BackgroundColor = Color.Gray };
            var cancelButton = new Button { Text = "Cancel", BackgroundColor = Color.Gray };

            saveButton.Clicked += SaveButton_Clicked;
            cancelButton.Clicked += CancelButton_Clicked;

            //create page contents
            Content = new StackLayout { Spacing = 0, 
                Orientation = StackOrientation.Vertical, 
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { 
                    new ScrollView { Content = table },
                    new StackLayout
                    {
                        IsVisible = true,
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Orientation = StackOrientation.Horizontal,
                        Children = { saveButton, cancelButton }
                    }}
            };

            //this has to be on a timer because PropertyChanged events fly out
            //with some delay
            var _wsApiTestTimer = 
                new Timer((arg) => _areSettingsLoaded = true, new object(),
                    new TimeSpan(0, 0, 0, 1), new TimeSpan(0, 0, 0, 0, -1));
        }

        ///********************************************************************
        /// <summary>
        /// User clicked save button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///********************************************************************
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            var result = await _portableAppSettings?.SaveChanges();

            if(result.Result == WebApiLib.ResultEnum.ok)
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    () => App.Current.MainPage.DisplayAlert(
                        "Success", "Settings updated", "Ok"));
            else 
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    () => App.Current.MainPage.DisplayAlert(
                         "Error", result.Result.ToString(), "Ok"));
        }

        ///********************************************************************
        /// <summary>
        /// User clicked cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///********************************************************************
        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            //noop
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