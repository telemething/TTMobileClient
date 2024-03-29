﻿using Plugin.Permissions;
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
using TThingComLib;
using TThingComLib.Messages;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using StartMission = RosClientLib.StartMission;
using TileServerLib;


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
    //*************************************************************************
    /// <summary>
    /// A status bar, shows status of devices and things
    /// </summary>
    //*************************************************************************

    public class StatusBar : StackLayout
    {
        MapPage _parentPage = null;

        private enum RosConnectionStatusEnum { unknown, gotAdvertisedUrl, 
            connecting, connected, disconected, error }
        private RosConnectionStatusEnum _rosConnectionStatus = 
            RosConnectionStatusEnum.unknown;
        string _rosBridgeServerUrlAdvertised = "";

        private enum PerifConnectionStatusEnum { unknown, connecting, 
            connected, disconected, error }
        private PerifConnectionStatusEnum _perifConnectionStatus = 
            PerifConnectionStatusEnum.unknown;

        RosClient.ConnectEventArgs _rosClientConnectEventArgs = null;
        TTMobileClient.Services.ApiService.ApiEventArgs 
            _perifConnectionEventArgs = null;

        //Button perifConnectionButton = new Button
        //{ Text = "Perif", BackgroundColor = Color.LightGray };

        //Button rosConnectionButton = new Button
        //{ Text = "ROS", BackgroundColor = Color.LightGray };

        ImageButton perifConnectionButton = new ImageButton
        {
            Source = "HololensConnection.png",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Color.LightGray
        };

        ImageButton rosConnectionButton = new ImageButton
        {
            Source = "DroneConnection.png",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Color.LightGray
        };


        public class PerfConnectionUserActions
        {
            public const string Connect = "Connect";
            public const string Disconnect = "Disconnect";
            public const string Reconnect = "Reconnect";
            public const string Block = "Block";
            public const string Unknown = "Unknown";
            public const string Cancel = "Cancel";
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************
        public StatusBar(MapPage parentPage)
        {
            Spacing = 10;
            HorizontalOptions = LayoutOptions.Fill;
            Orientation = StackOrientation.Horizontal;
            Children.Add(perifConnectionButton);
            Children.Add(rosConnectionButton);
            _parentPage = parentPage;

            perifConnectionButton.Clicked += PerifConnectionButton_Clicked;
            rosConnectionButton.Clicked += RosConnectionButton_Clicked;

            if (null != TTMobileClient.Services.ApiService.Singleton)
                TTMobileClient.Services.ApiService.Singleton.ClientConnection +=
                        PerifConnectionEventHandler;

            AdvertiseServices.Singleton.ServiceAdvertismentReceivedEvent +=
                ServiceAdvertismentReceivedEvent;

            RosClient.ConnectionEvent += RosConnectionEventHandler;
        }


        //*********************************************************************
        /// <summary>
        /// Listen for service avilablity announcements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void ServiceAdvertismentReceivedEvent(
            object sender, AdvertiseServices.ServiceAdvertismentReceivedEventArgs e)
        {
            foreach (var networkService in e.NetworkServices)
            {
                switch (networkService.ServiceType)
                {
                    case ServiceTypeEnum.RosBridge:
                        if (_rosConnectionStatus == RosConnectionStatusEnum.unknown)
                        {
                            _rosConnectionStatus = RosConnectionStatusEnum.gotAdvertisedUrl;
                            _rosBridgeServerUrlAdvertised = networkService.URL;
                            _parentPage._rosBridgeUri = networkService.URL;
                            Device.BeginInvokeOnMainThread( () =>
                                rosConnectionButton.BackgroundColor = Color.LightBlue);
                            if(null != _parentPage)
                                _parentPage.RosBridgeUri = networkService.URL;
                        }
                        break;
                }
            }
        }

        //*********************************************************************
        /// <summary>
        /// Displays information about the ApiServer connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private async void PerifConnectionButton_Clicked(object sender, EventArgs e)
        {
            string deviceName = "---";
            string action = "unkown";

            if (null != _perifConnectionEventArgs)
                deviceName = _perifConnectionEventArgs.remoteDeviceName;

            switch (_perifConnectionStatus)
            {
                case PerifConnectionStatusEnum.unknown:
                    action = await _parentPage?.DisplayActionSheet(
                        PerfConnectionUserActions.Unknown,
                        PerfConnectionUserActions.Cancel, null);
                    break;
                case PerifConnectionStatusEnum.connected:
                    action = await _parentPage?.DisplayActionSheet(
                        "Connected: " + deviceName,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Disconnect,
                        PerfConnectionUserActions.Reconnect,
                        PerfConnectionUserActions.Block);
                    break;
                case PerifConnectionStatusEnum.disconected:
                    action = await _parentPage?.DisplayActionSheet(
                        "Disconnected: " + deviceName,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Connect,
                        PerfConnectionUserActions.Block);
                    break;
                case PerifConnectionStatusEnum.error:
                    action = await _parentPage?.DisplayActionSheet(
                        "Error: " + deviceName,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Connect,
                        PerfConnectionUserActions.Block);
                    break;
            }

            switch (action)
            {
                case PerfConnectionUserActions.Block:
                    //TODO
                    break;
                case PerfConnectionUserActions.Connect:
                    //TODO
                    break;
                case PerfConnectionUserActions.Disconnect:
                    //TODO
                    break;
                case PerfConnectionUserActions.Reconnect:
                    //TODO
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// Called by the ApiSerivce on connection events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void PerifConnectionEventHandler(object sender,
            TTMobileClient.Services.ApiService.ApiEventArgs e)
        {
            switch (e.EventType)
            {
                case Services.ApiService.ApiEventArgs.EventTypeEnum.connection:
                    _perifConnectionEventArgs = e;
                    _perifConnectionStatus = PerifConnectionStatusEnum.connected;
                    perifConnectionButton.BackgroundColor = Color.Green;
                    break;
                case Services.ApiService.ApiEventArgs.EventTypeEnum.disconnection:
                    _perifConnectionEventArgs = e;
                    _perifConnectionStatus = PerifConnectionStatusEnum.disconected;
                    perifConnectionButton.BackgroundColor = Color.LightGray;
                    break;
                case Services.ApiService.ApiEventArgs.EventTypeEnum.failure:
                    _perifConnectionEventArgs = e;
                    _perifConnectionStatus = PerifConnectionStatusEnum.error;
                    perifConnectionButton.BackgroundColor = Color.Red;
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// Displays information about the RosClient connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private async void RosConnectionButton_Clicked(object sender, EventArgs e)
        {
            string message = "Not Connected";
            string state = "Not Connected";
            string action = "";

            if (null != _rosClientConnectEventArgs)
            {
                message = _rosClientConnectEventArgs.RemoteDeviceName +
                    " : " + _rosClientConnectEventArgs.Message;
                state = _rosClientConnectEventArgs.EventType.ToString();
            }

            switch (_rosConnectionStatus)
            {
                case RosConnectionStatusEnum.unknown:
                    action = await _parentPage?.DisplayActionSheet(
                        PerfConnectionUserActions.Unknown,
                        PerfConnectionUserActions.Cancel, null);
                    break;
                case RosConnectionStatusEnum.gotAdvertisedUrl:
                    action = await _parentPage?.DisplayActionSheet(
                        "Got URL: " + _rosBridgeServerUrlAdvertised,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Connect,
                        PerfConnectionUserActions.Block);
                    break;
                case RosConnectionStatusEnum.connecting:
                    action = await _parentPage?.DisplayActionSheet(
                        "Connecting: " + _rosBridgeServerUrlAdvertised,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Disconnect,
                        PerfConnectionUserActions.Reconnect,
                        PerfConnectionUserActions.Block);
                    break;
                case RosConnectionStatusEnum.connected:
                    action = await _parentPage?.DisplayActionSheet(
                       "Connected: " + _rosBridgeServerUrlAdvertised,
                       PerfConnectionUserActions.Cancel, null,
                       PerfConnectionUserActions.Disconnect,
                       PerfConnectionUserActions.Reconnect,
                       PerfConnectionUserActions.Block);
                    break;
                case RosConnectionStatusEnum.disconected:
                    action = await _parentPage?.DisplayActionSheet(
                        "Disconnected: " + _rosBridgeServerUrlAdvertised,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Connect,
                        PerfConnectionUserActions.Block);
                    break;
                case RosConnectionStatusEnum.error:
                    action = await _parentPage?.DisplayActionSheet(
                        "Error: " + _rosBridgeServerUrlAdvertised,
                        PerfConnectionUserActions.Cancel, null,
                        PerfConnectionUserActions.Connect,
                        PerfConnectionUserActions.Block);
                    break;
            }

            switch (action)
            {
                case PerfConnectionUserActions.Block:
                    //TODO
                    break;
                case PerfConnectionUserActions.Connect:
                    if (null != _parentPage)
                        Device.BeginInvokeOnMainThread(
                            () =>_parentPage.StartTelemetry(_rosBridgeServerUrlAdvertised));
                    break;
                case PerfConnectionUserActions.Disconnect:
                    //TODO
                    break;
                case PerfConnectionUserActions.Reconnect:
                    //TODO
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// Called by the RosClient on connection events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void RosConnectionEventHandler(object sender,
            RosClient.ConnectEventArgs e)
        {
            _rosClientConnectEventArgs = e;

            switch (e.EventType)
            {
                case RosClient.ConnectEventArgs.EventTypeEnum.connecting:
                    _rosConnectionStatus = RosConnectionStatusEnum.connecting;
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                       rosConnectionButton.BackgroundColor = Color.Yellow);
                    break;
                case RosClient.ConnectEventArgs.EventTypeEnum.connected:
                    _rosConnectionStatus = RosConnectionStatusEnum.connected;
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        rosConnectionButton.BackgroundColor = Color.Green);
                    break;
                case RosClient.ConnectEventArgs.EventTypeEnum.disconnected:
                    _rosConnectionStatus = RosConnectionStatusEnum.disconected;
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        rosConnectionButton.BackgroundColor = Color.LightGray);
                    break;
                case RosClient.ConnectEventArgs.EventTypeEnum.failure:
                    _rosConnectionStatus = RosConnectionStatusEnum.error;
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        rosConnectionButton.BackgroundColor = Color.Red);
                    break;
            }
        }
    }

    //*************************************************************************
    /// <summary>
    /// A command bar, has buttons that execute commands
    /// </summary>
    //*************************************************************************

    public class CommandBar : StackLayout
    {
        Button remoteConnectedButton = new Button
        { Text = "Remote", BackgroundColor = Color.LightGray };

        private MissionCtrl _missionCtrl;

        Label _MessageLabel = new Label
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
            Source = "cog.png",
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };
        ImageButton mapStyleButton = new ImageButton
        {
            Source = "view.png",
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };

        // Map style buttons
        Button mapStyleStreetButton = new Button { Text = "Street", BackgroundColor = Color.Gray };
        Button mapStyleHybridButton = new Button { Text = "Hybrid", BackgroundColor = Color.Gray };
        Button mapStyleSatelliteButton = new Button { Text = "Satellite", BackgroundColor = Color.Gray };

        // Config buttons
        Button configLockButton = new Button { Text = "Lock Cursor", BackgroundColor = Color.Gray };
        Button configAddWaypointsButton = new Button { Text = "Add Waypoints", BackgroundColor = Color.Gray };
        Button configSelfLocationButton = new Button { Text = "Set Self Location", BackgroundColor = Color.Gray };
        Button configDroneLocationButton = new Button { Text = "Set Drone Location", BackgroundColor = Color.Gray };

        // Mission buttons
        Button missionConnectButton = new Button { Text = "Connect", BackgroundColor = Color.Gray };
        Button missionSendMissionButon = new Button { Text = "Send Mission", BackgroundColor = Color.Gray };
        Button missionStartMissionButton = new Button { Text = "Start Mission", BackgroundColor = Color.Gray };
        Button missionEndMissionButton = new Button { Text = "End Mission", BackgroundColor = Color.Gray };
        Button missionClearMissionButton = new Button { Text = "Clear Mission", BackgroundColor = Color.Gray };

        public StackLayout _mapStyleStack { set; get; }

        public StackLayout _configStack { set; get; }

        public StackLayout _planStack { set; get; }

        public StackLayout _missionStack { set; get; }

        public CommandBar()
        {
            _missionCtrl = new MissionCtrl();   //TODO * figure out what this does, I forgot

            Spacing = 10;
            HorizontalOptions = LayoutOptions.Fill;
            Orientation = StackOrientation.Horizontal;
            Children.Add(missionButton);
            Children.Add(planButton);
            Children.Add(_MessageLabel);
            Children.Add(mapStyleButton);
            Children.Add(configButton);

            missionButton.Clicked += (sender, args) =>
            { _mapStyleStack.IsVisible = false; _missionStack.IsVisible = !_missionStack.IsVisible; _configStack.IsVisible = false; };
            planButton.Clicked += (sender, args) =>
            { /*_configStack.IsVisible = false; _missionStack.IsVisible = false;*/ _planStack.IsVisible = !_planStack.IsVisible; };
            mapStyleButton.Clicked += (sender, args) =>
            { _missionStack.IsVisible = false; _mapStyleStack.IsVisible = !_mapStyleStack.IsVisible; _configStack.IsVisible = false; };
            configButton.Clicked += (sender, args) =>
            { _mapStyleStack.IsVisible = false; _missionStack.IsVisible = false; _configStack.IsVisible = !_configStack.IsVisible; };

            /*
            // Map style buttons
            mapStyleStreetButton.Clicked += HandleClicked;
            mapStyleHybridButton.Clicked += HandleClicked;
            mapStyleSatelliteButton.Clicked += HandleClicked;

            // Config buttons
            configSelfLocationButton.Clicked += HandleClicked;
            configDroneLocationButton.Clicked += HandleClicked;

            // Mission buttons
            missionConnectButton.Clicked += HandleClicked;
            missionSendMissionButon.Clicked += HandleClicked;
            missionStartMissionButton.Clicked += HandleClicked;
            missionEndMissionButton.Clicked += HandleClicked;
            missionClearMissionButton.Clicked += HandleClicked;
            */

            _mapStyleStack = new StackLayout
            {
                IsVisible = true,
                Spacing = 10,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { mapStyleStreetButton, mapStyleHybridButton,
                        mapStyleSatelliteButton }
            };

            _configStack = new StackLayout
            {
                IsVisible = false,
                Spacing = 10,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { configLockButton, configAddWaypointsButton, configSelfLocationButton, configDroneLocationButton }
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
        }
    }


    public enum MissionStateEnum { Unknown, None, Sending, Sent, Starting, Underway, Failed, Completed }
    public enum LandedStateEnum { Unknown, Grounded, Liftoff, Flying, Landing, Failed }
    public enum ConnectionStateEnum { Unknown, NotConnected, Connecting, Connected, Failed, Dropped }
    public enum MapClickModeEnum { Unknown, SetSelfPosition, SetDronePosition, AddWaypoint, PrefetchSetCenter, Locked }

    //*************************************************************************
    /// <summary>
    /// 
    /// </summary>
    //*************************************************************************
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        #region private 
        private class PrefetchStatus
        {
            public int TotalTileCount = 0;
            public int TotalFetchedOrFailed = 0;
            public int ImageSuccessCount = 0;
            public int ImageFailureCount = 0;
            public int ElevationSuccessCount = 0;
            public int ElevationFailureCount = 0;
            public int MapSuccessCount = 0;
            public int MapFailureCount = 0;
        }

        private PrefetchStatus _prefetchStatus = new PrefetchStatus();
        private WorldCoordinate _prefetchCenter = null;
        private int _prefetchEdgeCount = AppSettings.PrefetchEdgeCount;
        private int _prefetchZoom = AppSettings.PrefetchZoom;

        public string _rosBridgeUri = AppSettings.DefaultRobotRosbridgeUrl;
        private double TestLat = AppSettings.DefaultGeoCoordsLat;
        private double TestLong = AppSettings.DefaultGeoCoordsLon;

        private bool _initialized = false;
        private int _heartbeatPeriodSeconds = AppSettings.HeartbeatPeriodSeconds;
        private int _selfTelemPeriodSeconds = AppSettings.SelfTelemPeriodSeconds;
        string _udpBroadcastIP = AppSettings.UdpBroadcastIP;
        int _thingTelemPort = AppSettings.ThingTelemPort;

        private bool _sendSelfTelem = true;  //*** TODO * Make this a config item
        private bool _runWacLoopbackTest = false;

        private TThingComLib.Repeater _telemetryRepeater = new TThingComLib.Repeater();
        private TThingComLib.Listener _telemetryUdpListener = null;

        private IRosClient _rosClient = null;

        WebApiLib.WebApiClient wac;
        private CustomMap _map;
        private Plugin.Geolocator.Abstractions.Position _myPosition;
        private Timer _heartbeatTimer;
        private Timer _selfTelemTimer;
        private MissionStatus _missionStatus;
        private MissionCtrl _missionCtrl;
        private MapClickModeEnum _mapClickMode = MapClickModeEnum.Unknown;
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
        private Label _ToolsLabel = new Label();

        private ProgressBar _progressBar = new ProgressBar 
            { ProgressColor = Color.Yellow, Progress = 0.0f };
        private uint _progressBarAnimationTime = 1000;

        private Label _StatusLatLabel;
        private Label _StatusLongLabel;
        private Label _StatusAltLabel;
        private Label _StatusLandedLabel;

        private StackLayout _toolsStack;
        private StackLayout _mapStyleStack;
        private StackLayout _mouseModeStack;
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

        #region public

        public string RosBridgeUri { set; get; }

        #endregion

        //*********************************************************************
        /// <summary>
        /// Constructor
        /// </summary>
        //*********************************************************************

        public MapPage()
        {
            _missionCtrl = new MissionCtrl();
            InitializeComponent();
        }

        //*********************************************************************
        /// <summary>
        /// The page is about to appear
        /// </summary>
        //*********************************************************************

        protected override void OnAppearing()
        {
            if (!_initialized)
            {
                Init();
                _initialized = true;
            }
        }

        //*********************************************************************
        /// <summary>
        /// The page is about to disappear
        /// </summary>
        //*********************************************************************

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        //*********************************************************************
        /// <summary>
        /// Initialize everything
        /// </summary>
        //*********************************************************************

        private void Init()
        {
            //ShowDeviceInfo();

            base.OnAppearing();
            ShowMap();
            StartRepeater(); //*** TODO * We don't always want to run this, make it a config item
            StartListener(); //*** TODO * We don't always want to run this, make it a config item

            //Xamarin.Forms.Device.BeginInvokeOnMainThread(ConnectToIotHub);
            //Xamarin.Forms.Device.BeginInvokeOnMainThread(StartTelemetry);

            StartHeartbeatTimer();
        }

        //*********************************************************************
        /// <summary>
        /// Show Device Info
        /// </summary>
        //*********************************************************************

        private void ShowDeviceInfo()
        {
            var ff1 = Xamarin.Essentials.DeviceInfo.DeviceType;
            var ff2 = Xamarin.Essentials.DeviceInfo.Idiom;
            var ff3 = Xamarin.Essentials.DeviceInfo.Manufacturer;
            var ff4 = Xamarin.Essentials.DeviceInfo.Model;
            var ff5 = Xamarin.Essentials.DeviceInfo.Name;
            var ff6 = Xamarin.Essentials.DeviceInfo.Platform;
            var ff7 = Xamarin.Essentials.DeviceInfo.Version;
            var ff8 = Xamarin.Essentials.DeviceInfo.VersionString;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //*********************************************************************
        private async Task<bool> StartListener()
        {
            try
            {
                _telemetryUdpListener = new Listener(45679, GotMessageCallback);
                //_telemetryUdpListener.ServiceAdvertismentReceivedEvent +=
                //    ServiceAdvertismentReceivedEvent;
                _telemetryUdpListener.ServiceAdvertismentReceivedEvent +=
                    AdvertiseServices.Singleton.ServiceAdvertismentReceivedEventHandler;
                _telemetryUdpListener.Connect();

                if (_runWacLoopbackTest)
                {
                    wac = new WebApiLib.WebApiClient();
                    wac.Connect("ws://localhost:8877/wsapi");
                }

                return true;
            }
            catch (Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert(
                    "StartListener() Exception:", "Error: " + ex.Message, "Ok");
                return false;
            }

            return true;
        }

        string _rosBridgeServerUrlAdvertised = null;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************
        private void ServiceAdvertismentReceivedEvent(object sender,
            Listener.ServiceAdvertismentReceivedEventArgs e)
        {
            foreach (var networkService in e.NetworkServices)
            {
                switch (networkService.ServiceType)
                {
                    case ServiceTypeEnum.RosBridge:
                        _rosBridgeServerUrlAdvertised = networkService.URL;
                        break;
                }
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        //*********************************************************************
        private void ProcessMessageCommand(TThingComLib.Messages.Command command)
        {
            switch (command.CommandId)
            {
                case CommandIdEnum.StartMission:
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(StartMission);
                    break;
                case CommandIdEnum.StopDrone:
                    break;
                case CommandIdEnum.ReturnHome:
                    break;
                case CommandIdEnum.LandDrone:
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        //*********************************************************************
        private void GotMessageCallback(Message message)
        {
            if (message?.Commands != null)
                foreach (var command in message.Commands)
                    ProcessMessageCommand(command);

            //if (null != message.Coord)
            //if (null != message.Orient)
            //if (null != message.Gimbal)
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //*********************************************************************

        private async Task<bool> StartRepeater()
        {
            try
            {
                _telemetryRepeater.AddTransport(
                    TThingComLib.Repeater.TransportEnum.UDP,
                    TThingComLib.Repeater.DialectEnum.ThingTelem,
                    _udpBroadcastIP, _thingTelemPort, 500);
            }
            catch (Exception ex)
            {
                //logger.DebugLogError("Fatal Error on permissions: " + ex.Message);
                await App.Current.MainPage.DisplayAlert(
                    "StartRepeater() Exception:", "Error: " + ex.Message, "Ok");
                return false;
            }

            return true;
        }

        //*********************************************************************
        /// <summary>
        /// Show the map, after getting permission from the OS to do so
        /// </summary>
        //*********************************************************************

        private async Task<bool> ShowMap()
        {
            //if (!await GetPermissions(new List<Permission>()
            //    { Permission.Location, Permission.LocationWhenInUse }))
            //    return false;

            if (!await GetPermissions())
                return false;

            try
            {
                // create map
                _map = new CustomMap(
                    MapSpan.FromCenterAndRadius(
                        new Position(TestLat, TestLong), Distance.FromMiles(0.3)))
                {
                    IsShowingUser = false,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

                _map.OnMapClick += OnMapClick;
                _map.OnMapReady += MapOnOnMapReady;

                //****************


                // below implemented on uwp, not iOS or Android
                //FaaUasLib.FaaUas faa = new FaaUas();
                //faa.getData();

                //_map.faaFascilityMap = faa.fascilityMap;
                // above

                /*_map.ShapeCoordinates = new List<Position>();

                _map.ShapeCoordinates.Add(new Position(47.0166766905289, -123.000014237528));
                _map.ShapeCoordinates.Add(new Position(47.0166766925289, -122.983347564466));
                _map.ShapeCoordinates.Add(new Position(47.0000100214669, -122.983347560466));
                _map.ShapeCoordinates.Add(new Position(47.0000100194669, -123.000014234528));

                _map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(37.79752, -122.40183), Distance.FromMiles(0.1)));*/

                //****************


                // Map style buttons
                var mapStyleStreetButton = new Button { Text = "Street", BackgroundColor = Color.Gray };
                var mapStyleHybridButton = new Button { Text = "Hybrid", BackgroundColor = Color.Gray };
                var mapStyleSatelliteButton = new Button { Text = "Satellite", BackgroundColor = Color.Gray };

                mapStyleStreetButton.Clicked += HandleClicked;
                mapStyleHybridButton.Clicked += HandleClicked;
                mapStyleSatelliteButton.Clicked += HandleClicked;

                // Mousemode buttons
                var mouseModeLockButton = new Button { Text = "Lock Cursor", BackgroundColor = Color.Gray };
                var mouseModeAddWaypointsButton = new Button { Text = "Add Waypoints", BackgroundColor = Color.Gray };
                var mouseModeSelfLocationButton = new Button { Text = "Set Self Location", BackgroundColor = Color.Gray };
                var mouseModeDroneLocationButton = new Button { Text = "Set Drone Location", BackgroundColor = Color.Gray };

                mouseModeLockButton.Clicked += HandleClicked;
                mouseModeAddWaypointsButton.Clicked += HandleClicked;
                mouseModeSelfLocationButton.Clicked += HandleClicked;
                mouseModeDroneLocationButton.Clicked += HandleClicked;

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

                // Tools buttons
                var preFetchMapSelectBoundariesButton = new Button { Text = "Select Center", BackgroundColor = Color.Gray };
                var preFetchMapFetchButton = new Button { Text = "Fetch Map", BackgroundColor = Color.Gray };

                preFetchMapSelectBoundariesButton.Clicked += HandleClicked;
                preFetchMapFetchButton.Clicked += HandleClicked;

                // Coordinates Display
                _StatusLatLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusLongLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusAltLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };
                _StatusLandedLabel = new Label { Text = "---", TextColor = Color.Red, FontSize = 20 };

                // Popup stacks
                _mapStyleStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { mapStyleStreetButton, mapStyleHybridButton,
                        mapStyleSatelliteButton }
                };

                _mouseModeStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { mouseModeLockButton, mouseModeAddWaypointsButton, 
                        mouseModeSelfLocationButton, mouseModeDroneLocationButton }
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

                _toolsStack = new StackLayout
                {
                    IsVisible = false,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Orientation = StackOrientation.Horizontal,
                    Children = { preFetchMapSelectBoundariesButton, preFetchMapFetchButton, _ToolsLabel }
                };

                // Bottom Tray

                _MessageLabel = new Label
                {
                    Text = "Hi",
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                /*_ToolsLabel = new Label
                {
                    Text = "---",
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };*/

                ImageButton missionButton = new ImageButton
                {
                    Source = "mission.png",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.White
                };
                ImageButton planButton = new ImageButton
                {
                    Source = "Path.png",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.White
                };
                ImageButton mousemodeButton = new ImageButton
                {
                    //Source = "adjust.png",
                    Source = "CursorMode.png",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.White
                };
                ImageButton mapStyleButton = new ImageButton
                {
                    Source = "MapLayers.png",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.White
                };
                ImageButton toolsButton = new ImageButton
                {
                    Source = "cog.png",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    BackgroundColor = Color.White
                };

                missionButton.Clicked += (sender, args) =>
                { _mapStyleStack.IsVisible = false; _missionStack.IsVisible = !_missionStack.IsVisible; _mouseModeStack.IsVisible = false; _toolsStack.IsVisible = false; };
                planButton.Clicked += (sender, args) =>
                { _planStack.IsVisible = !_planStack.IsVisible; };
                mapStyleButton.Clicked += (sender, args) =>
                { _missionStack.IsVisible = false; _mapStyleStack.IsVisible = !_mapStyleStack.IsVisible; _mouseModeStack.IsVisible = false; _toolsStack.IsVisible = false; };
                mousemodeButton.Clicked += (sender, args) =>
                { _mapStyleStack.IsVisible = false; _missionStack.IsVisible = false; _mouseModeStack.IsVisible = !_mouseModeStack.IsVisible; _toolsStack.IsVisible = false; };
                toolsButton.Clicked += (sender, args) =>
                { _mapStyleStack.IsVisible = false; _missionStack.IsVisible = false; _mouseModeStack.IsVisible = false; _toolsStack.IsVisible = !_toolsStack.IsVisible; };

                var bottomTray = new StackLayout
                {
                    Spacing = 11,
                    HorizontalOptions = LayoutOptions.Fill,
                    Orientation = StackOrientation.Horizontal,
                    Children = { missionButton, planButton, _MessageLabel, mapStyleButton, mousemodeButton, toolsButton }
                };

                var MapPlanstack = new StackLayout { Spacing = 0, Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.FillAndExpand };
                MapPlanstack.Children.Add(_map);
                MapPlanstack.Children.Add(_planStack);

                var stack = new StackLayout { Spacing = 0 };

                stack.Children.Add(new StatusBar(this));
                stack.Children.Add(MapPlanstack);
                stack.Children.Add(_progressBar);
                stack.Children.Add(_mapStyleStack);
                stack.Children.Add(_mouseModeStack);
                stack.Children.Add(_missionStack);
                stack.Children.Add(_toolsStack);

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

                //_map.TestPologon();

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

        private bool _mapReady = false;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void MapOnOnMapReady(object sender, EventArgs e)
        {
            if (_mapReady)
                return;

            _mapReady = true;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(ShowCurrentPositionOnMap);
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************

        private void OnMapClick(object sender, OnMapClickEventArgs e)
        {
            switch (_mapClickMode)
            {
                case MapClickModeEnum.AddWaypoint:
                    AddWaypoint(e.Lat, e.Lon, e.Alt);
                    break;
                case MapClickModeEnum.Locked:
                    return;
                    break;
                case MapClickModeEnum.SetSelfPosition:
                    SetSelfPosition(e.Lat, e.Lon, e.Alt);
                    _mapClickMode = MapClickModeEnum.AddWaypoint; //TODO
                    break;
                case MapClickModeEnum.SetDronePosition:
                    SetDronePosition(e.Lat, e.Lon, e.Alt);
                    _mapClickMode = MapClickModeEnum.AddWaypoint; //TODO
                    break;
                case MapClickModeEnum.PrefetchSetCenter:
                    PrefetchSetCenter(e.Lat, e.Lon, e.Alt);
                    break;
                case MapClickModeEnum.Unknown:
                    return;
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        //*********************************************************************

        private async void SetSelfPosition(double lat, double lon, double alt)
        {
            var sensorPos = await Services.Geolocation.GetCurrentPosition();

            SetSelfObject("self", lat, lon, alt,
                sensorPos.Latitude, sensorPos.Longitude, sensorPos.Altitude);

            // recenter map
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(lat, lon), Distance.FromMiles(0.3)));
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        //*********************************************************************

        private void SetDronePosition(double lat, double lon, double alt)
        {
            //leave if we dont have a drone
            if (null == _singleTrackedObject)
                return;

            //calculate offset
            _singleTrackedObject.PositionOffset = new Position(
                lat - _singleTrackedObject.PositionFromSensor.Latitude, 
                lon - _singleTrackedObject.PositionFromSensor.Longitude);

            //update object position
            _singleTrackedObject.Position = new Position(lat,lon);

            //tell map to move it now (to make UI more responsive)
            _map.Change = new ChangeHappened(_singleTrackedObject,
                ChangeHappened.ChangeTypeEnum.Changed);
        }

        //*********************************************************************
        /// <summary>
        /// Set a center for tile fetch, show the tile boundaries on the map
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        //*********************************************************************
        private async void PrefetchSetCenter(double lat, double lon, double alt)
        {
            _prefetchStatus = new PrefetchStatus
            {
                TotalTileCount =
                (int)Math.Pow(_prefetchEdgeCount + ((_prefetchEdgeCount % 2 == 0) ? 1 : 0), 2)
            };

            _prefetchCenter = new WorldCoordinate() 
                { Lat = (float)lat, Lon = (float)lon };

            GeoTileService gts = new GeoTileService();

            _map.GeoTileList = gts.FetchTileInfo(
                (float)lat, (float)lon, _prefetchZoom, _prefetchEdgeCount);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                _ToolsLabel.Text =
                    $"Tile Count: {_prefetchStatus.TotalTileCount.ToString()}");
        }

        //*********************************************************************
        /// <summary>
        /// Prefetch the tiles selected in PrefetchSetCenter()
        /// </summary>
        //*********************************************************************
        private async void PreFetchFetchMap()
        {
            _mapClickMode = MapClickModeEnum.Locked;

            GeoTileService gts = new GeoTileService();

            await gts.PreFetchMap(_prefetchCenter.Lat, _prefetchCenter.Lon,
                _prefetchZoom, _prefetchEdgeCount, 
                (fetchStatus) => PrefetchStatusUpdate(fetchStatus));

            //remove the tile polys from the map 
            _map.GeoTileList = null;
        }

        //*********************************************************************
        /// <summary>
        /// Show a running update of map tile fetch
        /// </summary>
        /// <param name="statusUpdate"></param>
        //*********************************************************************
        private void PrefetchStatusUpdate(TileServerLib.FetchStatus statusUpdate)
        {
            switch (statusUpdate.DataType)
            {
                case TileServerLib.FetchStatus.DataTypeEnum.Elevation:
                    if (statusUpdate.Result == TileServerLib.FetchStatus.ResultEnum.Success)
                        _prefetchStatus.ElevationSuccessCount++;
                    else
                        _prefetchStatus.ElevationFailureCount++;
                    break;
                case TileServerLib.FetchStatus.DataTypeEnum.Image:
                    if (statusUpdate.Result == TileServerLib.FetchStatus.ResultEnum.Success)
                        _prefetchStatus.ImageSuccessCount++;
                    else
                        _prefetchStatus.ImageFailureCount++;
                    break;
                case TileServerLib.FetchStatus.DataTypeEnum.Map:
                    if (statusUpdate.Result == TileServerLib.FetchStatus.ResultEnum.Success)
                        _prefetchStatus.MapSuccessCount++;
                    else
                        _prefetchStatus.MapFailureCount++;
                    break;
            }

            _prefetchStatus.TotalFetchedOrFailed++;
            _progressBar.ProgressTo(
                _prefetchStatus.TotalFetchedOrFailed / (2 * _prefetchStatus.TotalTileCount), 
                _progressBarAnimationTime, Easing.SinOut);

            //All done
            if(_prefetchStatus.TotalFetchedOrFailed == (2 * _prefetchStatus.TotalTileCount))
            {
                if (0 < (_prefetchStatus.ImageFailureCount + _prefetchStatus.ElevationFailureCount))
                    _progressBar.ProgressColor = Color.Red;
                else
                    _progressBar.ProgressColor = Color.Green;
            }
            else
                _progressBar.ProgressColor = Color.Yellow;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                _ToolsLabel.Text =
                    $"Tile Count: {_prefetchStatus.TotalTileCount.ToString()}, " +
                    $"Image OK: {_prefetchStatus.ImageSuccessCount.ToString()}, " +
                    $"Image Fail: {_prefetchStatus.ImageFailureCount.ToString()}, " +
                    $"Ele OK: {_prefetchStatus.ElevationSuccessCount.ToString()}, " +
                    $"Ele Fail: {_prefetchStatus.ElevationFailureCount.ToString()}");
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        //*********************************************************************

        private void AddWaypoint(double lat, double lon, double alt)
        {
            var wayPoint = new Waypoint
            {
                Type = PinType.Place,
                Position = new Position(lat, lon),
                Label = "Waypoint",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Waypoint",
                Url = "http://www.telemething.com/",
                IsActive = true
            };

            _map.AddWaypoint(wayPoint);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () => _missionCtrl.AddWaypoint(wayPoint));
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        //*********************************************************************

        private SelfObject SetSelfObject(string uniqueId,
            double lat, double lon, double alt,
            double sensorlat, double sensorlon, double sensorAlt)
        {
            SelfObject to = new SelfObject()
            {
                Type = PinType.Generic,
                Position = new Position(lat, lon),
                Label = "Self",
                Address = $"Lat: {lat}, Lon: {lon}, alt: {alt}",
                Id = "Self",
                Url = "http://www.telemething.com/",
                UniqueId = uniqueId,
                PositionFromSensor = new Position(sensorlat, sensorlon),
                PositionOffset = new Position(sensorlat - lat, sensorlon - lon)
            };

            _map.SetSelfObject(to);

            return to;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //*********************************************************************

        void HandleClicked(object sender, EventArgs e)
        {
            _missionStack.IsVisible = false;
            _mapStyleStack.IsVisible = false;

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
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => StartTelemetry(_rosBridgeUri));
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
                case "Lock Cursor":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(LockCursor);
                    break;
                case "Add Waypoints":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(AddWaypoints);
                    break;
                case "Set Self Location":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(SetSelfLocation);
                    break;
                case "Set Drone Location":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(SetDroneLocation);
                    break;

                case "Select Center":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(PreFetchSetCenter);
                    break;
                case "Fetch Map":
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(PreFetchFetchMap);
                    break;
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void ShowCurrentPositionOnMap()
        {
            Plugin.Geolocator.Abstractions.Position pos;

            try
            {
                pos = await Services.Geolocation.GetCurrentPosition();

                SetSelfObject("self",
                    pos.Latitude, pos.Longitude, pos.Altitude,
                    pos.Latitude, pos.Longitude, pos.Altitude);

                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(pos.Latitude, pos.Longitude), Distance.FromMiles(0.3)));
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + ex.Message, "Ok");
                Console.WriteLine(ex);
                return;
            }

        }

        //*********************************************************************
        /// <summary>
        /// Request permissions from the OS 
        /// </summary>
        /// <returns></returns>
        //*********************************************************************

        private async Task<bool> GetPermissions()
        {
            bool permissionsGranted = true;
            var status = Plugin.Permissions.Abstractions.PermissionStatus.Unknown;

            try
            {
                status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();

                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    status = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
                    await DisplayAlert("Permission Request", "Location: " + status.ToString(), "OK");
                }

                status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationWhenInUsePermission>();

                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    status = await CrossPermissions.Current.RequestPermissionAsync<LocationWhenInUsePermission>();
                    await DisplayAlert("Permission Request", "Location When In Use: " + status.ToString(), "OK");
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


        private async Task<bool> GetPermissionsOld(List<Permission> permissionsList)
        {
            bool permissionsGranted = true;
            try
            {
                foreach (var permission in permissionsList)
                {
                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);

                    if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
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

                        if (!(status == Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
                            status == Plugin.Permissions.Abstractions.PermissionStatus.Unknown))
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

        Timer _wsApiTestTimer;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private void StartHeartbeatTimer()
        {
            object obj = null;
            _heartbeatTimer = new Timer(HeartbeatTimerCallback, obj,
                new TimeSpan(0, 0, 0, 0),
                new TimeSpan(0, 0, 0, _heartbeatPeriodSeconds));

            if (_runWacLoopbackTest)
            {
                _wsApiTestTimer = new Timer(WsApiTestTimerCallback, obj,
                    new TimeSpan(0, 0, 0, 10),
                    new TimeSpan(0, 0, 0, 10));
            }

            if (_sendSelfTelem)
            {
                _selfTelemTimer = new Timer(SelfTelemTimerCallback, obj,
                    new TimeSpan(0, 0, 0, 0),
                    new TimeSpan(0, 0, 0, _selfTelemPeriodSeconds));
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private Plugin.Geolocator.Abstractions.Position FetchAdjustedSelfPosition()
        {
            var position = Services.Geolocation.GetCurrentPosition().Result;

            if (null != _map) if (null != _map.SelfObject)
                    if (null != _map.SelfObject.PositionOffset)
                    {
                        position.Latitude += _map.SelfObject.PositionOffset.Latitude;
                        position.Longitude += _map.SelfObject.PositionOffset.Longitude;
                    }

            return position;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        //*********************************************************************

        private void HeartbeatTimerCallback(object state)
        {
            double lat = 0, lon = 0;
            try
            {
                _myPosition = FetchAdjustedSelfPosition();
                lat = _myPosition.Latitude;
                lon = _myPosition.Longitude;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (Xamarin.Essentials.DeviceInfo.DeviceType == DeviceType.Virtual &&
                    Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    Console.WriteLine("Forgot to set location in iOS simulator");
                    lat = 47.468502;
                    lon = -121.7674;
                }
                else
                    return;
            }

            //SendPositionUpdate(lat, lon);
        }

        //*********************************************************************
        /// <summary>
        /// Test method, not for production
        /// </summary>
        /// <param name="state"></param>
        //*********************************************************************

        private async void WsApiTestTimerCallback(object state)
        {
            Guid reqGuid = new Guid();
            Guid respGuid = new Guid();

            try
            {
                var req = new WebApiLib.Request("Test1", new System.Collections.Generic.List<WebApiLib.Argument>());

                reqGuid = req.GUID;

                var resp = await wac.Invoke(req);

                //var resp = await wac.Invoke("method1", new System.Collections.Generic.List<WebApiLib.Argument>());

                if (resp.Result != WebApiLib.ResultEnum.ok)
                {
                    if (resp.Result == WebApiLib.ResultEnum.exception)
                        throw new Exception("Test Failed : " + resp.Exception?.Message);
                    if (resp.Result == WebApiLib.ResultEnum.notfound)
                        throw new Exception("Test Failed : not found");
                    if (resp.Result == WebApiLib.ResultEnum.timeout)
                        throw new Exception("Test Failed : timeout");
                    if (resp.Result == WebApiLib.ResultEnum.unknown)
                        throw new Exception("Test Failed : unknown");
                }

                respGuid = resp.RequestGUID;

                Debug.WriteLine("WsApiTestTimerCallback() : Req: " + reqGuid.ToString() + " Resp: " + respGuid.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine("WsApiTestTimerCallback() : Req: " + reqGuid.ToString() + " Exception : " + e.Message);
            }
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        //*********************************************************************

        private void SelfTelemTimerCallback(object state)
        {
            try
            {
                _myPosition = FetchAdjustedSelfPosition();

                _telemetryRepeater?.Send(new TThingComLib.Messages.Message(
                    MessageTypeEnum.Telem, "self", "*")
                {
                    Coord = new Coord(_myPosition.Latitude,
                    _myPosition.Longitude, _myPosition.Altitude)
                }, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //SendPositionUpdate(lat, lon);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        /*private async void ConnectToIotHub()
        {
            // missing AzureIotLib
            //_azureIotDevice = new AzureIotDevice();

            //await ait.GetDeviceTwinList();

            //await _azureIotDevice.ConnectToDevice(GotAzureDeviceMessageCallback);
        }*/

        //*********************************************************************
        ///
        /// <summary>
        /// Connect to a mavlink source
        /// </summary>
        /// <param name="rosBridgeUri">mavlink server uri</param>
        ///
        //*********************************************************************

        private async void ConnectToMav(string rosBridgeUri)
        {
            if (null == rosBridgeUri)
                rosBridgeUri = _rosBridgeUri;

            if (null == _rosClient)
                _rosClient = new RosClientLib.RosClient(rosBridgeUri,
                    OnConnected, OnConnectionFailed);
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

            /*Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () => App.Current.MainPage.DisplayAlert(
                    "Error", "Error: " + message, "Ok"));*/
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
                if (routeCoord.IsActive)
                    waypoints.AddWaypoint(Mavros.Command.NAV_WAYPOINT,
                        routeCoord.Position.Latitude, routeCoord.Position.Longitude, 30,
                        false, true, 5, 0, 0, 0);

            // return home and land
            waypoints.AddWaypoint(Mavros.Command.NAV_LAND,
                _missionStatus.x_lat, _missionStatus.y_long, 0, false,
                true, 5, 0, 0, 0);

            // send the mission to ROS
            ConnectToMav(_rosBridgeUri);
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

            ConnectToMav(_rosBridgeUri);
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
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void LockCursor()
        {
            _mapClickMode = MapClickModeEnum.Locked;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void AddWaypoints()
        {
            _mapClickMode = MapClickModeEnum.AddWaypoint;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void SetSelfLocation()
        {
            _mapClickMode = MapClickModeEnum.SetSelfPosition;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void SetDroneLocation()
        {
            _mapClickMode = MapClickModeEnum.SetDronePosition;
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        private async void PreFetchSetCenter()
        {
            _mapClickMode = MapClickModeEnum.PrefetchSetCenter;
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rosBridgeUri">ROS bridge server uri</param>
        //*********************************************************************

        public async void StartTelemetry(string rosBridgeUri)
        {
            if (this.ConnectionState != ConnectionStateEnum.Connected)
                this.ConnectionState = ConnectionStateEnum.Connecting;

            _moveMapToTrackedObject = true;

            ConnectToMav(rosBridgeUri);

            var subscriptionId = _rosClient.Subscribe
                <RosSharp.RosBridgeClient.Messages.Test.MissionStatus>(
                    "/tt_mavros_wp_mission/MissionStatus",
                    TelemetrySubscriptionHandler);

            //rosClient.Unsubscribe(subscriptionId);
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsIn"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="missionStatus"></param>
        //*********************************************************************

        private void TelemetrySubscriptionHandler(MissionStatus missionStatus)
        {
            // do something to prevent high frequency updating

            _missionStatus = missionStatus;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    _telemetryRepeater.Send(missionStatus, true);
                    _StatusLatLabel.Text = $"Lat: {missionStatus.x_lat}";
                    _StatusLongLabel.Text = $"Lon: {missionStatus.y_long}";
                    _StatusAltLabel.Text = $"Alt: {Math.Round(missionStatus.z_alt, 2)}";

                    SetLandedState(missionStatus.landed_state);

                    ShowTrackedObjectLocation(
                        missionStatus.x_lat, missionStatus.y_long, 2);
                });
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        //*********************************************************************

        public async void SendPositionUpdate(double lat, double lon)
        {
            //_azureIotDevice.SendD2C($"{{\"state\":{{\"reported\":{{\"lat\":\"{lat}\",\"lon\":\"{lon}\"}}}}}}");

            // missing AzureIotLib
            //_azureIotDevice.SendD2C($"{{\"payload_fields\":{{\"latitude\":\"{lat}\",\"longitude\":\"{lon}\"}}}}");
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="minDelta"></param>
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

                var positionFromSensor = new Position(lat, lon); 

                if (null != _singleTrackedObject)
                if (null != _singleTrackedObject.PositionOffset)
                {
                    lat += _singleTrackedObject.PositionOffset.Latitude;
                    lon += _singleTrackedObject.PositionOffset.Longitude;
                }

                var position = new Position(lat, lon); // Latitude, Longitude

                if (null == _singleTrackedObject)
                {
                    _singleTrackedObject = AddTrackedObject(
                        "singleTrackedObject", lat, lon, 0);
                    _singleTrackedObject.PositionFromSensor =
                        positionFromSensor;
                }
                else
                {
                    _singleTrackedObject.PositionFromSensor = positionFromSensor;
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

        /*private void GotAzureDeviceMessageCallback(byte[] messagepayload)
        {
            if (null == messagepayload)
                return;

            string strData = Encoding.UTF8.GetString(messagepayload);

            dynamic jsonObj = JsonConvert.DeserializeObject(strData);

            // missing AzureIotLib
            //string latS = jsonObj.a;
            //string lonS = jsonObj.o;

            //var lat = Convert.ToDouble(latS);
            //var lon = Convert.ToDouble(lonS);

            //Xamarin.Forms.Device.BeginInvokeOnMainThread(() => { ShowTrackedObjectLocation(lat, lon); });
        }*/

        #region Tests

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        /*private void BuildCustomPins()
        {
            var pin = new Waypoint
            {
                Type = PinType.Place,
                Position = new Position(37.79752, -122.40183),
                Label = "Xamarin San Francisco Office",
                Address = "394 Pacific Ave, San Francisco CA",
                Id = "Xamarin",
                Url = "http://xamarin.com/about/",
                IsActive = true
            };

            _map.Waypoints = new List<Waypoint> { pin };
        }*/

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        /*private void TestDropCustomPin()
        {
            _map.Pins.Add(_map.Waypoints.First());
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(37.79752, -122.40183), 
                Distance.FromMiles(1.0)));
        }*/

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        /*private void TestDrawPolyline()
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
        }*/

        #endregion
    }
}