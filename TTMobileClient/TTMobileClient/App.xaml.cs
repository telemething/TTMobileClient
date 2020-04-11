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

        public App()
        {
            InitializeComponent();

            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            /*System.Threading.Tasks.Task.Factory.StartNew(async () =>
            {
                using (var server = new WebServer(HttpListenerMode.EmbedIO, "http://*:8080"))
                {
                    System.Reflection.Assembly assembly = typeof(App).Assembly;
                    server.WithLocalSessionManager();
                    server.WithWebApi("/api", m => m.WithController(() => new TestController()));
                    server.WithEmbeddedResources("/", assembly, "EmbedIO.Forms.Sample.html");
                    await server.RunAsync();
                }
            });*/

            WebServerLib.TTWebServer TS = new WebServerLib.TTWebServer();
            TS.StartServer();

            WebServerLib.TileClient TC = new WebServerLib.TileClient();
            TC.Test();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            AppCenter.Start("ios=38ab36c2-6c0a-4104-8232-44b41526c37b;" +
                  "uwp={Your UWP App secret here};" +
                  "android={Your Android App secret here}",
                  typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
