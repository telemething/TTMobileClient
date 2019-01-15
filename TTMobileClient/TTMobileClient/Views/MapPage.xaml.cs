﻿using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flitesys.GeographicLib;
// missing AzureIotLib
//using AzureIotLib;
using Newtonsoft.Json;
using Plugin.Geolocator;
using RosClientLib;
using RosSharp.RosBridgeClient.Messages.Test;
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
        //private Map _map;
        private CustomMap _map;
        private Plugin.Geolocator.Abstractions.Position _myPosition;
        private Timer _heartbeatTimer;
        private MissionStatus _missionStatus;
        // missing AzureIotLib
        //private AzureIotDevice _azureIotDevice;

        private Label StatusLatLabel;
        private Label StatusLongLabel;
        private Label StatusAltLabel;
        private Label StatusLandedLabel;

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


            //Xamarin.Forms.Device.BeginInvokeOnMainThread(ConnectToIotHub);
            //Xamarin.Forms.Device.BeginInvokeOnMainThread(StartTelemetry);

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
                // create map
                _map = new CustomMap(
                    MapSpan.FromCenterAndRadius(
                        new Position(47.6062, -122.3321), Distance.FromMiles(0.3)))
                {
                    IsShowingUser = true,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

        _map.OnMapClick += OnMapClick;

                //BuildCustomPins();

                // create map style buttons
                var street = new Button { Text = "Street" };
                var hybrid = new Button { Text = "Hybrid" };
                var satellite = new Button { Text = "Satellite" };
                var connect = new Button { Text = "Connect" };
                var sendMission = new Button { Text = "sendMission" };
                var StartMission = new Button { Text = "StartMission" };

                street.Clicked += HandleClicked;
                hybrid.Clicked += HandleClicked;
                satellite.Clicked += HandleClicked;
                connect.Clicked += HandleClicked;
                sendMission.Clicked += HandleClicked;
                StartMission.Clicked += HandleClicked;

                //var PositionLabel = new Label { Text = "This is a green label.", TextColor = Color.FromHex("#77d065"), FontSize = 20 };
                StatusLatLabel = new Label { Text = "---" };
                StatusLongLabel = new Label { Text = "---" };
                StatusAltLabel = new Label { Text = "---" };
                StatusLandedLabel = new Label { Text = "---" };

                var buttons = new StackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { street, hybrid, satellite, connect, sendMission, StartMission }
                };

                var telem = new StackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { StatusLandedLabel, StatusLatLabel, StatusLongLabel, StatusAltLabel }
                };

                var stack = new StackLayout { Spacing = 0 };
                stack.Children.Add(_map);
                stack.Children.Add(buttons);
                stack.Children.Add(telem);
                Content = stack;

         //ShowCurrentPositionOnMap();

         //TestDropCustomPin();

         //TestDrawPolyline();

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnMapClick(object sender, OnMapClickEventArgs e)
        {
            AddWaypoint(e.Lat, e.Lon, e.Alt);
        }

        private void AddWaypoint(double lat, double lon, double alt)
        {
            _map.AddWaypoint( new Waypoint
            {
                Type = PinType.Place,
                Position = new Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://xamarin.com/about/"
            });

            //_map.CustomPins = new List<CustomPin> { pin };
            //_map.Pins.Add(pin);

            //_map.CustomPins.Add(pin);
            //_map.Change = new ChangeHappened(){addedObject = pin};
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        void HandleClicked(object sender, EventArgs e)
        {
            var b = sender as Button;
            switch (b.Text)
            {
                case "Street":
                    _map.MapType = MapType.Street;
                    break;
                case "Hybrid":
                    _map.MapType = MapType.Hybrid;
                    break;
                case "Satellite":
                    _map.MapType = MapType.Satellite;
                    break;
                case "Connect":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(StartTelemetry);
                    break;
                case "sendMission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(SendMission);
                    break;
                case "StartMission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(StartMission);
                    break;
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
            _heartbeatTimer = new Timer(HeartbeatTimerCallback, obj, 
                new TimeSpan(0, 0, 0, 0), 
                new TimeSpan(0, 0, 5, 0));
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

            await _azureIotDevice.ConnectToDevice(GotAzureDeviceMessageCallback);*/
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        string TestUri = "ws://192.168.1.30:9090";
        private IRosClient _rosClient = null;

        private async void ConnectToMav()
        {
            if( null == _rosClient)
                _rosClient = new RosClientLib.RosClient(TestUri, 
                    OnConnected, OnConnectionFailed );
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnConnectionFailed(object sender, EventArgs e)
        {
            string message = "unspecified connection error";

            _rosClient.DisConnect();

            _rosClient = null;

            if (e is ConnectionEventArgs tt)
                message = tt.message;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () => App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + message, "Ok"));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnConnected(object sender, EventArgs e)
        {
            var tt = e as ConnectionEventArgs;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void SendMission()
        {
            var waypoints = new Waypoints();

            waypoints.StartNewMission();

            // take off from home
            waypoints.AddWaypoint(Mavros.Command.NAV_TAKEOFF, 
                _missionStatus.x_lat, _missionStatus.y_long, 10, 
                true, true, 5, 0, 0, 0);

            // travel to each waypoint
            foreach (var routeCoord in _map.Waypoints)
                waypoints.AddWaypoint(Mavros.Command.NAV_WAYPOINT,
                    routeCoord.Position.Latitude, routeCoord.Position.Longitude, 30, 
                    false, true, 5, 0, 0, 0);

            // return home and land
            waypoints.AddWaypoint(Mavros.Command.NAV_LAND,
                _missionStatus.x_lat, _missionStatus.y_long, 0, false, 
                true, 5, 0, 0, 0);

            // send the mission to ROS
            ConnectToMav();
            _rosClient.CallService<Waypoints.WaypointReqResp>(waypoints,
                resp => {
                    if (!resp.success)
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(
                            () => App.Current.MainPage.DisplayAlert(
                                "Error", "Unable to send waypoints", "Ok"));
                });
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void StartMission()
        {
            ConnectToMav();
            _rosClient.CallService<StartMission.StartMissionReqResp>(new StartMission(), 
                resp => {
                    if (!resp.success)
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(
                            () => App.Current.MainPage.DisplayAlert(
                                "Error", "Unable to start mission", "Ok"));
                });
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void StartTelemetry()
        {
            _moveMapToTrackedObject = true;

            ConnectToMav();

            var subscriptionId = _rosClient.Subscribe
                <RosSharp.RosBridgeClient.Messages.Test.MissionStatus>(
                    "/tt_mavros_wp_mission/MissionStatus",
                    TelemetrySubscriptionHandler);

            //rosClient.Unsubscribe(subscriptionId);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="missionStatus"></param>
        /// 
        //*********************************************************************

        private void TelemetrySubscriptionHandler(MissionStatus missionStatus)
        {
            // do something to prevent high frequency updating

            _missionStatus = missionStatus;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    StatusLatLabel.Text = $"Lat: {missionStatus.x_lat}";
                    StatusLongLabel.Text = $"Lon: {missionStatus.y_long}";
                    StatusAltLabel.Text = $"Alt: {Math.Round(missionStatus.z_alt, 2)}";
                    StatusLandedLabel.Text = $"State: {missionStatus.landed_state}";

                    ShowTrackedObjectLocation(
                        missionStatus.x_lat, missionStatus.y_long, 2); 
                });
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

        private double lastLat = 0;
        private double lastLon = 0;
        private readonly Geodesic _geo = Geodesic.WGS84;
        private bool _moveMapToTrackedObject = true;

        private void ShowTrackedObjectLocation(double lat, double lon, double minDelta)
        {
            try
            {
                if (minDelta > _geo.Inverse(lastLat, lastLon, lat, lon).Distance)
                    return;

                lastLat = lat;
                lastLon = lon;

                var position = new Position(lat, lon); // Latitude, Longitude
                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = "Dr. Elkman",
                    Address = $"Time:{DateTime.UtcNow.ToString()}"
                };
                _map.Pins.Add(pin);

                if (_moveMapToTrackedObject)
                {
                    _map.MoveToRegion(
                        MapSpan.FromCenterAndRadius(
                            position, Distance.FromMiles(1)));

                    _moveMapToTrackedObject = false;
                }
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

        private void GotAzureDeviceMessageCallback(byte[] messagepayload)
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

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void BuildCustomPins()
        {
            var pin = new Waypoint
            {
                Type = PinType.Place,
                Position = new Position(37.79752, -122.40183),
                Label = "Xamarin San Francisco Office",
                Address = "394 Pacific Ave, San Francisco CA",
                Id = "Xamarin",
                Url = "http://xamarin.com/about/"
            };

            _map.Waypoints = new List<Waypoint> { pin };
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void TestDropCustomPin()
        {
            _map.Pins.Add(_map.Waypoints.First());
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(37.79752, -122.40183), Distance.FromMiles(1.0)));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void TestDrawPolyline()
        {
            _map.RouteCoordinates = new List<Position>()
            {
                new Position(37.797534, -122.401827),
                new Position(37.797510, -122.402060),
                new Position(37.790269, -122.400589),
                new Position(37.790265, -122.400474),
                new Position(37.790228, -122.400391),
                new Position(37.790126, -122.400360),
                new Position(37.789250, -122.401451),
                new Position(37.788440, -122.400396),
                new Position(37.787999, -122.399780),
                new Position(37.786736, -122.398202),
                new Position(37.786345, -122.397722),
                new Position(37.785983, -122.397295),
                new Position(37.785559, -122.396728),
                new Position(37.780624, -122.390541),
                new Position(37.777113, -122.394983),
                new Position(37.776831, -122.394627)
            };

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () => _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(37.79752, -122.40183), Distance.FromMiles(1.0))));
        }
    }
}