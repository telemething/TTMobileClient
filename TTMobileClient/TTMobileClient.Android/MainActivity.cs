﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Permissions;

namespace TTMobileClient.Droid
{
    [Activity(Label = "TTMobileClient", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Activity = this;
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Octane.Xamarin.Forms.VideoPlayer.Android.FormsVideoPlayer.Init();
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            //Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // https://github.com/jamesmontemagno/PermissionsPlugin
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Callback for Plugin.Permissions
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        /// 
        //*********************************************************************

        /*public override void OnRequestPermissionsResult(int requestCode, 
            string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(
                requestCode, permissions, grantResults);
        }*/

        public override void OnRequestPermissionsResult(int requestCode,
            string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(
                requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /*public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }*/
    }

}
