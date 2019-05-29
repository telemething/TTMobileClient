using System;
using System.Collections.Generic;
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

        async void OnCallRosService(object sender, EventArgs args)
        {
            FetchTopicList();

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
    }
}