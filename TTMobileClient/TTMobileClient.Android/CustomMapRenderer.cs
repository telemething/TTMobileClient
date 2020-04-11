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
        List<Waypoint> _waypoints;
        List<TrackedObject> _trackedObjects;
        List<Position> routeCoordinates;
        SelfObject _selfObject;
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
            _waypoints = new List<Waypoint>(8);
            _trackedObjects = new List<TrackedObject>(1);
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
                var newObject = formsMap.change.SubjectObject;

                if (newObject is Waypoint newPin)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            _waypoints.Add(newPin);
                            formsMap.Pins.Add(newPin);
                            Control.GetMapAsync(this);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Removed:
                            //_waypoints.Remove(newPin);
                            newPin.IsActive = false;
                            formsMap.Pins.Remove(newPin);
                            break;
                    }

                if (newObject is TrackedObject to)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddTrackedObject(to, formsMap);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Changed:
                            ChangeTrackedObject(to);
                            break;
                    }

                if (newObject is SelfObject so)
                    switch (formsMap.change.ChangeType)
                    {
                        case ChangeHappened.ChangeTypeEnum.Added:
                            AddSelfObject(so, formsMap);
                            break;
                        case ChangeHappened.ChangeTypeEnum.Changed:
                            ChangeSelfObject(so);
                            break;
                    }
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="formsMap"></param>
        ///
        //*********************************************************************

        private void AddSelfObject(SelfObject so, CustomMap formsMap)
        {
            _selfObject = so;
            formsMap.Pins.Add(so);
            Control.GetMapAsync(this);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="changedTO"></param>
        ///
        //*********************************************************************

        private void ChangeSelfObject(SelfObject changedTO)
        {
            _selfObject.Position = new Position(
                changedTO.Position.Latitude, changedTO.Position.Longitude);
            _selfObject.PositionFromSensor = new Position(
                changedTO.PositionFromSensor.Latitude, changedTO.PositionFromSensor.Longitude);
            _selfObject.PositionOffset = new Position(
                changedTO.PositionOffset.Latitude, changedTO.PositionOffset.Longitude);

            Control.GetMapAsync(this);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="formsMap"></param>
        ///
        //*********************************************************************

        private void AddTrackedObject(TrackedObject to, CustomMap formsMap)
        {
            _trackedObjects.Add(to);
            formsMap.Pins.Add(to);
            Control.GetMapAsync(this);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="changedTO"></param>
        ///
        //*********************************************************************

        private void ChangeTrackedObject(TrackedObject changedTO)
        {
            var trackedObject = GetTrackedObject(changedTO.UniqueId);

            trackedObject.Position = new Position(
                changedTO.Position.Latitude, changedTO.Position.Longitude);

            Control.GetMapAsync(this);
        }

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
                _waypoints = formsMap.Waypoints;
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

            var formsMap = Element as CustomMap;

            formsMap?.MapReadyCallback();

            if (0 < routeCoordinates?.Count)
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
            marker.SetPosition(new LatLng(
                pin.Position.Latitude, pin.Position.Longitude));

            if (pin is TrackedObject to)
            {
                marker.SetIcon(BitmapDescriptorFactory.
                    FromResource(Resource.Drawable.uav));
            }

            if (pin is SelfObject so)
            {
                marker.SetIcon(BitmapDescriptorFactory.
                    FromResource(Resource.Drawable.self));
            }

            else if (pin is Waypoint waypoint)
            {
                marker.SetTitle(pin.Label);
                marker.SetSnippet(pin.Address);
                marker.SetIcon(BitmapDescriptorFactory.
                    FromResource(Resource.Drawable.pin));
            }

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
            var customPin = GetWaypoint(e.Marker);
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

                var customPin = GetWaypoint(marker);
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

        Waypoint GetWaypoint(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, 
                annotation.Position.Longitude);

            foreach (var pin in _waypoints)
                if (pin.Position == position)
                    return pin;

            return null;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        TrackedObject GetTrackedObject(string id)
        {
            foreach (var to in _trackedObjects)
                if (to.UniqueId.Equals(id))
                    return to;

            return null;
        }

    }
}