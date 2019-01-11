using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// missing AzureIotLib
//using AzureIotLib;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;


/*namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }
    }
}*/

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private Map _map;
        private Plugin.Geolocator.Abstractions.Position _myPosition;
        private Timer _heartbeatTimer;
        // missing AzureIotLib
        //private AzureIotDevice _azureIotDevice;

        //*********************************************************************
        ///
        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        //*********************************************************************

        public MapPage()
        {
            InitializeComponent();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// The page is about to appear
        /// </summary>
        /// 
        //*********************************************************************

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShowMap();
            //Xamarin.Forms.Device.BeginInvokeOnMainThread(ShowCurrentPositionOnMap);
            Xamarin.Forms.Device.BeginInvokeOnMainThread(ConnectToIotHub);
            StartHeartbeatTimer();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Show the map, after getting permission from the OS to do so
        /// </summary>
        /// 
        //*********************************************************************

        private async Task<bool> ShowMap()
        {
            if (!await GetPermissions(new List<Permission>()
                { Permission.Location, Permission.LocationWhenInUse }))
                return false;

            try
            {
                _map = new Map(
                    MapSpan.FromCenterAndRadius(
                        new Position(47.6062, -122.3321), Distance.FromMiles(0.3)))
                {
                    IsShowingUser = true,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                var stack = new StackLayout { Spacing = 0 };
                stack.Children.Add(_map);
                Content = stack;

                ShowCurrentPositionOnMap();

                return true;
            }
            catch (System.Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
                return false;
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void ShowCurrentPositionOnMap()
        {
            Plugin.Geolocator.Abstractions.Position pos;


            try
            {
                pos = await Services.Geolocation.GetCurrentPosition();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
                Console.WriteLine(ex);
                return;
            }

            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(pos.Latitude, pos.Longitude), Distance.FromMiles(0.3)));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ///
        //*********************************************************************

        private async Task<bool> ShowMapOld()
        {
            if (!await GetPermissions(new List<Permission>()
                { Permission.Location, Permission.LocationWhenInUse, Permission.LocationAlways }))
                return false;

            try
            {
                var map = new Map(
                    MapSpan.FromCenterAndRadius(
                        new Position(37, -122), Distance.FromMiles(0.3)))
                {
                    IsShowingUser = true,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                var stack = new StackLayout { Spacing = 0 };
                stack.Children.Add(map);
                Content = stack;

                return true;
            }
            catch (System.Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
                return false;
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Request permissions from the OS 
        /// </summary>
        /// <param name="permissionsList"></param>
        /// <returns></returns>
        /// 
        //*********************************************************************

        private async Task<bool> GetPermissions(List<Permission> permissionsList)
        {
            bool permissionsGranted = true;
            try
            {
                foreach (var permission in permissionsList)
                {
                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);

                    if (status != PermissionStatus.Granted)
                    {
                        if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permission))
                        {
                            await App.Current.MainPage.DisplayAlert("Need a permission", $"Required: {permission.ToString()}", "OK");
                        }

                        var results = await CrossPermissions.Current.RequestPermissionsAsync(permission);
                        //Best practice to always check that the key exists
                        if (results.ContainsKey(permission))
                            status = results[permission];

                        if (!(status == PermissionStatus.Granted || status == PermissionStatus.Unknown))
                        {
                            await App.Current.MainPage.DisplayAlert("Permission Denied", "Can not continue, try again.", "OK");
                            permissionsGranted = false;
                            break;
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
            }

            return permissionsGranted;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void StartHeartbeatTimer()
        {
            object obj = null;
            _heartbeatTimer = new Timer(HeartbeatTimerCallback, obj, new TimeSpan(0, 0, 0, 0), new TimeSpan(0, 0, 5, 0));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        ///
        //*********************************************************************

        private void HeartbeatTimerCallback(object state)
        {
            double lat = 0, lon = 0;
            try
            {
                _myPosition = Services.Geolocation.GetCurrentPosition().Result;
                lat = _myPosition.Latitude;
                lon = _myPosition.Longitude;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            SendPositionUpdate(lat, lon);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void ConnectToIotHub()
        {
            // missing AzureIotLib
            /*_azureIotDevice = new AzureIotDevice();

            //await ait.GetDeviceTwinList();

            await _azureIotDevice.ConnectToDevice(GotMessageCallback);*/
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        ///
        //*********************************************************************

        public async void SendPositionUpdate(double lat, double lon)
        {
            //_azureIotDevice.SendD2C($"{{\"state\":{{\"reported\":{{\"lat\":\"{lat}\",\"lon\":\"{lon}\"}}}}}}");

            // missing AzureIotLib
            //_azureIotDevice.SendD2C($"{{\"payload_fields\":{{\"latitude\":\"{lat}\",\"longitude\":\"{lon}\"}}}}");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        ///
        //*********************************************************************

        private void ShowTrackedObjectLocation(double lat, double lon)
        {
            try
            {
                var position = new Position(lat, lon); // Latitude, Longitude
                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = "Dr. Elkman",
                    Address = $"Time:{DateTime.UtcNow.ToString()}"
                };
                _map.Pins.Add(pin);

                _map.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        position, Distance.FromMiles(1)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messagepayload"></param>
        ///
        //*********************************************************************

        private void GotMessageCallback(byte[] messagepayload)
        {
            if (null == messagepayload)
                return;

            string strData = Encoding.UTF8.GetString(messagepayload);

            dynamic jsonObj = JsonConvert.DeserializeObject(strData);

            // missing AzureIotLib
            /*string latS = jsonObj.a;
            string lonS = jsonObj.o;

            var lat = Convert.ToDouble(latS);
            var lon = Convert.ToDouble(lonS);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => { ShowTrackedObjectLocation(lat, lon); });*/
        }
    }
}