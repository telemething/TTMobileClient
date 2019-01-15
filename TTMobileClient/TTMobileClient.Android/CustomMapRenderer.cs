using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using TTMobileClient;
using TTMobileClient.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace TTMobileClient.Droid
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        List<Waypoint> customPins;
        List<Position> routeCoordinates;
        private bool _viewingPinInfo = false;

        //*********************************************************************
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        ///
        //*********************************************************************

        public CustomMapRenderer(Context context) : base(context)
        {
        }

        //*********************************************************************
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        protected override void OnElementPropertyChanged(
            object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName.Equals("RouteCoordinates"))
            {
                var formsMap = (CustomMap)sender;
                routeCoordinates = formsMap.RouteCoordinates;
                Control.GetMapAsync(this);
            }

            if (e.PropertyName.Equals("Change"))
            {
                var formsMap = (CustomMap)sender;
                var newObject = formsMap.change.addedObject;


                if (newObject is Waypoint newPin)
                {
                    customPins.Add(newPin);
                    formsMap.Pins.Add(newPin);
                    Control.GetMapAsync(this);
                }
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPin"></param>
        ///
        //*********************************************************************

        /*private void AddPin(Waypoint newPin)
        {
            var snPoint = new Geopoint(
                new BasicGeoposition
                {
                    Latitude = newPin.Position.Latitude,
                    Longitude = newPin.Position.Longitude
                });

            var mapIcon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(
                    new Uri("ms-appx:///pin.png")),
                CollisionBehaviorDesired =
                    MapElementCollisionBehavior.RemainVisible,
                Location = snPoint,
                NormalizedAnchorPoint =
                    new Windows.Foundation.Point(0.5, 1.0)
            };

            nativeMap.MapElements.Add(mapIcon);
        }*/



        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        protected override void OnElementChanged(
            Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null)
            {
                var formsMap = (CustomMap)e.NewElement;
                customPins = formsMap.Waypoints;
                Control.GetMapAsync(this);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        ///
        //*********************************************************************

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            NativeMap.MapClick += NativeMapOnMapClick;
            NativeMap.InfoWindowClick += OnInfoWindowClick;
            NativeMap.SetInfoWindowAdapter(this);

            if(0 < routeCoordinates?.Count)
            {
                var polylineOptions = new PolylineOptions();
                polylineOptions.InvokeColor(0x66FF0000);

                foreach (var position in routeCoordinates)
                    polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));

                routeCoordinates.Clear();
                NativeMap.AddPolyline(polylineOptions);
            }
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

        private void NativeMapOnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            // if user is viewing pin info, then an outside click just clears
            // the pin info, and we don't drop a new pin

            if (_viewingPinInfo)
            {
                _viewingPinInfo = false;
                return;
            }

            var formsMap = Element as CustomMap;

            formsMap?.MapClickCallback(e.Point.Latitude, e.Point.Longitude );

            //((ExtMap)Element).OnTap(new Position(e.Point.Latitude, e.Point.Longitude));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);
            marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
            return marker;
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

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            if (!string.IsNullOrWhiteSpace(customPin.Url))
            {
                var url = Android.Net.Uri.Parse(customPin.Url);
                var intent = new Intent(Intent.ActionView, url);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        public Android.Views.View GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(
                Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null)
            {
                Android.Views.View view;

                var customPin = GetCustomPin(marker);
                if (customPin == null)
                {
                    throw new Exception("Custom pin not found");
                }

                if (customPin.Id.ToString() == "Xamarin")
                {
                    view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
                }
                else
                {
                    view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                }

                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                if (infoTitle != null)
                {
                    infoTitle.Text = marker.Title;
                }
                if (infoSubtitle != null)
                {
                    infoSubtitle.Text = marker.Snippet;
                }

                _viewingPinInfo = true;
                return view;
            }
            return null;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        Waypoint GetCustomPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, 
                annotation.Position.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }
            return null;
        }
    }
}