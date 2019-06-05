using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Forms;
//using OpenTK.Graphics.ES30;
//using OpenTK;
//using OpenTK.Graphics;
//using OpenTK.Graphics.OpenGL;
//using OpenTK.Input;

#if __ANDROID__
	using Android.Util;
	using Android.App;
	using Android.Opengl;
	using Android.Graphics;
	using Android;
#elif __IOS__
	using UIKit;
	using Foundation;
	using CoreGraphics;
#endif    

namespace TTMobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OpenGlTest : ContentPage
    {
        float red, green, blue;
        private OpenGLView view;
        private Switch toggle;
        private Button button;

        void myButtonClicked()
        {
            view.Display();
        }

        public OpenGlTest()
        {
           //InitializeComponent();
           
           Title = "OpenGL";
           view = new OpenGLView { HasRenderLoop = true };
           toggle = new Switch { IsToggled = true };
           button = new Button { Text = "Display" };

           //view.On

           view.HeightRequest = 300;
           view.WidthRequest = 300;

           view.OnDisplay = r => {

               //GL.ClearColor(red, green, blue, 1.0f);
               //GL.Clear((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

               red += 0.01f;
               if (red >= 1.0f)
                   red -= 1.0f;
               green += 0.02f;
               if (green >= 1.0f)
                   green -= 1.0f;
               blue += 0.03f;
               if (blue >= 1.0f)
                   blue -= 1.0f;
           };

           toggle.Toggled += (s, a) => {
               view.HasRenderLoop = toggle.IsToggled;
           };

           //button.Clicked += (s, a) => view.Display();

           button.Clicked += (s, a) => myButtonClicked();

           var stack = new StackLayout
           {
               Padding = new Size(20, 20),
               Children = { view, toggle, button }
           };

           Content = stack;
        }
    }
}