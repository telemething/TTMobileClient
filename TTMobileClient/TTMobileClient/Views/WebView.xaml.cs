using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TTMobileClient.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class WebView : ContentPage
	{
		public WebView ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            //videoView.Source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
            //ImagePage.Source = "http://192.168.1.30:8080/stream_viewer?topic=/airsim/image_raw";

            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = @"<html><head><title>/airsim/image_raw</title></head><body><img src=""http://192.168.1.30:8080/stream?topic=/airsim/image_raw""></img></body></html>";
            ImagePage.Source = htmlSource;

        }
    }
}