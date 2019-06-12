using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Converters;
using RosClientLib;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RosBridgeTest : ContentPage
    {
        private IRosClient _rosClient = null;
        private ConnectionStateEnum _connectionState = ConnectionStateEnum.Unknown;
        string TestUri = "ws://192.168.1.30:9090";

        private ConnectionStateEnum ConnectionState
        {
            set
            {
                _connectionState = value;
                //UpdateMessageBox();
            }
            get => _connectionState;
        }


        public RosBridgeTest()
        {
            InitializeComponent();
        }

        async void OnCallFetchTopics(object sender, EventArgs args)
        {
            FetchTopicList();

            //RosClientLib.RosClient.WaypointTest();
            //await label.RelRotateTo(360, 1000);
        }

        async void OnCallSubscribePointCloud(object sender, EventArgs args)
        {
            SubscribeToPointCloud();

            //RosClientLib.RosClient.WaypointTest();
            //await label.RelRotateTo(360, 1000);
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
            if (null == _rosClient)
                _rosClient = new RosClientLib.RosClient(TestUri,
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

        private async void FetchTopicList()
        {
            if (this.ConnectionState != ConnectionStateEnum.Connected)
                this.ConnectionState = ConnectionStateEnum.Connecting;

            ConnectToMav();

            var topicDataTemplate = new DataTemplate(() =>
            {
                var grid = new Grid();

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.2, GridUnitType.Star) });

                var topicLable = new Label();
                var typeLable = new Label();

                topicLable.SetBinding(Label.TextProperty, "topic");
                typeLable.SetBinding(Label.TextProperty, "type");

                grid.Children.Add(topicLable);
                grid.Children.Add(typeLable, 1, 0);

                return new ViewCell { View = grid };
            });

            ItemsListView.HasUnevenRows = true;
            ItemsListView.ItemTemplate = topicDataTemplate;

            _rosClient.CallService<TopicList.TopicListReqResp>(new TopicList(),
                resp => {
                    if (resp.success)
                    {
                        resp.rosTopics.Sort((x,y) => string.Compare(x.topic, y.topic));
                        //this.MissionState = MissionStateEnum.Starting;
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() => ItemsListView.ItemsSource = resp.rosTopics);
                    }
                    else
                    {
                        //this.MissionState = MissionStateEnum.Failed;

                        //Xamarin.Forms.Device.BeginInvokeOnMainThread(
                        //    () => App.Current.MainPage.DisplayAlert(
                        //        "Error", "Unable to start mission", "Ok"));
                    }
                });


            //var subscriptionId = _rosClient.Subscribe
            //    <RosSharp.RosBridgeClient.Messages.Test.MissionStatus>(
            //        "/tt_mavros_wp_mission/MissionStatus",
            //        TelemetrySubscriptionHandler);

            ////rosClient.Unsubscribe(subscriptionId);
        }


        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private async void SubscribeToPointCloud()
        {
            ConnectToMav();

            var subscriptionId = _rosClient.Subscribe
                <RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2>(
                    "/rtabmap/octomap_occupied_space",
                    PointCloudSubscriptionHandler);

            //rosClient.Unsubscribe(subscriptionId);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct pcPoint
        {
            [FieldOffset(0)]
            unsafe public fixed byte data[32];

            [FieldOffset(0)]
            public float x;

            [FieldOffset(4)]
            public float y;

            [FieldOffset(8)]
            public float z;

            [FieldOffset(16)]
            public Byte r;

            [FieldOffset(17)]
            public Byte g;

            [FieldOffset(18)]
            public Byte b;

            [FieldOffset(19)]
            public Byte a;
        }

        List<pcPoint> pcPoints = new List<pcPoint>(100);

        //*********************************************************************
        ///
        /// <summary>
        /// For inspecting the PC data stream, not for production
        /// </summary>
        /// <param name="pc"></param>
        /// 
        //*********************************************************************

        private void DesrializePointCloud(RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc)
        {
            int index = 0;
            int length = 32;

            for (var outer = 0; outer < 20; outer++)
            {
                var pt = new pcPoint();

                unsafe
                {
                    for (int index2 = 0; index2 < length; index2++)
                        pt.data[index2] = pc.data[index + index2];
                }

                index += length;

                pcPoints.Add(pt);
            }
        }

        private void PackPointCloud(RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc)
        {
            int outBite = 0;
            int block1Length = 12;
            int block2Offset = 16;
            int block2Length = 4;

            for (int inBite = 0; inBite < pc.data.Length; inBite += (int)pc.point_step)
            {
                Buffer.BlockCopy(pc.data, inBite, pc.data, outBite, block1Length);
                Buffer.BlockCopy(pc.data, inBite + block2Offset, pc.data, outBite + block1Length, block2Length);
                outBite += block1Length + block2Length;
            }

            pc.row_step = (uint)outBite;
            pc.point_step = (uint)(block1Length + block2Length);

            foreach (var feeld in pc.fields)
                if (feeld.name.Equals("rgb"))
                {
                    feeld.offset = (uint)block1Length;
                    break;
                }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        /// 
        //*********************************************************************
        private int pointCloudMessageCount = 1;
        private long pointCloudAccumulatedSize = 0;

        private void PointCloudSubscriptionHandler(RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc)
        {

            pointCloudAccumulatedSize += pc.data.Length;

            PackPointCloud(pc);

            //DesrializePointCloud(pc);

            System.Diagnostics.Debug.WriteLine(
                "--------- PointCloud Data {0}, size: {1}, total: {2} ---------", 
                pointCloudMessageCount++, pc.data.Length, pointCloudAccumulatedSize);

            // do something to prevent high frequency updating

            /*_missionStatus = missionStatus;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    _StatusLatLabel.Text = $"Lat: {missionStatus.x_lat}";
                    _StatusLongLabel.Text = $"Lon: {missionStatus.y_long}";
                    _StatusAltLabel.Text = $"Alt: {Math.Round(missionStatus.z_alt, 2)}";

                    SetLandedState(missionStatus.landed_state);

                    ShowTrackedObjectLocation(
                        missionStatus.x_lat, missionStatus.y_long, 2);
                });*/
        }


    }
}