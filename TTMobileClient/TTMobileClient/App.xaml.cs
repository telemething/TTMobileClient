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

        WebServerLib.TTWebSocketServer WSS = null;
        WebServerLib.TTWebServer TS = null;
        WebServerLib.TileClient TC = null;
        WebServerLib.TTWebSocketClient sc;
        WebApiLib.WebApiClient wac = null;

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
            WSS = new WebServerLib.TTWebSocketServer();
            WSS.StartServer();

            //wac = new WebApiLib.WebApiClient();
            //wac.Connect("ws://localhost:8877/chat");
            //InvokeTest();

            //sc = new WebServerLib.TTWebSocketClient();
            //sc.Connect();
            //sc.Send("hi");

            //start web server
            //TS = new WebServerLib.TTWebServer();
            //TS.StartServer();

            //web server client test
            //TC = new WebServerLib.TileClient();
            //TC.Test();

            AdvertiseServices AS = new AdvertiseServices();
            AS.StartAdvertising();
        }

        private async void InvokeTest()
        {
            var resp = await wac.Invoke("method1", new System.Collections.Generic.List<WebApiLib.Argument>());
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
