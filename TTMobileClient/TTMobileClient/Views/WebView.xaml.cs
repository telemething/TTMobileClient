
namespace TTMobileClient.Views
{
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebView : ContentPage
    {
        const string _enveleopeString = 
            "<html><head></head><body><img src=\"{0}\"></img></body></html>";

        private string _imageUrl;

        public string ImageUrl
        {
            set => _imageUrl = value;
            get => _imageUrl;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
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
                    { Html = string.Format(_enveleopeString, _imageUrl) };
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

            DisplayImageStream(envelope ?
                "http://192.168.1.30:8080/stream?topic=/airsim/image_raw" :
                "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4", envelope);
        }
    }
}