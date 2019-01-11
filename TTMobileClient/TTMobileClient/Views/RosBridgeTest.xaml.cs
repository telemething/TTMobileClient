using System;
using RosClientLib;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RosBridgeTest : ContentPage
    {
        public RosBridgeTest()
        {
            InitializeComponent();
        }

        async void OnCallRosService(object sender, EventArgs args)
        {
            RosClientLib.RosClient.WaypointTest();
            //await label.RelRotateTo(360, 1000);
        }
    }
}