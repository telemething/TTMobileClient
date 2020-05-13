using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TTMobileClient.Services;
using TTMobileClient.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TTMobileClient
{
    public partial class App : Application
    {
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
        public static string AzureBackendUrl = "http://localhost:5000";
        public static bool UseMockDataStore = true;

        TTMobileClient.Services.ApiService _was = null;
        TTMobileClient.AppSettings _appSettings = null;
        TTMobileClient.GeoTileService _geoTileService = null;
        AdvertiseServices _as = null;

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        public App()
        {
            InitializeComponent();

            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            MainPage = new MainPage();

            StartServices();
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        protected void StartServices()
        {
            //we start the API service, listening for requets from remote devices
            _was = new TTMobileClient.Services.ApiService();
            _was.StartService();

            //we start the AppSettings here so that it registers a callback with
            //the API service, listening for requests from remote devices
            _appSettings = new AppSettings();

            //we start the GeoTileService here so that it registers a callback with
            //the API service, listening for requests from remote devices
            _geoTileService = new GeoTileService();

            _geoTileService.FetchTest();

            //we start the advertising service, informing remote devices of the
            //services we offer
            //_as = new AdvertiseServices();
            _as = AdvertiseServices.Singleton;
            _as.StartAdvertising();
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        protected override void OnStart()
        {
            AppCenter.Start("ios=38ab36c2-6c0a-4104-8232-44b41526c37b;" +
                  "uwp={Your UWP App secret here};" +
                  "android={Your Android App secret here}",
                  typeof(Analytics), typeof(Crashes));
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        //*********************************************************************
        /// <summary>
        /// 
        /// </summary>
        //*********************************************************************

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
