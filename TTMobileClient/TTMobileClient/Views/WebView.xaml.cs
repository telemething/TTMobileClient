
using System;

namespace TTMobileClient.Views
{
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebView : ContentPage
    {
        private RosClientLib.IRosClient _rosClient = null;
        private ConnectionStateEnum _connectionState = ConnectionStateEnum.Unknown;
        string RosBridgeUrl = AppSettings.DefaultRobotRosbridgeUrl;
        string RosVideoUrl = AppSettings.DefaultRobotRosVideoUrl;

        const string EnvelopeString =
            "<html><head></head><body><img src=\"{0}\"></img></body></html>";

        const string RosImageStreamUrlFormat =
            "{0}/stream?topic={1}";

        const string ImageTypeName = "sensor_msgs/Image";

        private string _imageUrl;

        public string ImageUrl
        {
            set => _imageUrl = value;
            get => _imageUrl;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// ROS Web_Video_Server
        /// http://wiki.ros.org/web_video_server
        /// Xamarin Forms WebView :
        /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/webview?tabs=windows
        /// </summary>
        ///
        //*********************************************************************

        public WebView()
        {
            InitializeComponent();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="envelope"></param>
        ///
        //*********************************************************************

        public void DisplayImageStream(string imageUrl, bool envelope)
        {
            _imageUrl = imageUrl;

            if(envelope)
                ImageContainer.Source = new HtmlWebViewSource
                    { Html = string.Format(EnvelopeString, _imageUrl) };
            else
                ImageContainer.Source = imageUrl;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        protected override void OnAppearing()
        {
            bool envelope = true;

            UpdateTopicList();

            DisplayImageStream(envelope ?
                "http://192.168.1.30:8080/stream?topic=/airsim/image_raw" :
                "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4", envelope);
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

        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;

            if (picker.SelectedIndex == -1)
            {
                //TODO: message or not?
            }
            else
            {
                var topicName = picker.Items[picker.SelectedIndex];
                DisplayImageStream(
                    string.Format(RosImageStreamUrlFormat, RosVideoUrl, topicName), true);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void UpdateTopicList()
        {
            if (null == _rosClient)
                _rosClient = new RosClientLib.RosClient(RosBridgeUrl,
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

            if (e is RosClientLib.ConnectionEventArgs tt)
                message = tt.message;

            this._connectionState = ConnectionStateEnum.Failed;

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

            this._connectionState = ConnectionStateEnum.Connected;

            _rosClient.FetchTopicList(ImageTypeName,
                resp =>
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        {
                            if (resp.success)
                            {
                                TopicPicker.Items.Clear();

                                foreach (var tpc in resp.rosTopics)
                                    TopicPicker.Items.Add(tpc.topic);
                            }
                            else
                            {
                                App.Current.MainPage.DisplayAlert(
                                    "Error", "Unable to fetch image topic list", "Ok");
                            }
                        }
                    );
                });

        }


    }
}