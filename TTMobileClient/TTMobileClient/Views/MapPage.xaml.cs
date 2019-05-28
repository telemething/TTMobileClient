using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FaaUasLib;
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
    public enum MissionStateEnum { Unknown, None, Sending, Sent, Starting, Underway, Failed, Completed }
    public enum LandedStateEnum { Unknown, Grounded, Liftoff, Flying, Landing, Failed }
    public enum ConnectionStateEnum { Unknown, NotConnected, Connecting, Connected, Failed, Dropped }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        string TestUri = "ws://192.168.1.30:9090";
        double TestLat = 47.6062;
        double TestLong = -122.3321;

        #region private 

        private IRosClient _rosClient = null;

        private CustomMap _map;
        private Plugin.Geolocator.Abstractions.Position _myPosition;
        private Timer _heartbeatTimer;
        private MissionStatus _missionStatus;
        private MissionCtrl _missionCtrl;
        // missing AzureIotLib
        //private AzureIotDevice _azureIotDevice;

        private MissionStateEnum _missionState = MissionStateEnum.Unknown;
        private LandedStateEnum _landedState = LandedStateEnum.Unknown;
        private ConnectionStateEnum _connectionState = ConnectionStateEnum.Unknown;

        private MissionStateEnum MissionState
        {
            set
            { _missionState = value; UpdateMessageBox(); }
            get => _missionState;
        }

        private LandedStateEnum LandedState
        {
            set
            { _landedState = value; UpdateMessageBox(); }
            get => _landedState;
        }

        private ConnectionStateEnum ConnectionState
        {
            set
            { _connectionState = value; UpdateMessageBox(); }
            get => _connectionState;
        }

        private Label _MessageLabel = new Label();

        private Label _StatusLatLabel;
        private Label _StatusLongLabel;
        private Label _StatusAltLabel;
        private Label _StatusLandedLabel;

        private StackLayout _configStack;
        private StackLayout _planStack;
        private StackLayout _missionStack;
        private StackLayout _telemStack;

        // Position tracking stuff
        private double _lastLat = 0;
        private double _lastLon = 0;
        private readonly Geodesic _geo = Geodesic.WGS84;
        private bool _moveMapToTrackedObject = true;
        private TrackedObject _singleTrackedObject;

        #endregion

        //*********************************************************************
        ///
        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        //*********************************************************************

        public MapPage()
        {
            _missionCtrl = new MissionCtrl();
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
                        new Position(TestLat, TestLong), Distance.FromMiles(0.3)))
                {
                    IsShowingUser = true,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

                _map.OnMapClick += OnMapClick;

                //****************



                FaaUasLib.FaaUas faa = new FaaUas();
                faa.getData();

                _map.faaFascilityMap = faa.fascilityMap;

                /*_map.ShapeCoordinates = new List<Position>();

                _map.ShapeCoordinates.Add(new Position(47.0166766905289, -123.000014237528));
                _map.ShapeCoordinates.Add(new Position(47.0166766925289, -122.983347564466));
                _map.ShapeCoordinates.Add(new Position(47.0000100214669, -122.983347560466));
                _map.ShapeCoordinates.Add(new Position(47.0000100194669, -123.000014234528));

                _map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(37.79752, -122.40183), Distance.FromMiles(0.1)));*/

                //****************


                // Map style buttons
                var mapStyleStreetButton = new Button { Text = "Street", BackgroundColor = Color.Gray};
                var mapStyleHybridButton = new Button { Text = "Hybrid", BackgroundColor = Color.Gray };
                var mapStyleSatelliteButton = new Button { Text = "Satellite", BackgroundColor = Color.Gray };

                mapStyleStreetButton.Clicked += HandleClicked;
                mapStyleHybridButton.Clicked += HandleClicked;
                mapStyleSatelliteButton.Clicked += HandleClicked;

                // Mission buttons
                var missionConnectButton = new Button { Text = "Connect", BackgroundColor = Color.Gray };
                var missionSendMissionButon = new Button { Text = "Send Mission", BackgroundColor = Color.Gray };
                var missionStartMissionButton = new Button { Text = "Start Mission", BackgroundColor = Color.Gray };
                var missionEndMissionButton = new Button { Text = "End Mission", BackgroundColor = Color.Gray };
                var missionClearMissionButton = new Button { Text = "Clear Mission", BackgroundColor = Color.Gray };

                missionConnectButton.Clicked += HandleClicked;
                missionSendMissionButon.Clicked += HandleClicked;
                missionStartMissionButton.Clicked += HandleClicked;
                missionEndMissionButton.Clicked += HandleClicked;
                missionClearMissionButton.Clicked += HandleClicked;

                // Coordinates Display
                _StatusLatLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusLongLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusAltLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusLandedLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };

                // Popup stacks
                _configStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { mapStyleStreetButton, mapStyleHybridButton,
                        mapStyleSatelliteButton }
                };

                _planStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { _missionCtrl.viewCtrl }
                };

                _missionStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { missionConnectButton, missionSendMissionButon,
                        missionStartMissionButton, missionEndMissionButton,
                        missionClearMissionButton }
                };

                // Telemetry Stack
                _telemStack = new StackLayout
                {
                    Spacing = 10,
                    //HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    Orientation = StackOrientation.Vertical,
                    Children = { _StatusLatLabel, _StatusLongLabel, _StatusAltLabel }
                };

                // Bottom Tray

                _MessageLabel = new Label
                {
                    Text = "Hi",
                    //TextColor = Color.Red,
                    //FontSize = 30,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                ImageButton missionButton = new ImageButton
                {
                    Source = "mission.png",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };
                ImageButton planButton = new ImageButton
                {
                    Source = "plan.png",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                };
                ImageButton configButton = new ImageButton
                {
                    Source = "view.png",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                missionButton.Clicked += (sender, args) =>
                { _configStack.IsVisible = false; _missionStack.IsVisible = !_missionStack.IsVisible; };
                planButton.Clicked += (sender, args) =>
                { /*_configStack.IsVisible = false; _missionStack.IsVisible = false;*/ _planStack.IsVisible = !_planStack.IsVisible; };
                configButton.Clicked += (sender, args) => 
                { _missionStack.IsVisible = false; _configStack.IsVisible = !_configStack.IsVisible; };

                var bottomTray = new StackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Fill,
                    Orientation = StackOrientation.Horizontal,
                    Children = { missionButton, planButton, _MessageLabel, configButton }
                };

                var MapPlanstack = new StackLayout { Spacing = 0, Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.FillAndExpand };
                MapPlanstack.Children.Add(_map);
                MapPlanstack.Children.Add(_planStack);

                var stack = new StackLayout { Spacing = 0 };
                stack.Children.Add(MapPlanstack);
                stack.Children.Add(_configStack);
                stack.Children.Add(_missionStack);

                stack.Children.Add(bottomTray);

                var layout = new AbsoluteLayout();

                layout.Children.Add(stack);
                AbsoluteLayout.SetLayoutBounds(stack, 
                    new Rectangle(0, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(stack, 
                    AbsoluteLayoutFlags.All);

                layout.Children.Add(_telemStack);
                AbsoluteLayout.SetLayoutBounds(_telemStack, 
                    new Rectangle(.01, .9, 250, 200));
                AbsoluteLayout.SetLayoutFlags(_telemStack, 
                    AbsoluteLayoutFlags.PositionProportional);

                /*layout.Children.Add(_configStack);
                AbsoluteLayout.SetLayoutBounds(_configStack, 
                    new Rectangle(.5, .9, 200, 40));
                AbsoluteLayout.SetLayoutFlags(_configStack, 
                    AbsoluteLayoutFlags.PositionProportional);

                layout.Children.Add(_missionStack);
                AbsoluteLayout.SetLayoutBounds(_missionStack, 
                    new Rectangle(.5, .9, 200, 40));
                AbsoluteLayout.SetLayoutFlags(_missionStack, 
                    AbsoluteLayoutFlags.PositionProportional);*/

                Content = layout;

                //ShowCurrentPositionOnMap();

                //TestDropCustomPin();

                //TestDrawPolyline();

                UpdateMessageBox();

                return true;
            }
            catch (System.Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + ex.Message, "Ok");
                return false;
            }
        }

        private async Task<bool> ShowMapy()
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
                _StatusLatLabel = new Label { Text = "---" };
                _StatusLongLabel = new Label { Text = "---" };
                _StatusAltLabel = new Label { Text = "---" };
                _StatusLandedLabel = new Label { Text = "---" };

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
                    Children = { _StatusLandedLabel, _StatusLatLabel, _StatusLongLabel, _StatusAltLabel }
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
        ///
        //*********************************************************************

        private void UpdateMessageBox()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                _MessageLabel.Text = 
                    $"Connection: {this.ConnectionState.ToString()}, " +
                    $"Mission: {this.MissionState.ToString()}, " +
                    $"UAV: {this.LandedState.ToString()}");
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

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        ///
        //*********************************************************************

        private void AddWaypoint(double lat, double lon, double alt)
        {
            _map.AddWaypoint(new Waypoint
            {
                Type = PinType.Place,
                Position = new Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://www.telemething.com/"
            });
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        private TrackedObject AddTrackedObject(string uniqueId, 
            double lat, double lon, double alt)
        {
            TrackedObject to = new TrackedObject()
            {
                Type = PinType.Generic,
                Position = new Position(lat, lon),
                Label = "UAV",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "UAV",
                Url = "http://www.telemething.com/",
                UniqueId = uniqueId
            };

            _map.AddTrackedObject(to);

            return to;
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
            _missionStack.IsVisible = false;
            _configStack.IsVisible = false;

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
                case "Send Mission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(SendMission);
                    break;
                case "Start Mission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(StartMission);
                    break;
                case "End Mission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(EndMission);
                    break;
                case "Clear Mission":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(ClearMission);
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
                await App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + ex.Message, "Ok");
                Console.WriteLine(ex);
                return;
            }

            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(pos.Latitude, pos.Longitude), Distance.FromMiles(0.3)));
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
                            await App.Current.MainPage.DisplayAlert(
                                "Need a permission", $"Required: " +
                                                     $"{permission.ToString()}", "OK");
                        }

                        var results = await CrossPermissions.Current.RequestPermissionsAsync(permission);
                        //Best practice to always check that the key exists
                        if (results.ContainsKey(permission))
                            status = results[permission];

                        if (!(status == PermissionStatus.Granted || status == PermissionStatus.Unknown))
                        {
                            await App.Current.MainPage.DisplayAlert(
                                "Permission Denied", 
                                "Can not continue, try again.", "OK");
                            permissionsGranted = false;
                            break;
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + ex.Message, "Ok");
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

            this.ConnectionState = ConnectionStateEnum.Failed;

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
            this.ConnectionState = ConnectionStateEnum.Connected;
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
            this.MissionState = MissionStateEnum.Sending;

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
                    if (resp.success)
                    {
                        this.MissionState = MissionStateEnum.Sent;
                    }
                    else
                    {
                        this.MissionState = MissionStateEnum.Failed;

                        Xamarin.Forms.Device.BeginInvokeOnMainThread(
                            () => App.Current.MainPage.DisplayAlert(
                                "Error", "Unable to send waypoints", "Ok"));
                    }
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
            this.MissionState = MissionStateEnum.Starting;

            ConnectToMav();
            _rosClient.CallService<StartMission.StartMissionReqResp>(new StartMission(),
                resp => {
                    if (resp.success)
                    {
                        this.MissionState = MissionStateEnum.Starting;
                    }
                    else
                    {
                        this.MissionState = MissionStateEnum.Failed;

                        Xamarin.Forms.Device.BeginInvokeOnMainThread(
                            () => App.Current.MainPage.DisplayAlert(
                                "Error", "Unable to start mission", "Ok"));
                    }
                });
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void EndMission()
        {
            //this.MissionState = MissionStateEnum.Underway;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void ClearMission()
        {
            // we cant clear a mission which is underway
            if (this.MissionState == MissionStateEnum.Starting | 
                this.MissionState == MissionStateEnum.Underway)
                return;

            //remove all the waypoints

            _map.RemoveWaypoints();

            this.MissionState = MissionStateEnum.None;
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
            if(this.ConnectionState != ConnectionStateEnum.Connected)
                this.ConnectionState = ConnectionStateEnum.Connecting;

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
        /// <param name="lsIn"></param>
        ///
        //*********************************************************************

        private void SetLandedState(string lsIn)
        {
            if (0 == lsIn.Length)
                return;

            switch (lsIn)
            {
                case "LANDED_STATE_ON_GROUND":
                    LandedState = LandedStateEnum.Grounded;
                    if (MissionState == MissionStateEnum.Underway)
                        MissionState = MissionStateEnum.Completed;
                    break;
                case "LANDED_STATE_IN_AIR":
                    LandedState = LandedStateEnum.Flying;
                    MissionState = MissionStateEnum.Underway;
                    break;
                case "LANDED_STATE_LANDING":
                    LandedState = LandedStateEnum.Landing;
                    MissionState = MissionStateEnum.Underway;
                    break;
                case "LANDED_STATE_TAKEOFF":
                    LandedState = LandedStateEnum.Liftoff;
                    MissionState = MissionStateEnum.Underway;
                    break;
                default:
                    LandedState = LandedStateEnum.Unknown;
                    break;
            }
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
                    _StatusLatLabel.Text = $"Lat: {missionStatus.x_lat}";
                    _StatusLongLabel.Text = $"Lon: {missionStatus.y_long}";
                    _StatusAltLabel.Text = $"Alt: {Math.Round(missionStatus.z_alt, 2)}";

                    SetLandedState(missionStatus.landed_state);

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
        /// <param name="minDelta"></param>
        ///
        //*********************************************************************

        private void ShowTrackedObjectLocation(double lat, double lon, double minDelta)
        {
            try
            {
                if (minDelta > _geo.Inverse(
                        _lastLat, _lastLon, lat, lon).Distance)
                    return;

                _lastLat = lat;
                _lastLon = lon;

                var position = new Position(lat, lon); // Latitude, Longitude

                if (null == _singleTrackedObject)
                    _singleTrackedObject = AddTrackedObject(
                        "singleTrackedObject", lat, lon, 0);
                else
                {
                    _singleTrackedObject.Position = position;
                    _map.Change = new ChangeHappened(_singleTrackedObject, 
                        ChangeHappened.ChangeTypeEnum.Changed);
                }

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

        #region Tests

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
                new Position(37.79752, -122.40183), 
                Distance.FromMiles(1.0)));
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

        #endregion
    }
}